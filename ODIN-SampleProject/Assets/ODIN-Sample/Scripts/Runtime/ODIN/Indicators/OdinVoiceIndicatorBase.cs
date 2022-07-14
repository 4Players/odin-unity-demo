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

        // private int _numActivePlaybacks;

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
            {
                OdinHandler.Instance.OnMediaActiveStateChanged.RemoveListener(OnMediaStateChanged);
                OdinHandler.Instance.OnMediaRemoved.RemoveListener(OnMediaRemoved);
                OdinHandler.Instance.OnPeerLeft.RemoveListener(OnPeerLeft);
            }
        }


        private IEnumerator WaitForConnection()
        {
            while (!OdinHandler.Instance)
                yield return null;

            OdinHandler.Instance.OnMediaActiveStateChanged.AddListener(OnMediaStateChanged);
            OdinHandler.Instance.OnMediaRemoved.AddListener(OnMediaRemoved);
            OdinHandler.Instance.OnPeerLeft.AddListener(OnPeerLeft);
        }

        private void OnPeerLeft(object sender, PeerLeftEventArgs peerLeftEventArgs)
        {
            Debug.Log("On Peer Left.");
            if (sender is Room sendingRoom)
            {
                ulong peerId = peerLeftEventArgs.PeerId;
                if (IsEventRelevant(sendingRoom, peerId))
                    UpdateFeedback(false);
            }
        }

        private void OnMediaRemoved(object sender, MediaRemovedEventArgs mediaRemovedEventArgs)
        {
            Debug.Log("OnMediaRemoved.");

            if (sender is Room sendingRoom)
            {
                ulong peerId = mediaRemovedEventArgs.Peer.Id;
                if (IsEventRelevant(sendingRoom, peerId))
                    UpdateFeedback(false);

            }
        }

        private void OnMediaStateChanged(object sender,
            MediaActiveStateChangedEventArgs args)
        {
            Debug.Log($"OnMediaStateChanged: {args.Active}");

            if (sender is Room sendingRoom)
            {
                ulong peerId = args.PeerId;
                if (IsEventRelevant(sendingRoom, peerId))
                    UpdateFeedback(args.Active);
            }
        }

        private bool IsEventRelevant(Room sendingRoom, ulong peerId)
        {
            var userdata = GetUserData(sendingRoom, peerId);
            return null != userdata && adapter.GetUniqueUserId() == userdata.uniqueUserId && sendingRoom.Config.Name == odinRoomName.Value;
        }

        private static OdinSampleUserData GetUserData(Room sendingRoom, ulong peerId)
        {
            OdinSampleUserData userdata;
            if (!sendingRoom.RemotePeers.Contains(peerId))
            {
                userdata = OdinHandler.Instance.GetUserData().ToOdinSampleUserData();
            }
            else
            {
                Peer peer = sendingRoom.RemotePeers[peerId];
                userdata = peer.UserData.ToOdinSampleUserData();
            }

            return userdata;
        }

        protected abstract void UpdateFeedback(bool isVoiceOn);
    }
}