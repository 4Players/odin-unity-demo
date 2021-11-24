using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace ODIN_Sample.Scripts.Runtime.ThirdPerson
{
    public class PhotonSyncedColor : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private Renderer capsuleRenderer = null;
        private static string ColorKey => "PlayerColor";

        private void Awake()
        {
            Assert.IsNotNull(photonView);
            Assert.IsNotNull(capsuleRenderer);

            if (capsuleRenderer && photonView.IsMine)
            {
                Random.InitState(SystemInfo.deviceUniqueIdentifier.GetHashCode());
                Color playerColor = Random.ColorHSV();
                if (PlayerPrefs.HasKey(ColorKey))
                {
                    string playerColorString = PlayerPrefs.GetString(ColorKey);
                    ColorUtility.TryParseHtmlString("#" + playerColorString, out playerColor);
                }
                else
                {
                    PlayerPrefs.SetString(ColorKey, ColorUtility.ToHtmlStringRGB(playerColor));
                    PlayerPrefs.Save();
                }
                capsuleRenderer.material.color = playerColor;
                
                if(PhotonNetwork.IsConnected)
                    SendColorSync();
            }
        }
        

        public override void OnJoinedRoom()
        {
            if (photonView.IsMine)
            {
                SendColorSync();
            }
        }
        
        private void SendColorSync()
        {
            // Debug.Log("Sending Color Sync");
            if (PhotonNetwork.IsConnectedAndReady)
            {
                photonView.RPC("PerformColorSync", RpcTarget.OthersBuffered,
                    ColorUtility.ToHtmlStringRGB(capsuleRenderer.material.color));
            }
            
        }
        
        [PunRPC]
        private void PerformColorSync(string colorName)
        {
            // Debug.Log($"Received PerformColorSync: {colorName}");
            if (!photonView.IsMine)
            {
                ColorUtility.TryParseHtmlString("#" + colorName, out Color capsuleColor);
                capsuleRenderer.material.color = capsuleColor;
                // Debug.Log($"Successfully set remote capsule color.");
            }
        }
    }
}