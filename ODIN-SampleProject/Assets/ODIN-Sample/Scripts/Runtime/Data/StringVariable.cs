using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Data
{
    [CreateAssetMenu(fileName = "StringVariable", menuName = "Odin-Sample/StringVariable", order = 0)]
    public class StringVariable : ScriptableObject
    {
        [field: SerializeField]
        public string Value { get; set; } = "default";

        public override string ToString()
        {
            return Value;
        }
    }
}