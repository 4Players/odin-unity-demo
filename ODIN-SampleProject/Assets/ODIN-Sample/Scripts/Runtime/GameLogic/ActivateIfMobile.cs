using UnityEngine;

public class ActivateIfMobile : MonoBehaviour
{
    [SerializeField] private bool enableInEditor;

    // Start is called before the first frame update
    private void Awake()
    {
        bool bShouldEnableInEditor = enableInEditor && Application.isEditor;
        if (!(Application.isMobilePlatform || bShouldEnableInEditor)) gameObject.SetActive(false);
    }
}