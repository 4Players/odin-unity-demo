using OdinNative.Odin;
using OdinNative.Odin.Media;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using OdinNative.Unity;
using OdinNative.Unity.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OdinNative.Unity.Samples
{
    public class Odin3dTrigger : MonoBehaviour
    {
        public GameObject prefab;
        public List<GameObject> PeersObjects;
        private Color LastCubeColor;
        // Start is called before the first frame update
        void Start()
        {
            OdinHandler.Instance.OnCreatedMediaObject.AddListener(Instance_OnCreatedMediaObject);
            OdinHandler.Instance.OnDeleteMediaObject.AddListener(Instance_OnDeleteMediaObject);
            OdinHandler.Instance.OnMediaActiveStateChanged.AddListener(Instance_OnMediaActiveStateChanged);
            OdinHandler.Instance.OnRoomLeft.AddListener(Instance_OnRoomLeft);

            var SelfData = OdinHandler.Instance.GetUserData();
            if (SelfData.IsEmpty())
            {
                SelfData = new CustomUserDataJsonFormat().ToUserData();
                OdinHandler.Instance.UpdateUserData(SelfData);
            }

            //Set Player
            GameObject player = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
            if (player != null)
            {
                TextMesh label = player.GetComponentInChildren<TextMesh>();
                label.text = CustomUserDataJsonFormat.FromUserData(SelfData)?.name ?? player.name;
            }
        }

        GameObject CreateObject()
        {
            return Instantiate(prefab, new Vector3(0, 0.5f, 6), Quaternion.identity);
        }

        private void Instance_OnCreatedMediaObject(string roomName, ulong peerId, ushort mediaId)
        {
            Room room = OdinHandler.Instance.Rooms[roomName];
            if (room == null || room.Self == null || room.Self.Id == peerId) return;

            var peerContainer = CreateObject();

            //Add PlaybackComponent to new dummy PeerCube
            PlaybackComponent playback = OdinHandler.Instance.AddPlaybackComponent(peerContainer, room.Config.Name, peerId, mediaId);

            //Some AudioSource test settings
            playback.PlaybackSource.spatialBlend = 1.0f;
            playback.PlaybackSource.rolloffMode = AudioRolloffMode.Linear;
            playback.PlaybackSource.minDistance = 1;
            playback.PlaybackSource.maxDistance = 10;

            //set dummy PeerCube label
            var data = CustomUserDataJsonFormat.FromUserData(room.RemotePeers[peerId]?.UserData);
            playback.gameObject.GetComponentInChildren<TextMesh>().text = data == null ?
                $"Peer {peerId} (Media {mediaId})" :
                $"{data.name} (Peer {peerId} Media {mediaId})";

            PeersObjects.Add(playback.gameObject);
        }

        private void Instance_OnMediaActiveStateChanged(object sender, MediaActiveStateChangedEventArgs args)
        {
            PlaybackComponent playback = PeersObjects
                .Select(obj => obj.GetComponent<PlaybackComponent>())
                .FirstOrDefault(p => p.MediaId == args.MediaId);
            if(playback == null) return;

            Material cubeMaterial = playback.GetComponentInParent<Renderer>().material;
            if (playback.HasActivity)
            {
                LastCubeColor = cubeMaterial.color;
                cubeMaterial.color = Color.green;
            }
            else
                cubeMaterial.color = LastCubeColor;
        }

        private void Instance_OnDeleteMediaObject(int mediaId)
        {
            GameObject obj = PeersObjects.FirstOrDefault(o => o.GetComponent<PlaybackComponent>()?.MediaId == mediaId);
            if (obj == null) return;

            PeersObjects.Remove(obj);
            Destroy(obj);
        }

        private void Instance_OnRoomLeft(RoomLeftEventArgs args)
        {
            GameObject[] objs = PeersObjects
                .Where(o => o.GetComponent<PlaybackComponent>()?.RoomName == args.RoomName)
                .ToArray();

            if (objs.Length <= 0) return;

            foreach (var obj in objs)
            {
                PeersObjects.Remove(obj);
                Destroy(obj);
            }
        }

        private void OnDestroy()
        {
            foreach (var obj in PeersObjects)
                Destroy(obj);
        }
    }
}