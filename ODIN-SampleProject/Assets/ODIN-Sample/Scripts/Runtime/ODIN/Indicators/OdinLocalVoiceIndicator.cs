using System.Collections;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    ///     Behaviour for showing visual feedback, when the local player's voice is transmitting to an ODIN room with the name
    ///     <see cref="roomName" />.
    /// </summary>
    public class OdinLocalVoiceIndicator : MonoBehaviour
    {
        /// <summary>
        ///     This renderers color will be switched to <see cref="voiceOnColor" />, if the local player is transmitting. The
        ///     color will return back to the original color of the renderers material.
        /// </summary>
        [SerializeField] private Renderer indicationTarget;

        /// <summary>
        ///     The name of the ODIN room on which the indicator should be listening for transmissions.
        /// </summary>
        [SerializeField] private OdinStringVariable roomName;

        /// <summary>
        ///     The color the <see cref="indicationTarget" /> should display when the local player is transmitting.
        /// </summary>
        [ColorUsage(true, true)] [SerializeField]
        private Color voiceOnColor = Color.green;

        private Color _originalColor;

        private void Awake()
        {
            if (null == indicationTarget)
                indicationTarget = GetComponent<Renderer>();
            Assert.IsNotNull(indicationTarget);
            _originalColor = indicationTarget.material.color;
        }

        private void OnEnable()
        {
            StartCoroutine(WaitForConnection());
        }

        private IEnumerator WaitForConnection()
        {
            while (!OdinHandler.Instance)
                yield return null;

            OdinHandler.Instance.OnMediaActiveStateChanged.AddListener(OnMediaStateChanged);
        }


        private void OnMediaStateChanged(object sender,
            MediaActiveStateChangedEventArgs mediaActiveStateChangedEventArgs)
        {
            if (sender is Room sendingRoom && sendingRoom.Config.Name == roomName.Value &&
                sendingRoom.Self.Id == mediaActiveStateChangedEventArgs.PeerId)
                //Debug.Log($"Sending state of Room {sendingRoom.Config.Name} changed: {mediaActiveStateChangedEventArgs.Active}");
                SetFeedbackColor(mediaActiveStateChangedEventArgs.Active && !sendingRoom.MicrophoneMedia.IsMuted);
        }

        //private void Update()
        //{

        //    //bool isVoiceOn = false;

        //    //if (OdinHandler.Instance && OdinHandler.Instance.Rooms.Contains(roomName))
        //    //{
        //    //    Room room = OdinHandler.Instance.Rooms[roomName];
        //    //    if(null != room.MicrophoneMedia)
        //    //    {
        //    //        if (isVoiceOn != room.MicrophoneMedia.IsActive)
        //    //            Debug.Log("Is Media active: " + isVoiceOn);
        //    //        isVoiceOn = room.MicrophoneMedia.IsActive;

        //    //    }
        //    //}
        //    //SetFeedbackColor(isVoiceOn);

        //}

        private void SetFeedbackColor(bool isVoiceOn)
        {
            if (isVoiceOn)
                indicationTarget.material.color = voiceOnColor;
            else
                indicationTarget.material.color = _originalColor;
        }
    }
}