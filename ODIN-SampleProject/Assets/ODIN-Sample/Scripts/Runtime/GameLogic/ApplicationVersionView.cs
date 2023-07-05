using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    [RequireComponent(typeof(TMP_Text))]
    public class ApplicationVersionView : MonoBehaviour
    {
        private TMP_Text _text;
    
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            Assert.IsNotNull(_text);

            _text.text = "v"+Application.version;
        }
    }
}
