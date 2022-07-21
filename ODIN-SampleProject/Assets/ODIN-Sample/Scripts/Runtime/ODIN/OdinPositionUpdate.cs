using System;
using System.Collections;
using ODIN_Sample.Scripts.Runtime.Odin;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    /// <summary>
    /// This script will update the current players position in the ODIN room on a regular interval. This enables
    /// the ODIN optimization feature for 
    /// </summary>
    public class OdinPositionUpdate : MonoBehaviour
    {
        [SerializeField]
        private OdinPositionUpdateSettings settings;

        private void Awake()
        {
            Assert.IsNotNull(settings, $"ODIN-Demo: Missing settings file in OdinPositionUpdate script on object {gameObject}");
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


            foreach (Room room in OdinHandler.Instance.Rooms)
            {
                InitRoom(room);
            }
            OdinHandler.Instance.OnRoomJoined.AddListener(OnRoomJoined);

            yield return null;
            UpdateAllRoomPositions();
            StartCoroutine(UpdatePositionRoutine());
        }

        private void OnRoomJoined(RoomJoinedEventArgs eventArgs)
        {
            if (null != eventArgs && null != eventArgs.Room) InitRoom(eventArgs.Room);
        }

        private void InitRoom(Room toInit)
        {
            OdinRoomProximityStatus status = settings.GetRoomProximityStatus(toInit.Config.Name);
            if (null != status && status.isActive)
            {
                toInit.SetPositionScale(1.0f / status.proximityRadius);
            }
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
                {
                    if (proximitySetting.isActive &&  OdinHandler.Instance.Rooms.Contains(proximitySetting.roomName))
                        UpdateRoomPosition(OdinHandler.Instance.Rooms[proximitySetting.roomName]);
                }
                    
        }

        private void UpdateRoomPosition(Room toUpdate)
        {
            if (null != toUpdate && toUpdate.IsJoined)
            {
                var position = transform.position;
                bool success = toUpdate.UpdatePosition(position.x, position.z);
                // Debug.Log($"Updated to position {position.x}, {position.z}, Success: {success}");
            }
        }
    }
}