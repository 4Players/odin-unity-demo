using System;
using OdinNative.Odin;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Odin
{
    /// <summary>
    /// Extension class for converting user data.
    /// </summary>
    public static class UserDataExtension
    {
        /// <summary>
        /// Converts user data into the Sample projects User Data definition.
        /// </summary>
        /// <param name="userData">The Odin native user data object.</param>
        /// <returns>The converted user data.</returns>
        public static OdinSampleUserData ToOdinSampleUserData(this UserData userData)
        {
            return OdinSampleUserData.FromUserData(userData);
        }
    }
    
    /// <summary>
    /// User data definition for the Sample Project. Most important are the <see cref="name"/> and <see cref="uniqueUserId"/>
    /// properties. The <see cref="name"/> is used for displaying the user name using only ODIN functionality. The <see cref="uniqueUserId"/>
    /// is required for connecting ODIN users with their in-game representation, using the Multiplayer Solution used in
    /// your project. This could be e.g. a photon views unique id.
    /// </summary>
    [Serializable]
    public class OdinSampleUserData
    {
        public static implicit operator UserData(OdinSampleUserData data) => data?.ToUserData() ?? null;

        /// <summary>
        /// The player's name
        /// </summary>
        public string name;
        /// <summary>
        /// The player's unique user id, used for identification of a user in multiplayer scenes.
        /// </summary>
        public string uniqueUserId;
        /// <summary>
        /// The player's capsule color as an html rgb string. Can be converted by using:
        /// <code>
        ///     ColorUtility.TryParseHtmlString("#" + color, out var result);
        /// </code>
        /// </summary>
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

        /// <summary>
        /// Converts this user data back into Odins base UserData definition for network synchronization.
        /// </summary>
        /// <returns></returns>
        public UserData ToUserData()
        {
            return new UserData(this.ToJson());
        }

        /// <summary>
        /// Converts the given Odin base UserData definition into an OdinSampleUserData object.
        /// </summary>
        /// <param name="userData">The Odin base UserData object.</param>
        /// <returns>The converted OdinSampleUserData object.</returns>
        public static OdinSampleUserData FromUserData(UserData userData)
        {
            return JsonUtility.FromJson<OdinSampleUserData>(userData.ToString());
        }

        /// <summary>
        /// Converts this object into Json format.
        /// </summary>
        /// <returns>The resulting Json object.</returns>
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
        
    }
}