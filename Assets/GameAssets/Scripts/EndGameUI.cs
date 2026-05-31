using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    private const string GameSceneName = "V1";
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

    public void ReStartGameButton()
    {
        GameManager.Instance.RestartGame();
    }

    public void StartScreenButton()
    {
        GameManager.Instance.ReturnToStartScreen();
    }

    private void ConfigureCanvasForXR()
    {
        if (configuredForXR || !XRRuntimeSupport.IsActive)
        {
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            return;
        }

        Vector3 position = SceneManager.GetActiveScene().name == GameSceneName
            ? new Vector3(0.55f, 0.32f, 1.5f)
            : new Vector3(0f, 0f, 1.5f);
        Vector2 size = SceneManager.GetActiveScene().name == GameSceneName
            ? new Vector2(220f, 70f)
            : new Vector2(900f, 600f);

        XRRuntimeSupport.ConfigureCanvasForXR(canvas, position, size);

        XRUIButtonPointer.EnsureExists();
        configuredForXR = true;
    }
}
  

   
