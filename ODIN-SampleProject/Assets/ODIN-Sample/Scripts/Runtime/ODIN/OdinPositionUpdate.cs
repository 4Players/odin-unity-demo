using System.Collections;
using ODIN_Sample.Scripts.Runtime.Odin;
using OdinNative.Odin.Room;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    public class OdinPositionUpdate : MonoBehaviour
    {
        [SerializeField] private float RoomScaling = 0.1f;
        [SerializeField] private float PositionUpdateInterval = 1.0f;


        [SerializeField] private OdinStringVariable[] connectedOdinRooms;

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

        private void InitRoom(Room toInit)
        {
            toInit.SetPositionScale(RoomScaling);
        }

        private IEnumerator UpdatePositionRoutine()
        {
            while (enabled)
            {
                UpdateAllRoomPositions();
                yield return new WaitForSeconds(PositionUpdateInterval);
            }
        }

        private void UpdateAllRoomPositions()
        {
            if (OdinHandler.Instance)
                foreach (OdinStringVariable odinRoomName in connectedOdinRooms)
                    if (OdinHandler.Instance.Rooms.Contains(odinRoomName))
                        UpdateRoomPosition(OdinHandler.Instance.Rooms[odinRoomName]);
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