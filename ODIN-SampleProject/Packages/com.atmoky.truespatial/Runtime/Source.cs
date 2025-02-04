
using UnityEngine;
using System;

namespace Atmoky
{
	[Icon("Packages/com.atmoky.truespatial/Editor/Icons/AtmokyPentatope.tiff")]
	[AddComponentMenu("Atmoky/Atmoky Source")]
	[RequireComponent(typeof(AudioSource))]
	[ExecuteInEditMode]
	public class Source : MonoBehaviour, ISerializationCallbackReceiver
	{
		[Range(0, 100), Tooltip("Index of the renderer to use for spatialization.")]
		public int rendererIndex = 0;

		[Range(0, 100), Tooltip("Index of the receiver to use for send.")]
		public int receiverIndex = 0;

		[Range(0.0f, 1.0f), Tooltip("Sets the amount of occlusion applied to the source. 0.0 = no occlusion, 1.0 = full occlusion.")]
		public float occlusion = 0.0f;


		[Range(0, 1), Tooltip("Send Level Source. 0 = Parameter, 1 = Reverb Zone Mix.")]
		public int sendLevelSrc = 0;

		[Range(-80.0f, 10.0f), Tooltip("Send level in decibels (will be used if Send Level Source is set to 'Parameter').")]
		public float sendLevel = 0.0f;


		[Range(0.0f, 360.0f), Tooltip("Inner angle of the sound cone where sound is not attenuated.")]
		public float innerAngle = 90f;

		[Range(0.0f, 360.0f), Tooltip("Outer angle of the sound cone where sound is attenuated.")]
		public float outerAngle = 270.0f;

		[Range(0.0f, 1.0f), Tooltip("Amount of gain applied to sound outside the sound cone.")]
		public float outerGain = 1.0f;

		[Range(0.0f, 1.0f), Tooltip("Lowpass intensity applied outside the sound cone.")]
		public float outerLowpass = 0.0f;


		[Range(0.0f, 5.0f), Tooltip("Distance at which nearfield effects start to take place.")]
		public float nfeDistance = 1.0f;

		[Range(0.0f, 10.0f), Tooltip("Amount of gain in decibels applied to sounds inside the nearfield.")]
		public float nfeGain = 0.0f;

		[Range(0.0f, 10.0f), Tooltip("Bass boost in decibels applied to sounds inside the nearfield.")]
		public float nfeBassBoost = 0.0f;


		[SerializeField, HideInInspector]
		private bool isSerialized = false;

		public AudioSource LinkedAudioSource { get; private set; }

		private enum EffectData
		{
			RendererIndex = 0,
			Occlusion = 1,
			DirectivityInnerAngle = 2,
			DirectivityOuterAngle = 3,
			DirectivityOuterGain = 4,
			DirectivityOuterLowpass = 5,
			NearfieldDistance = 6,
			NearfieldGain = 7,
			NearfieldBassBoost = 8,
			ReceiverIndex = 9,
			SendLevelSrc = 10,
			SendLevel = 11
		}

		void Awake()
		{
			LinkedAudioSource = GetComponent<AudioSource>();

			if (LinkedAudioSource == null)
			{
				Debug.LogError("atmokySpatializer requires an AudioSource component");
				enabled = false;
				return;
			}

			if (!isSerialized)
			{
				if (LinkedAudioSource.GetSpatializerFloat((int)EffectData.RendererIndex, out float stateRendererIndex))
					rendererIndex = (int)stateRendererIndex;
				if (LinkedAudioSource.GetSpatializerFloat((int)EffectData.ReceiverIndex, out float stateReceiverIndex))
					receiverIndex = (int)stateReceiverIndex;
				LinkedAudioSource.GetSpatializerFloat((int)EffectData.Occlusion, out occlusion);
				LinkedAudioSource.GetSpatializerFloat((int)EffectData.DirectivityInnerAngle, out innerAngle);
				LinkedAudioSource.GetSpatializerFloat((int)EffectData.DirectivityOuterAngle, out outerAngle);
				LinkedAudioSource.GetSpatializerFloat((int)EffectData.DirectivityOuterGain, out outerGain);
				LinkedAudioSource.GetSpatializerFloat((int)EffectData.DirectivityOuterLowpass, out outerLowpass);
				LinkedAudioSource.GetSpatializerFloat((int)EffectData.NearfieldDistance, out nfeDistance);
				LinkedAudioSource.GetSpatializerFloat((int)EffectData.NearfieldGain, out nfeGain);
				LinkedAudioSource.GetSpatializerFloat((int)EffectData.NearfieldBassBoost, out nfeBassBoost);

				if (LinkedAudioSource.GetSpatializerFloat((int)EffectData.SendLevelSrc, out float stateSendLevelSrc))
					sendLevelSrc = (int)stateSendLevelSrc;

				LinkedAudioSource.GetSpatializerFloat((int)EffectData.SendLevel, out sendLevel);
			}

			isSerialized = true;
		}


		void Update()
		{
			SendUpdate();
		}

		private void SendUpdate()
		{
			if (LinkedAudioSource == null)
				return;

			LinkedAudioSource.SetSpatializerFloat((int)EffectData.Occlusion, occlusion);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.RendererIndex, rendererIndex);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.DirectivityInnerAngle, innerAngle);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.DirectivityOuterAngle, outerAngle);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.DirectivityOuterGain, outerGain);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.DirectivityOuterLowpass, outerLowpass);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.NearfieldDistance, nfeDistance);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.NearfieldGain, nfeGain);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.NearfieldBassBoost, nfeBassBoost);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.ReceiverIndex, receiverIndex);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.SendLevelSrc, sendLevelSrc);
			LinkedAudioSource.SetSpatializerFloat((int)EffectData.SendLevel, sendLevel);
		}

		public void OnBeforeSerialize()
		{
			isSerialized = true;
		}

		public void OnAfterDeserialize() { }
	}
}
