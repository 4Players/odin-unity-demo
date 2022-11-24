using System;
using System.Collections;
using System.Collections.Generic;
using OdinNative.Odin.Media;
using OdinNative.Odin.Room;
using OdinNative.Unity.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestJoin : MonoBehaviour
{
    [SerializeField] private PlaybackComponent playbackComponentPrefab;
    [SerializeField] private InputActionReference[] pushToTalks;
    [SerializeField] private string[] rooms;

    private List<PlaybackComponent> _spawnedPlaybacks = new List<PlaybackComponent>();

    private void OnEnable()
    {
        foreach (InputActionReference actionReference in pushToTalks)
        {
            actionReference.action.Enable();
        }
    }

    private void Update()
    {
        if (OdinHandler.Instance)
        {
            for (var i = 0; i < pushToTalks.Length; i++)
            {
                InputActionReference pushToTalk = pushToTalks[i];
                string room = rooms[i];

                Room currentRoom = OdinHandler.Instance.Rooms[room];
                if (null != currentRoom && null != currentRoom.MicrophoneMedia)
                {
                    currentRoom.MicrophoneMedia.SetMute(!pushToTalk.action.IsPressed());
                }
            }
        }
    }


    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (!OdinHandler.Instance)
            yield return null;
        
        foreach (string room in rooms)
        {
            OdinHandler.Instance.JoinRoom(room);
            Debug.Log($"Joining room {room}");
            yield return null;
        }
        
        OdinHandler.Instance.OnMediaAdded.AddListener(OnMediaAdded);
        OdinHandler.Instance.OnMediaRemoved.AddListener(OnMediaRemoved);
    }

    private void OnMediaRemoved(object roomObject, MediaRemovedEventArgs mediaRemovedEventArgs)
    {
        if (roomObject is Room room)
        {
            string mediaRoomName = room.Config.Name;
            ulong peerId = mediaRemovedEventArgs.Peer.Id;
            long mediaId = mediaRemovedEventArgs.MediaStreamId;

            PlaybackComponent toRemove = null;
            foreach (PlaybackComponent pc in _spawnedPlaybacks)
            {
                if (pc.RoomName == mediaRoomName && pc.PeerId == peerId && pc.MediaStreamId == mediaId)
                {
                    toRemove = pc;
                }
            }

            if (null != toRemove)
            {
                _spawnedPlaybacks.Remove(toRemove);
                Destroy(toRemove.gameObject);
            }
        }
    }

    private void OnMediaAdded(object roomObject, MediaAddedEventArgs mediaAddedEventArgs)
    {
        if (roomObject is Room room)
        {
            string mediaRoomName = mediaAddedEventArgs.Peer.RoomName;
            ulong mediaPeerId = mediaAddedEventArgs.PeerId;

            MicrophoneStream microphoneStream = OdinHandler.Instance.Rooms[mediaRoomName].MicrophoneMedia;
            ulong localPeerId = microphoneStream?.GetPeerId() ?? 0;
            if (localPeerId != mediaPeerId)
            {
                long mediaId = mediaAddedEventArgs.Media.Id;
                
                PlaybackComponent spawned = Instantiate(playbackComponentPrefab.gameObject, transform)
                    .GetComponent<PlaybackComponent>();

                spawned.RoomName = mediaRoomName;
                spawned.PeerId = mediaPeerId;
                spawned.MediaStreamId = mediaId;
                
                _spawnedPlaybacks.Add(spawned);
            }
        }
    }
}
