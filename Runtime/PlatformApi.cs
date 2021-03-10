using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GamificationBackend
{
    public partial class PlatformManager
    {
        public class PlatformApi
        {
            private readonly string _gameToken;
            private readonly string _authURL;
            private readonly string _identifyURL;
            private readonly string _activityURL;
            private readonly string _registerURL;
            
            private string _personalToken;
            
            public PlatformApi(string host, string gameToken)
            {
                _gameToken = gameToken;
                _authURL = $"{host}/gamification/api/auth/";
                _identifyURL = $"{host}/gamification/api/game/identify/";
                _activityURL = $"{host}/gamification/api/game/<game_id>/activity/<campaign_id>/";
                _registerURL = $"{host}/gamification/api/register/";
            }
            
            #region API calls
            
            public IEnumerator BuildSession(string phone, string passwd, Action<PlatformResponse<PlaySession>> callback)
            {
                var requestPayload = new PayloadAuth
                {
                    game_token = _gameToken,
                    password = passwd,
                    phone = phone
                };
                var gameTokenPayload = new PayloadGameToken
                {
                    game_token = _gameToken
                };
                yield return PostData<PayloadAuth, PayloadToken>(_authURL, requestPayload);
                var tokenResponse = (PlatformResponse<PayloadToken>) responseCache;
                if (tokenResponse.status == RequestStatus.ERROR)
                {
                    callback(new PlatformResponse<PlaySession>
                    {
                        content = default,
                        error = tokenResponse.error,
                        status = RequestStatus.ERROR
                    });
                    yield break;
                }
                
                _personalToken = tokenResponse.content.token;
                yield return PostData<PayloadGameToken, PayloadGameDetail>(_identifyURL, gameTokenPayload);
                var gameDetail = (PlatformResponse<PayloadGameDetail>) responseCache;
                if (gameDetail.status == RequestStatus.ERROR)
                {
                    callback(new PlatformResponse<PlaySession>
                    {
                        content = default,
                        error = gameDetail.error,
                        status = RequestStatus.ERROR
                    });
                    yield break;
                }
                
                var url = _activityURL
                    .Replace("<game_id>", gameDetail.content.id.ToString())
                    .Replace("<campaign_id>", gameDetail.content.campaign.ToString());
                yield return GetData<PayloadActivityDetail>(url);
                var activityResponse = (PlatformResponse<PayloadActivityDetail>) responseCache;
                if (activityResponse.status == RequestStatus.ERROR)
                {
                    callback(new PlatformResponse<PlaySession>
                    {
                        content = default,
                        error = activityResponse.error,
                        status = RequestStatus.ERROR
                    });
                    yield break;
                }
                
                PlaySession session = new PlaySession
                {
                    status = activityResponse.content.status,
                    appToken = _gameToken,
                    campaignID = gameDetail.content.campaign,
                    gameID = gameDetail.content.id,
                    personalToken = tokenResponse.content.token,
                    player = activityResponse.content.player
                };
                callback(new PlatformResponse<PlaySession>
                {
                    content = session,
                    error = "",
                    status = RequestStatus.SUCCESS
                });
            }

            public IEnumerator UpdateActivityStatus(PlaySession session, int status, Action<PlatformResponse<bool>> callback)
            {
                var url = _activityURL
                    .Replace("<game_id>", session.gameID.ToString())
                    .Replace("<campaign_id>", session.campaignID.ToString());
                var payload = new PayloadActivityDetail
                {
                    game = session.gameID,
                    campaign = session.campaignID,
                    last_modified = "",
                    player = session.player,
                    status = status
                };
                yield return PostData<PayloadActivityDetail, PayloadActivityDetail>(url, payload);
                var activityResponse = (PlatformResponse<PayloadActivityDetail>) responseCache;
                if (activityResponse.status == RequestStatus.ERROR)
                {
                    callback(new PlatformResponse<bool>
                    {
                        content = default,
                        error = activityResponse.error,
                        status = RequestStatus.ERROR
                    });
                    yield break;
                }
                
                session.status = activityResponse.content.status;
                callback(new PlatformResponse<bool>
                {
                    content = activityResponse.content.status == status,
                    error = "",
                    status = RequestStatus.SUCCESS
                });
            }

            public IEnumerator RegisterPlayer(string firstName, string lastName,
                string company, string phone, string password, Action<PlatformResponse<bool>> callback)
            {
                var payload = new PayloadRegisterPlayer
                {
                    game_token = _gameToken,
                    first_name = firstName,
                    last_name = lastName,
                    company = company,
                    phone = phone,
                    password = password
                };
                yield return PostData<PayloadRegisterPlayer, PayloadPlayer>(_registerURL, payload);
                var playerRegistrationResponse = (PlatformResponse<PayloadPlayer>) responseCache;
                if (playerRegistrationResponse.status == RequestStatus.ERROR)
                {
                    callback(new PlatformResponse<bool>
                    {
                        content = default,
                        error = playerRegistrationResponse.error,
                        status = RequestStatus.ERROR
                    });
                    yield break;
                }
                
                callback(new PlatformResponse<bool>
                {
                    content = playerRegistrationResponse.content.id > 0,
                    error = "",
                    status = RequestStatus.SUCCESS
                });
            }
            
            #endregion

            #region Http handling
            
            private object responseCache;

            private T Deserialize<T>(string payload)
            where T : IBaseSerializable
            {
                var result = JsonUtility.FromJson<T>(payload);
                if (!result.check())
                {
                    throw new DeserializeException();
                }

                return result;
            }
            
            private IEnumerator PostData<T1, T2>(string url, T1 payload)
            where T1: IBaseSerializable
            where T2 : IBaseSerializable
            {
                Debug.Log("PostData: Posting new request: " + url);
                using (UnityWebRequest webRequest = UnityWebRequest.Put(url, JsonUtility.ToJson(payload)))
                {
                    webRequest.method = "POST";
                    webRequest.SetRequestHeader("Accept", "application/json");
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    if (!string.IsNullOrEmpty(_personalToken))
                    {
                        webRequest.SetRequestHeader("Authorization", "Token " + _personalToken);
                    }
                    
                    Debug.Log("PostData: Yielding until request is done");
                    yield return webRequest.SendWebRequest();
                    Debug.Log("PostData: Response received");

                    if (webRequest.isNetworkError)
                    {
                        Debug.Log("WebRequest failed: " + webRequest.error);
                        responseCache = new PlatformResponse<T2>
                        {
                            status = RequestStatus.ERROR,
                            error = "Could not connect to platform API",
                            content = default
                        };
                    }
                    else
                    {
                        Debug.Log("PostData: Request successful. Calling callback");
                        try
                        {
                            responseCache = new PlatformResponse<T2>
                            {
                                status = RequestStatus.SUCCESS,
                                error = "",
                                content = Deserialize<T2>(webRequest.downloadHandler.text)
                            };
                        }
                        catch (DeserializeException e)
                        {
                            responseCache = new PlatformResponse<T2>
                            {
                                status = RequestStatus.ERROR,
                                error = "Response did not match expected format",
                                content = default
                            };
                        }
                        catch (ArgumentException e)
                        {
                            responseCache = new PlatformResponse<T2>
                            {
                                status = RequestStatus.ERROR,
                                error = "Response did not match expected format",
                                content = default
                            };
                        }
                    }
                }
            }
            
            private IEnumerator GetData<T2>(string url)
            where T2 : IBaseSerializable
            {
                Debug.Log("PostData: Posting new request: " + url);
                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    webRequest.SetRequestHeader("Accept", "application/json");
                    // webRequest.SetRequestHeader("Content-Type", "application/json");
                    if (!string.IsNullOrEmpty(_personalToken))
                    {
                        webRequest.SetRequestHeader("Authorization", "Token " + _personalToken);
                    }
                    
                    Debug.Log("PostData: Yielding until request is done");
                    yield return webRequest.SendWebRequest();
                    Debug.Log("PostData: Response received");

                    if (webRequest.isNetworkError)
                    {
                        responseCache = new PlatformResponse<T2>
                        {
                            status = RequestStatus.ERROR,
                            error = "Could not connect to platform API",
                            content = default
                        };
                    }
                    else
                    {
                        try
                        {
                            responseCache = new PlatformResponse<T2>
                            {
                                status = RequestStatus.SUCCESS,
                                error = "",
                                content = Deserialize<T2>(webRequest.downloadHandler.text)
                            };
                        }
                        catch (DeserializeException e)
                        {
                            Debug.LogWarning("PostData: Got back unexpected format");
                            responseCache = new PlatformResponse<T2>
                            {
                                status = RequestStatus.ERROR,
                                error = "Response did not match expected format",
                                content = default
                            };
                        }
                        catch (ArgumentException e)
                        {
                            Debug.LogWarning("PostData: Got response, but it wasn't JSON");
                            responseCache = new PlatformResponse<T2>
                            {
                                status = RequestStatus.ERROR,
                                error = "Response did not match expected format",
                                content = default
                            };
                        }
                    }
                }
            }
            
            #endregion
        }

        public enum RequestStatus { ERROR, SUCCESS }

        public class PlatformResponse<T>
        {
            public RequestStatus status;
            public string error = "";
            public T content;
        }
    }
}