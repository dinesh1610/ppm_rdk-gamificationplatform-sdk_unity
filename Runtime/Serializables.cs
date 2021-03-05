using System;
using UnityEngine;

namespace GamificationBackend
{
    /// <summary>
    /// An instance describing the authenticated session. Inpect the IsValid property to learn
    /// whether it has authenticated correctly
    /// </summary>
    [Serializable]
    public class PlaySession
    {
        public string appToken;
        public string personalToken;
        public int campaignID;
        public int gameID;
        public int status;
        public int player;
    }

    [Serializable]
    public class PayloadAuth
    {
        public string game_token;
        public string phone;
        public string password;
    }

    [Serializable]
    public class PayloadToken
    {
        public string token;
    }

    [Serializable]
    public class PayloadGameDetail
    {
        public int id;
        public string name;
        public string content_url;
        public int status;
        public int campaign;
    }

    [Serializable]
    public class PayloadActivityDetail
    {
        public int game;
        public int player;
        public int status;
        public int campaign;
        public string last_modified;
    }

    [Serializable]
    public class PayloadRegisterPlayer
    {
        public string first_name;
        public string last_name;
        public string company;
        public string phone;
        public string password;
        public string game_token;
    }

    [Serializable]
    public class PayloadPlayer
    {
        public int id;
        public string email;
        public string first_name;
        public string last_name;
        public string full_name;
    }

    public enum CompletionStatus
    {
        ENROLLED,
        STARTED,
        COMPLETED
    }
    
}
