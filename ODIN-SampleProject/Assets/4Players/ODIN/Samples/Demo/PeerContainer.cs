using OdinNative.Unity.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OdinNative.Unity.Samples
{
	/// <summary>
	/// Demo Example where everything in #region Demo does not correlate with Odin
	/// </summary>
	public class PeerContainer : MonoBehaviour
	{
		public float HearingDistance;
		private PlaybackComponent Playback;
		private Color lastCubeColor;

		void Start()
		{
			// Get the PlaybackComponent of this Container 
			Playback = gameObject.GetComponent<PlaybackComponent>();
			// Example set the AudioSource Rolloff to the HearingDistance of this Container
			Playback.PlaybackSource.maxDistance = HearingDistance;
			#region Demo
			Icon = gameObject.GetComponentInChildren<UnityEngine.UI.RawImage>();
			Player = GameObject.FindGameObjectWithTag("Player");

            Pointlight.range = HearingDistance;
			originalPointlightColour = Pointlight.color;
			Spotlight.range = ViewDistance;

			if (pathContainer != null)
			{
				Vector3[] checkpoints = new Vector3[pathContainer.childCount];
				for (int i = 0; i < checkpoints.Length; i++)
				{
					checkpoints[i] = pathContainer.GetChild(i).position;
					checkpoints[i] = new Vector3(checkpoints[i].x, transform.position.y, checkpoints[i].z);
				}
				DoMove = FollowPath(checkpoints);
			}
			else
				AutoMove = false;

			if (AutoMove)
				MoveWorker = StartCoroutine(DoMove);
			#endregion Demo
		}

		void Update()
		{
			#region Demo
			// Demo moving
			if (AutoMove)
			{
				if (MoveWorker == null && DoMove != null)
					MoveWorker = StartCoroutine(DoMove);
			}
			else if (AutoMove == false && MoveWorker != null)
			{
				StopCoroutine(MoveWorker);
				MoveWorker = null;
			}
			#endregion Demo

			if (Playback == null) return;
			CheckTalkIndicator();

			/* 
			 * This is just an example how to access the PlaybackComponent or AudioSource,
			 * not a way for real audio occlusion behaviors. 
			 */
			switch (PlayerCanHear())
			{
				case DemoHearingType.Normal:
					Icon.texture = Voice;
					Pointlight.color = originalPointlightColour;
					// set for this AudioSource in PlaybackComponent the AudioMixerGroup
					Playback.PlaybackSource.outputAudioMixerGroup = OdinHandler.Instance.PlaybackAudioMixer.FindMatchingGroups("Normal")[0];
					// set how much this AudioSource is affected by 3D spatialisation calculations
					Playback.PlaybackSource.spatialBlend = 1.0f;
					break;
				case DemoHearingType.Blocked:
					Icon.texture = Muff;
					Pointlight.color = Color.yellow;
					Playback.PlaybackSource.outputAudioMixerGroup = OdinHandler.Instance.PlaybackAudioMixer.FindMatchingGroups("Muffled")[0];
					Playback.PlaybackSource.spatialBlend = 1.0f;
					break;
				case DemoHearingType.Echo:
					Icon.texture = Room;
					Pointlight.color = Color.red;
					Playback.PlaybackSource.outputAudioMixerGroup = OdinHandler.Instance.PlaybackAudioMixer.FindMatchingGroups("Room")[0];
					Playback.PlaybackSource.spatialBlend = 1.0f;
					break;
				case DemoHearingType.None:
					Icon.texture = Walk;
					Pointlight.color = Color.gray;
					Playback.PlaybackSource.outputAudioMixerGroup = OdinHandler.Instance.PlaybackAudioMixer.FindMatchingGroups("Radio")[0];
					Playback.PlaybackSource.spatialBlend = 0.0f; // 0.0 makes the sound full 2D, 1.0 makes it full 3D
					break;
			}
		}

		private void CheckTalkIndicator()
		{
			Material cubeMaterial = Playback.GetComponentInParent<Renderer>().material;
			if (Playback.HasActivity)
			{
				lastCubeColor = cubeMaterial.color;
				cubeMaterial.color = Color.green;
			}
			else
				cubeMaterial.color = lastCubeColor;
		}

		#region Demo
		public bool AutoMove = true;
		private Vector3 MoveOffset;
		private float MouseZCoord;

		public float Speed = 5;
		public float WaitTime = .3f;
		public float TurnSpeed = 90;

		public Light Pointlight;
		public LayerMask HearingMask;
		private Color originalPointlightColour;

		public Light Spotlight;
		public float ViewDistance;

		private UnityEngine.UI.RawImage Icon;
		public Texture2D Area;
		public Texture2D Muff;
		public Texture2D Room;
		public Texture2D Voice;
		public Texture2D Walk;

		public Transform pathContainer;
		private GameObject Player;

		private Coroutine MoveWorker;
		private IEnumerator DoMove;

		/// <summary>
		/// Demo level ids
		/// </summary>
		private enum DemoHearingType
		{
			None,
			Blocked,
			Echo,
			Normal
		}

		void OnMouseDown()
		{
			if (AutoMove) return;
			MouseZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
			MoveOffset = gameObject.transform.position - GetMouseWorldPosition();
		}

		void OnMouseDrag()
		{
			if (AutoMove) return;
			gameObject.transform.position = MoveOffset + GetMouseWorldPosition();
		}

		private Vector3 GetMouseWorldPosition()
		{
			Vector3 mousePoint = Input.mousePosition;
			mousePoint.z = MouseZCoord;

			return Camera.main.ScreenToWorldPoint(mousePoint);
		}

		/// <summary>
		/// Demo level check preset
		/// </summary>
		/// <remarks>Should be a real calculation</remarks>
		private DemoHearingType PlayerCanHear()
		{
			if (Player == null) return DemoHearingType.None;
			if (Vector3.Distance(transform.position, Player.transform.position) < HearingDistance)
			{
				if (Physics.Linecast(transform.position, Player.transform.position, HearingMask))
				{
					if (Physics.OverlapSphere(transform.position, HearingDistance / 2, HearingMask).Length > 1)
						return DemoHearingType.Echo;
					else
						return DemoHearingType.Blocked;
				}
				else
					return DemoHearingType.Normal;
			}

			return DemoHearingType.None;
		}

		/// <summary>
		/// Demo Moving
		/// </summary>
		IEnumerator FollowPath(Vector3[] checkpoints)
		{
			transform.position = checkpoints[0];

			int targetCheckpointIndex = 1;
			Vector3 targetCheckpoint = checkpoints[targetCheckpointIndex];
			transform.LookAt(targetCheckpoint);

			while (true)
			{
				transform.position = Vector3.MoveTowards(transform.position, targetCheckpoint, Speed * Time.deltaTime);
				if (transform.position == targetCheckpoint)
				{
					targetCheckpointIndex = (targetCheckpointIndex + 1) % checkpoints.Length;
					targetCheckpoint = checkpoints[targetCheckpointIndex];
					yield return new WaitForSeconds(WaitTime);
					yield return StartCoroutine(TurnToFace(targetCheckpoint));
				}
				yield return null;
			}
		}

		/// <summary>
		/// Demo Moving
		/// </summary>
		IEnumerator TurnToFace(Vector3 lookTarget)
		{
			Vector3 directionToLookTarget = (lookTarget - transform.position).normalized;
			float targetAngle = 90 - Mathf.Atan2(directionToLookTarget.z, directionToLookTarget.x) * Mathf.Rad2Deg;

			while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
			{
				float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, TurnSpeed * Time.deltaTime);
				transform.eulerAngles = Vector3.up * angle;
				yield return null;
			}
		}

		/// <summary>
		/// Demo gizmos
		/// </summary>
		void OnDrawGizmos()
		{
			// Path
			if (pathContainer != null)
			{
				Vector3 startPosition = pathContainer.GetChild(0).position;
				Vector3 previousPosition = startPosition;

				foreach (Transform checkpoint in pathContainer)
				{
					Gizmos.DrawSphere(checkpoint.position, .3f);
					Gizmos.DrawLine(previousPosition, checkpoint.position);
					previousPosition = checkpoint.position;
				}
				Gizmos.DrawLine(previousPosition, startPosition);
			}

			// Sound
			Gizmos.DrawWireSphere(gameObject.transform.position, HearingDistance);
		}
		#endregion Demo
	}
}