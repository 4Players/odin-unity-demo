using System.Collections;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    ///     Behaviour for displaying feedback on whether the remote player represented by the <see cref="adapter" /> script
    ///     is currently transmitting in the ODIN room with the name <see cref="odinRoomName" />, by changing the color of a
    ///     mesh.
    /// </summary>
    public class OdinVoiceIndicator : MonoBehaviour
    {
        /// <summary>
        ///     Reference to the multiplayer adapter for which the indicator should check for transmissions.
        /// </summary>
        [FormerlySerializedAs("odinUser")] [SerializeField]
        private AOdinMultiplayerAdapter adapter;

        /// <summary>
        ///     This renderers color will be switched to <see cref="voiceOnColor" />, if the remote player is transmitting. The
        ///     color will return back to the original color of the main materials' initial color.
        /// </summary>
        [SerializeField] private Renderer indicationTarget;

        /// <summary>
        ///     The name of the ODIN room, for which this indicator should signal voice activity.
        /// </summary>
        [SerializeField] private OdinStringVariable odinRoomName;

        /// <summary>
        ///     The color the <see cref="indicationTarget" /> should display when the remote player is transmitting.
        /// </summary>
        [ColorUsage(true, true)] [SerializeField]
        private Color voiceOnColor = Color.green;

        private int _numActivePlaybacks;

        private Color _originalColor;

        private void Awake()
        {
            Assert.IsNotNull(odinRoomName);

            Assert.IsNotNull(adapter);

            if (null == indicationTarget)
                indicationTarget = GetComponent<Renderer>();
            Assert.IsNotNull(indicationTarget);

            _originalColor = indicationTarget.material.color;
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForConnection());
        }

        private void OnDisable()
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

            SetFeedbackColor(_numActivePlaybacks > 0);
        }

        private void SetFeedbackColor(bool isVoiceOn)
        {
            if (isVoiceOn)
                indicationTarget.material.color = voiceOnColor;
            else
                indicationTarget.material.color = _originalColor;
        }
    }
}