using System.Collections;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    public abstract class OdinVoiceIndicatorBase : MonoBehaviour
    {
        /// <summary>
        ///     Reference to the multiplayer adapter for which the indicator should check for transmissions.
        /// </summary>
        [SerializeField] private AOdinMultiplayerAdapter adapter;

        /// <summary>
        ///     The name of the ODIN room, for which this indicator should signal voice activity.
        /// </summary>
        [SerializeField] private OdinStringVariable odinRoomName;

        private int _numActivePlaybacks;

        protected virtual void Awake()
        {
            Assert.IsNotNull(odinRoomName);
            Assert.IsNotNull(adapter);
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(WaitForConnection());
        }

        protected virtual void OnDisable()
        {
            if (OdinHandler.Instance)
                OdinHandler.Instance.OnMediaActiveStateChanged.RemoveListener(OnMediaStateChanged);
        }


        private IEnumerator WaitForConnection()
        {
            while (!OdinHandler.Instance)
                yield return null;

            OdinHandler.Instance.OnMediaActiveStateChanged.AddListener(OnMediaStateChanged);
        }


        private void OnMediaStateChanged(object sender,
            MediaActiveStateChangedEventArgs args)
        {
            if (sender is Room sendingRoom && sendingRoom.Config.Name == odinRoomName.Value)
            {
                OdinSampleUserData userdata;
                if (!sendingRoom.RemotePeers.Contains(args.PeerId))
                {
                    userdata = OdinHandler.Instance.GetUserData().ToOdinSampleUserData();
                }
                else
                {
                    Peer peer = sendingRoom.RemotePeers[args.PeerId];
                    userdata = peer.UserData.ToOdinSampleUserData();
                }

                if (null != userdata && adapter.GetUniqueUserId() == userdata.uniqueUserId)
                {
                    if (args.Active)
                        _numActivePlaybacks++;
                    else
                        _numActivePlaybacks--;
                }
            }

            UpdateFeedback(_numActivePlaybacks > 0);
        }

        protected abstract void UpdateFeedback(bool isVoiceOn);
    }
}