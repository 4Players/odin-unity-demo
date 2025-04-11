using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Atmoky;

public class TriggerBoxScript : MonoBehaviour
{
    private List<GameObject> playersInBox = new List<GameObject>();

    public AudioMixerSnapshot snapshotInside;
    public AudioMixerSnapshot snapshotOutside;

    private bool locaPlayerInBox = false;

    void OnTriggerEnter(Collider other)
    {

        var photonView = other.gameObject.GetComponent<Photon.Pun.PhotonView>();
        if (photonView != null)
        {
            if (photonView.IsMine)
            {
                Debug.Log("Player entered trigger box");
                locaPlayerInBox = true;
                snapshotInside.TransitionTo(0.5f);
            }
            else
            {
                Debug.Log("Player entered trigger box");
                if (!playersInBox.Contains(other.gameObject))
                {
                    Debug.Log("add player to list");
                    playersInBox.Add(other.gameObject);
                }
            }

            updateSendLevels();
        }
    }

    void OnTriggerExit(Collider other)
    {
        var photonView = other.gameObject.GetComponent<Photon.Pun.PhotonView>();
        if (photonView != null)
        {
            if (photonView.IsMine)
            {
                Debug.Log("Player exited trigger box");
                locaPlayerInBox = false;
                snapshotOutside.TransitionTo(0.5f);
                updateSendLevels();
            }
            else
            {
                Debug.Log("Player exited trigger box");
                var player = other.gameObject;
                setSendLevelDb(player, -80);
                if (playersInBox.Contains(player))
                {
                    Debug.Log("remove player from list");
                    playersInBox.Remove(player);


                }
            }

        }
    }

    void updateSendLevels()
    {
        var sendLevel = locaPlayerInBox ? 0 : -80;
        var playersToRemove = new List<GameObject>();
        foreach (var player in playersInBox)
        {
            if (player == null)
            {
                playersToRemove.Add(player);
                continue;
            }
            setSendLevelDb(player, sendLevel);
        }
        foreach (var player in playersToRemove)
        {
            playersInBox.Remove(player);
        }
    }

    void setSendLevelDb(GameObject player, float sendLevelDb)
    {
        var atmokySource = player.GetComponentInChildren<Atmoky.Source>();
        if (atmokySource != null)
        {
            atmokySource.sendLevel = sendLevelDb;
        }
    }
}
