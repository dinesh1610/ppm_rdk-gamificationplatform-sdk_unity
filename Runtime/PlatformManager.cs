using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamificationBackend
{
    /// <summary>
    /// A typical manager behaviour, which lets a game interface with the gamification backend.
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        private static PlatformManager instance;
        public static PlatformManager Instance
        {
            get => instance;
            set
            {
                if (instance != null)
                {
                    return;
                }
                instance = value;
            }
        }

        private PlatformApi api;
        private PlaySession session;

        private void Awake()
        {
            Instance = this;
            if (Instance != this)
            {
                Debug.LogWarning("Attempted to overwrite PlatformManager singleton." +
                                 " Destroying offender");
                Destroy(gameObject);
            }
            else
            {
                api = new PlatformApi();
            }
        }

        /// <summary>
        /// Authenticates with the backend and internally stores reference to the session,
        /// so that even though the API needs to add authentication to each request, the
        /// caller in the game won't ned to care about that detail.
        /// </summary>
        /// <param name="pin1">First PIN for authentication</param>
        /// <param name="pin2">Second PIN for authentication</param>
        /// <param name="callback">Callback accepting the produced session</param>
        /// <returns></returns>
        public IEnumerator AuthenticateAndGetSession(int pin1, int pin2, Action<PlaySession> callback)
        {
            return api.BuildSession($"{pin1}-{pin2}", (PlaySession sessionResult) =>
            {
                session = sessionResult;
                callback(session);
            });
        }

        /// <summary>
        /// Set the completion status of the session on the backend
        /// </summary>
        /// <param name="status">The status to set for the session</param>
        /// <param name="callback">Callback accepting a bool, which signifies the success of the call</param>
        /// <returns></returns>
        public IEnumerator SetCompletionStatus(CompletionStatus status, Action<bool> callback)
        {
            return api.PostData(session, "status", status, callback);
        }

        /// <summary>
        /// Set any data on the session, which will show up in the table on the backend
        /// </summary>
        /// <param name="path"></param>
        /// <param name="values"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator SetMetaData<T>(string path, List<DataPoint<T>> values, Action<bool> callback)
        {
            // TODO format the datapoints into something the API understands
            return api.PostData(session, path, "random string for now", callback);
        }

        class PlatformApi
        {
            public IEnumerator BuildSession(string auth, Action<PlaySession> callback)
            {
                // TODO
                yield return new WaitForSeconds(1f);
                callback(new PlaySession(auth, "123123", true));  // TODO
            }

            public IEnumerator PostData(PlaySession session, string path, int value, Action<bool> callback)
            {
                // TODO
                yield return new WaitForSeconds(1f);
                callback(true);
            }
            
            public IEnumerator PostData(PlaySession session, string path, string value, Action<bool> callback)
            {
                // TODO
                yield return new WaitForSeconds(1f);
                callback(true);
            }
            
            public IEnumerator PostData(PlaySession session, string path, CompletionStatus value, Action<bool> callback)
            {
                // TODO
                yield return new WaitForSeconds(1f);
                callback(true);
            }
        }
    }

    /// <summary>
    /// An instance describing the authenticated session. Inpect the IsValid property to learn
    /// whether it has authenticated correctly
    /// </summary>
    public class PlaySession
    {
        private string auth;
        private string key;
        private bool isValid;

        public bool IsValid => isValid;

        public PlaySession(string auth, string key, bool isValid)
        {
            this.auth = auth;
            this.key = key;
            this.isValid = isValid;
        }
        
    }

    [Serializable]
    public class DataPoint<T>
    {
        // TODO
        public string name;
        public T value;

    }

    public enum CompletionStatus
    {
        INITIAL,
        COMPLETED,
        FAILED
    }
}