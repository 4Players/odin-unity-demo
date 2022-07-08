using OdinNative.Odin;
using System;
using System.Text;
using UnityEngine;

namespace OdinNative.Unity.Samples
{
    [Serializable]
    class CustomUserDataJsonFormat : IUserData
    {
        public string id;
        public string seed;
        public string name;
        public string status;
        public int inputMuted;
        public int outputMuted;
        public string renderer;
        public string revision;
        public string version;
        public string platform;
        public string buildno;

        public CustomUserDataJsonFormat() : this("Unity Player", "online") { }
        public CustomUserDataJsonFormat(string name, string status)
        {
            this.id = SystemInfo.deviceUniqueIdentifier;
            this.seed = SystemInfo.deviceUniqueIdentifier;
            this.name = name;
            this.status = status;
            this.inputMuted = 0;
            this.outputMuted = 0;
            this.renderer = Application.unityVersion;
            this.revision = "0";
            this.version = Application.version;
            this.platform = string.Format("{0}/{1}", Application.platform, Application.unityVersion);
            this.buildno = Application.buildGUID;
        }

        public UserData ToUserData()
        {
            return new UserData(this.ToJson());
        }

        public static CustomUserDataJsonFormat FromUserData(IUserData userData)
        {
            try
            {
                return JsonUtility.FromJson<CustomUserDataJsonFormat>(userData.ToString());
            } catch { return null; }
        }

        public static bool FromUserData(UserData userData, out CustomUserDataJsonFormat customUserData)
        {
            try
            {
                customUserData = JsonUtility.FromJson<CustomUserDataJsonFormat>(userData.ToString());
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                customUserData = null;
                return false;
            }
        }

        internal string GetAvatarUrl()
        {
            return string.Format("https://avatars.dicebear.com/api/bottts/{0}.svg?textureChance=0", this.seed);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override string ToString()
        {
            return this.ToJson();
        }

        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(this.ToString());
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(this.ToString());
        }
    }
}