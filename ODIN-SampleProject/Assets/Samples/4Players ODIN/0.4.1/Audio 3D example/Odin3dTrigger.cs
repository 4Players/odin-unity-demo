using OdinNative.Odin.Media;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using OdinNative.Unity;
using OdinNative.Unity.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        OdinHandler.Instance.OnRoomLeft.AddListener(Instance_OnRoomLeft);

        var SelfData = OdinHandler.Instance.GetUserData();
        //Set Player
        GameObject player = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
        if (player != null)
        {
            TextMesh lable = player.GetComponentInChildren<TextMesh>();
            lable.text = SelfData.name;
        }
    }

    GameObject CreateObject()
    {
        return Instantiate(prefab, new Vector3(0, 0.5f, 6), Quaternion.identity);
    }

    private void Instance_OnCreatedMediaObject(string roomName, ulong peerId, int mediaId)
    {
        Room room = OdinHandler.Instance.Rooms[roomName];
        if (room == null || room.Self.Id == peerId) return;

        //create dummy PeerCube
        var peerContainer = CreateObject();
        //Add PlaybackComponent to new dummy PeerCube
        PlaybackComponent playback = OdinHandler.Instance.AssignPlaybackComponent(peerContainer, room.Config.Name, peerId, mediaId);

        //Update Example
        //playback.CheckPlayingStatusInUpdate = true; // set checking status per frame active
        //InvokeRepeating Example
        playback.CheckPlayingStatusAsInvoke = true; // set checking status as MonoBehaviour.InvokeRepeating active
        playback.PlayingStatusDelay = 1.0f; // (default 0f)
        playback.PlayingStatusRepeatingTime = 0.3f; // (default 0.2f)

        playback.OnPlaybackPlayingStatusChanged += TalkIndicator; // set function for talking indication by status
        playback.PlaybackSource.spatialBlend = 1.0f; // set AudioSource to full 3D

        //set dummy PeerCube label
        var data = OdinUserData.FromUserData(room.RemotePeers[peerId]?.UserData);
        playback.gameObject.GetComponentInChildren<TextMesh>().text = $"{data.name} (Peer {peerId} Media {mediaId})";
        PeersObjects.Add(playback.gameObject);
    }

    private void TalkIndicator(PlaybackComponent playback, bool status)
    {
        Material cubeMaterial = playback.GetComponentInParent<Renderer>().material;
        if (status)
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
