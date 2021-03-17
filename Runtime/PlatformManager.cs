using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.Win32.SafeHandles;
using UnityEngine;

namespace GamificationBackend
{
    /// <summary>
    /// A typical manager behaviour, which lets a game interface with the gamification backend.
    /// </summary>
    public partial class PlatformManager : MonoBehaviour
    {
        [SerializeField]
        private string gameToken;
        [SerializeField]
        private string host;
        
        #region private
        
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

        public PlaySession Session => session;

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
                api = new PlatformApi(host, gameToken);
            }
        }
        
        #endregion

        #region Public methods

        
        /// <summary>
        /// Registers a new player on the platform for the current game
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="company"></param>
        /// <param name="phone"></param>
        /// <param name="password"></param>
        /// <param name="callback">A callback action receiving a bool signifying whether the player was registered
        /// successfully</param>
        /// <returns></returns>
        public IEnumerator RegisterPlayer(string firstName, string lastName, string company, string phone, 
            string password, Action<PlatformResponse<bool>> callback)
        {
            yield return api.RegisterPlayer(firstName, lastName, company, phone, password, callback);
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
        public IEnumerator AuthenticateAndGetSession(string phone, string password, Action<PlatformResponse<PlaySession>> callback)
        {
            yield return api.BuildSession(phone, password, sessionResult =>
            {
                session = sessionResult.content;
                callback(sessionResult);
            });
        }

        /// <summary>
        /// Set the completion status of the session on the backend
        /// </summary>
        /// <param name="status">The status to set for the session</param>
        /// <param name="callback">Callback accepting a bool, which signifies the success of the call</param>
        /// <returns></returns>
        public IEnumerator SetCompletionStatus(CompletionStatus status, Action<PlatformResponse<bool>> callback)
        {
            if (session == null)
            {
                Debug.LogWarning("Play session has not been built. Ignoring request");
                yield break;
            }
            yield return api.UpdateActivityStatus(session, CompletionStatusToInt(status), callback);
        }
        
        public IEnumerator GetCompletionStatus(Action<PlatformResponse<PayloadActivityDetail>> callback)
        {
            if (session == null)
            {
                Debug.LogWarning("Play session has not been built. Ignoring request");
                yield break;
            }
            yield return api.GetActivityStatus(session, callback);
        }

        private int CompletionStatusToInt(CompletionStatus status)
        {
            switch (status)
            {
                case CompletionStatus.ENROLLED:
                    return 10;
                case CompletionStatus.STARTED:
                    return 20;
                case CompletionStatus.COMPLETED:
                    return 30;
            }

            return 0;
        }

        #endregion
    }
}