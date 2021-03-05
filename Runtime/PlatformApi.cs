using System;
using System.Collections;
using System.ComponentModel;
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
            
            private string _personalToken;
            
            public PlatformApi(string host, string gameToken)
            {
                _gameToken = gameToken;
                _authURL = $"{host}/gamification/api/auth/";
                _identifyURL = $"{host}/gamification/api/game/identify/";
                _activityURL = $"{host}/gamification/api/game/<game_id>/activity/<campaign_id>/";
            }
            
            #region API calls
            
            public IEnumerator BuildSession(string phone, string passwd, Action<PlaySession> callback)
            {
                var requestPayload = new PayloadAuth
                {
                    game_token = _gameToken,
                    password = passwd,
                    phone = phone
                };
                var gameTokenPayload = new PayloadToken
                {
                    token = _gameToken
                };
                yield return PostData<PayloadAuth, PayloadToken>(_authURL, requestPayload);
                var tokenResponse = (PayloadToken) responseCache;
                _personalToken = tokenResponse.token;
                yield return PostData<PayloadToken, PayloadGameDetail>(_identifyURL, gameTokenPayload);
                var gameDetail = (PayloadGameDetail) responseCache;
                var url = _activityURL
                    .Replace("<game_id>", gameDetail.id.ToString())
                    .Replace("<campaign_id>", gameDetail.campaign.ToString());
                yield return GetData<PayloadActivityDetail>(url);
                var activityResponse = (PayloadActivityDetail) responseCache;
                PlaySession session = new PlaySession
                {
                    status = activityResponse.status,
                    appToken = _gameToken,
                    campaignID = gameDetail.campaign,
                    gameID = gameDetail.id,
                    personalToken = tokenResponse.token,
                    player = activityResponse.player
                };
                callback(session);
            }

            public IEnumerator UpdateActivityStatus(PlaySession session, int status, Action<bool> callback)
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
                var activityResponse = (PayloadActivityDetail) responseCache;
                session.status = activityResponse.status;
                callback(activityResponse.status == status);
            }
            
            #endregion

            #region Http handling
            
            private object responseCache;
            
            private IEnumerator PostData<T1, T2>(string url, T1 payload)
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
                    }
                    else
                    {
                        Debug.Log("PostData: Request successful. Calling callback");
                        try
                        {
                            responseCache = JsonUtility.FromJson<T2>(webRequest.downloadHandler.text);
                        }
                        catch (ArgumentException e)
                        {
                            Debug.LogWarning("PostData: Got response, but it wasn't JSON");
                            throw;
                        }
                    }
                }
            }
            
            private IEnumerator GetData<T2>(string url)
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
                        Debug.Log("WebRequest failed: " + webRequest.error);
                    }
                    else
                    {
                        Debug.Log("PostData: Request successful. Calling callback");
                        try
                        {
                            responseCache = JsonUtility.FromJson<T2>(webRequest.downloadHandler.text);
                        }
                        catch (ArgumentException e)
                        {
                            Debug.LogWarning("PostData: Got response, but it wasn't JSON");
                            throw;
                        }
                    }
                }
            }
            
            #endregion
        }
    }
}