using System;
using UnityEngine;

namespace GamificationBackend
{
    class DeserializeException : ArgumentException {}

    interface IBaseSerializable
    {
        bool check();
    }
    
    /// <summary>
    /// An instance describing the authenticated session. Inpect the IsValid property to learn
    /// whether it has authenticated correctly
    /// </summary>
    [Serializable]
    public class PlaySession : IBaseSerializable
    {
        public string appToken;
        public string personalToken;
        public int campaignID;
        public int gameID;
        public int status;
        public int player;
        
        public bool check()
        {
            return player != 0;
        }
    }

    [Serializable]
    public class PayloadAuth : IBaseSerializable
    {
        public string game_token;
        public string phone;
        public string password;
        
        public bool check()
        {
            return !string.IsNullOrEmpty(phone);
        }
    }

    [Serializable]
    public class PayloadToken : IBaseSerializable
    {
        public string token;
        
        public bool check()
        {
            return !string.IsNullOrEmpty(token);
        }
    }

    [Serializable]
    public class PayloadGameToken : IBaseSerializable
    {
        public string game_token;
        
        public bool check()
        {
            return !string.IsNullOrEmpty(game_token);
        }
    }

    [Serializable]
    public class PayloadGameDetail : IBaseSerializable
    {
        public int id;
        public string name;
        public string content_url;
        public int status;
        public int campaign;
        
        public bool check()
        {
            return id != 0;
        }
    }

    [Serializable]
    public class PayloadActivityDetail : IBaseSerializable
    {
        public int game;
        public int player;
        public int status;
        public int campaign;
        public string last_modified;
        
        public bool check()
        {
            return game != 0;
        }
    }

    [Serializable]
    public class PayloadRegisterPlayer : IBaseSerializable
    {
        public string first_name;
        public string last_name;
        public string company;
        public string phone;
        public string password;
        public string game_token;
        
        public bool check()
        {
            return !string.IsNullOrEmpty(phone);
        }
    }

    [Serializable]
    public class PayloadPlayer : IBaseSerializable
    {
        public int id;
        public string username;
        public string email;
        public string first_name;
        public string last_name;
        public string full_name;
        
        public bool check()
        {
            return id != 0;
        }
    }

    [Serializable]
    public class PayloadSetUdfValue<T> : IBaseSerializable
    {
        public string name;
        public T value;
        public int type_id;
        
        public bool check()
        {
            return !string.IsNullOrEmpty(name);
        }
    }

    [Serializable]
    public class PayloadUdfValue<T> : IBaseSerializable
    {
        // {"id":5005,"field_type":{"id":114,"name":"Game Score","type_id":20,"options":"","project":655},"value":7,"target_object_id":3}
        public int id;
        public UdfFieldType field_type;
        public T value;
        public int target_object_id;
        
        public bool check()
        {
            return id != 0;
        }

        public override string ToString()
        {
            return $"{id}::{field_type.name}::{value}::{field_type.type_id}";
        }
    }

    [Serializable]
    public class UdfFieldType
    {
        public int id;
        public string name;
        public int type_id;
    }
    

    public enum CompletionStatus
    {
        ENROLLED,
        STARTED,
        COMPLETED
    }

    public enum UdfType
    {
        TEXT_TYPE,
        NUMBER_TYPE,
        TEXT_LIST_TYPE,
        NUMBER_LIST_TYPE,
        DATE_TYPE,
        DATE_LIST_TYPE,
        LINK_TYPE,
        LINK_LIST_TYPE
    }
    
}
