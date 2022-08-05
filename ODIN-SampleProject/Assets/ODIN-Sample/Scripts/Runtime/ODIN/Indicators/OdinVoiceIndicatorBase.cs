using System.Collections;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    ///     Behaviour for displaying feedback on whether the remote player represented by the <see cref="adapter" /> script
    ///     is currently transmitting in the ODIN room with the name <see cref="odinRoomName" />.
    /// </summary>
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
            }
        }


        private IEnumerator WaitForConnection()
        {
            while (!OdinHandler.Instance)
                yield return null;

            OdinHandler.Instance.OnMediaActiveStateChanged.AddListener(OnMediaStateChanged);
            OdinHandler.Instance.OnMediaRemoved.AddListener(OnMediaRemoved);
        }


        private void OnMediaRemoved(object sender, MediaRemovedEventArgs mediaRemovedEventArgs)
        {
            if (sender is Room sendingRoom)
                if (sendingRoom.Config.Name == odinRoomName)
                {
                    ulong peerId = mediaRemovedEventArgs.Peer.Id;
                    if (!adapter.IsLocalUser() && !sendingRoom.RemotePeers.Contains(peerId))
                    {
                        // Debug.Log(
                        //     $"Adapter: {adapter.gameObject.name}, setting feedback inactive in OnMediaRemoved, sending room: {sendingRoom.Config.Name}, odin room: {odinRoomName}, " +
                        //     $"self id: {sendingRoom.Self.Id}, removed peer id: {peerId}");
                        UpdateFeedback(false);
                    }
                }
        }

        private void OnMediaStateChanged(object sender,
            MediaActiveStateChangedEventArgs args)
        {
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
            return null != userdata && adapter.GetUniqueUserId() == userdata.uniqueUserId &&
                   sendingRoom.Config.Name == odinRoomName.Value;
        }

        private static OdinSampleUserData GetUserData(Room sendingRoom, ulong peerId)
        {
            OdinSampleUserData userdata = null;
            if (sendingRoom.Self.Id == peerId)
            {
                userdata = OdinHandler.Instance.GetUserData().ToOdinSampleUserData();
            }
            else if (sendingRoom.RemotePeers.Contains(peerId))
            {
                Peer peer = sendingRoom.RemotePeers[peerId];
                userdata = peer.UserData.ToOdinSampleUserData();
            }

            return userdata;
        }

        protected abstract void UpdateFeedback(bool isVoiceOn);
    }
}