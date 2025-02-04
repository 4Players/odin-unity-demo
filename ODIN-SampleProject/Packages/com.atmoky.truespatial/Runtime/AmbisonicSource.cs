
using UnityEngine;

namespace Atmoky
{
	[Icon("Packages/com.atmoky.truespatial/Editor/Icons/AtmokyPentatope.tiff")]
	[AddComponentMenu("Atmoky/Atmoky Ambisonic Source")]
	[RequireComponent(typeof(AudioSource))]
	[ExecuteInEditMode]
	public class AmbisonicSource : MonoBehaviour, ISerializationCallbackReceiver
	{
		[Tooltip("When enabled, the orientation of the Ambisonic Soundfield stays locked to the listener's head, meaning listener rotation won't affect the rendering.")]
		public bool headlocked = false;

		[SerializeField, HideInInspector]
		private bool isSerialized = false;

		public AudioSource LinkedAudioSource { get; private set; }

		private enum EffectData
		{
			Headlocked = 0,
		}

		void Awake()
		{
			LinkedAudioSource = GetComponent<AudioSource>();

			if (!isSerialized)
			{
				LinkedAudioSource.GetSpatializerFloat((int)EffectData.Headlocked, out float headLockedValue);
				headlocked = headLockedValue > 0.0f;
			}

			isSerialized = true;
		}

		void Update()
		{
			LinkedAudioSource.SetAmbisonicDecoderFloat((int)EffectData.Headlocked, headlocked ? 1.0f : 0.0f);
		}

		public void OnBeforeSerialize()
		{
			isSerialized = true;
		}

		public void OnAfterDeserialize() { }
	}
}
