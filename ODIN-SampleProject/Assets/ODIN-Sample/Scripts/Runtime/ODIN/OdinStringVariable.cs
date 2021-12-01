using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Data
{
    [CreateAssetMenu(fileName = "OdinStringVariable", menuName = "Odin-Sample/StringVariable", order = 0)]
    public class OdinStringVariable : ScriptableObject
    {
        [field: SerializeField]
        public string Value { get; set; } = "default";

        public static implicit operator string(OdinStringVariable v) => v.Value;
        
        public override string ToString()
        {
            return Value;
        }
    }
}