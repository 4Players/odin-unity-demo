using UnityEngine;

namespace Atmoky
{
    [Icon("Packages/com.atmoky.truespatial/Editor/Icons/AtmokyPentatope.tiff")]
    [AddComponentMenu("Atmoky/Atmoky Occluder")]
    public class Occluder : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float occlusion = 0.0f;
    }
}
