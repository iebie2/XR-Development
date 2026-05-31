using UnityEngine;
using UnityEngine.UI;

public class StartScreenUI : MonoBehaviour
{
    private bool configuredForXR;

    private void Start()
    {
        ConfigureCanvasForXR();
    }

    private void Update()
    {
        if (!XRRuntimeSupport.IsActive)
        {
            return;
        }

        ConfigureCanvasForXR();

        XRUIButtonPointer.EnsureExists();
    }

    public void StartGameButton()
    {
        GameManager.Instance.StartGame();
    }

    private void ConfigureCanvasForXR()
    {
        if (configuredForXR || !XRRuntimeSupport.IsActive)
        {
            return;
        }

        XRRuntimeSupport.ConfigureCanvasForXR(GetComponentInParent<Canvas>(), new Vector3(0f, 0f, 1.5f), new Vector2(800f, 500f));
        XRUIButtonPointer.EnsureExists();
        configuredForXR = true;
    }
}
