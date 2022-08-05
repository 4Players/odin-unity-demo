using System.Collections;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    /// <summary>
    ///     This script will update the current players position in the ODIN room on a regular interval. This enables
    ///     the ODIN proximity feature, which removes and adds Media Streams based on the given position of players,
    ///     optimizing the required audio streaming bandwidth.
    /// </summary>
    public class OdinPositionUpdate : MonoBehaviour
    {
        /// <summary>
        ///     The settings file. Contains data for the rooms on which the optimization feature should be activated.
        /// </summary>
        [SerializeField] private OdinPositionUpdateSettings settings;

        private void Awake()
        {
            Assert.IsNotNull(settings,
                $"ODIN-Demo: Missing settings file in OdinPositionUpdate script on object {gameObject}");
        }

        private void OnEnable()
        {
            StartCoroutine(DelayedEnable());
        }

        private void OnDisable()
        {
            if (OdinHandler.Instance) OdinHandler.Instance.OnRoomJoined.RemoveListener(OnRoomJoined);

            StopCoroutine(UpdatePositionRoutine());
        }

        private IEnumerator DelayedEnable()
        {
            while (!OdinHandler.Instance)
                yield return null;


            foreach (Room room in OdinHandler.Instance.Rooms) InitRoom(room);
            OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);

            yield return null;
            UpdateAllRoomPositions();
            StartCoroutine(UpdatePositionRoutine());
        }

        private void OnRoomJoined(RoomJoinedEventArgs eventArgs)
        {
            if (null != eventArgs && null != eventArgs.Room) InitRoom(eventArgs.Room);
        }

        /// <summary>
        /// Initializes the position scale. The position scale should be set to the same value on each client.
        /// </summary>
        /// <param name="toInit">Room to initialize position scale in.</param>
        private void InitRoom(Room toInit)
        {
            OdinRoomProximityStatus status = settings.GetRoomProximityStatus(toInit.Config.Name);
            if (null != status && status.isActive) toInit.SetPositionScale(1.0f / status.proximityRadius);
        }

        private IEnumerator UpdatePositionRoutine()
        {
            while (enabled)
            {
                UpdateAllRoomPositions();
                yield return new WaitForSeconds(settings.updateInterval);
            }
        }

        private void UpdateAllRoomPositions()
        {
            if (OdinHandler.Instance)
                foreach (var proximitySetting in settings.roomSettings)
                    if (proximitySetting.isActive && OdinHandler.Instance.Rooms.Contains(proximitySetting.roomName))
                        UpdateRoomPosition(OdinHandler.Instance.Rooms[proximitySetting.roomName]);
        }

        private void UpdateRoomPosition(Room toUpdate)
        {
            if (null != toUpdate && toUpdate.IsJoined)
            {
                var position = transform.position;
                toUpdate.UpdatePosition(position.x, position.z);
            }
        }
    }
}