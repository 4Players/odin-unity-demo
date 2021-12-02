using System;
using OdinNative.Odin;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    public static class UserDataExtension
    {
        public static OdinSampleUserData ToOdinSampleUserData(this UserData userData)
        {
            return OdinSampleUserData.FromUserData(userData);
        }
    }
    
    [Serializable]
    public class OdinSampleUserData
    {
        public static implicit operator UserData(OdinSampleUserData data) => data?.ToUserData() ?? null;

        public string name;
        public string uniqueUserId;
        public string color;
        public int muted;
        public string user;
        public string renderer;
        public string platform;
        public string revision;
        public string version;
        public string buildno;

        public OdinSampleUserData() : this("Unity Player") { }
        public OdinSampleUserData(string name)
        {
            this.name = name;
            color = ColorUtility.ToHtmlStringRGB(Color.white);
            this.uniqueUserId = SystemInfo.deviceUniqueIdentifier;
            this.muted = 0;
            this.user = string.Format("{0}.{1}", Application.companyName, Application.productName);
            this.renderer = Application.unityVersion;
            this.platform = string.Format("{0}/{1}", Application.platform, Application.unityVersion);
            this.revision = "0";
            this.version = Application.version;
            this.buildno = Application.buildGUID;
        }

        public UserData ToUserData()
        {
            return new UserData(this.ToJson());
        }

        public static OdinSampleUserData FromUserData(UserData userData)
        {
            return JsonUtility.FromJson<OdinSampleUserData>(userData.ToString());
        }
        

        public static bool FromUserData(UserData userData, out OdinSampleUserData odinUserData)
        {
            try
            {
                odinUserData = JsonUtility.FromJson<OdinSampleUserData>(userData.ToString());
                return true;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                odinUserData = null;
                return false;
            }
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
        
    }
}