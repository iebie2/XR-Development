using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;


public enum GameSessionState
{
    NotStarted,
    Playing,
    GameOver
}

public class GameManager : Singleton<GameManager>
{
   
    [Header("Scenes")]
    [SerializeField] private string startSceneName = "StartScreen";
    [SerializeField] private string gameSceneName = "V1";
    [SerializeField] private string endSceneName = "EndScreen";
    [SerializeField] private string winSceneName = "WinScreen";

    [Header("Game Over")]
    [SerializeField] private bool pauseOnGameOver = true;

    public event Action<string> GameEnded;

    public GameSessionState SessionState { get; private set; } = GameSessionState.NotStarted;
    public float RunTime { get; private set; }
    public string LastEndReason { get; private set; } = string.Empty;

    protected override void Awake()
    {
        base.Awake();

        if (TryGetInstance() != this)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (TryGetInstance() == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Update()
    {
        if (SessionState == GameSessionState.Playing)
        {
            RunTime += Time.deltaTime;
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SetGameplayCursor();
        RunTime = 0f;
        LastEndReason = string.Empty;
        SessionState = GameSessionState.Playing;

        if (!string.IsNullOrEmpty(gameSceneName) && SceneManager.GetActiveScene().name != gameSceneName)
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void EndGame(string reason)
    {
        if (SessionState == GameSessionState.GameOver)
        {
            return;
        }

        LastEndReason = reason;
        SessionState = GameSessionState.GameOver;
        Debug.Log($"Game over: {LastEndReason}");
        GameEnded?.Invoke(LastEndReason);

        if (pauseOnGameOver)
        {
            Time.timeScale = 0f;
        }

        if (!string.IsNullOrEmpty(endSceneName))
        {
            Time.timeScale = 1f;
            Debug.Log($"Loading end scene: {endSceneName}");
            SceneManager.LoadScene(endSceneName);
        }
    }

    public void WinGame()
    {
        if (SessionState == GameSessionState.GameOver)
        {
            return;
        }

        LastEndReason = "Player reached the water tower";
        SessionState = GameSessionState.GameOver;
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(winSceneName))
        {
            SceneManager.LoadScene(winSceneName);
        }
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void ReturnToStartScreen()
    {
        Time.timeScale = 1f;
        SetMenuCursor();
        SessionState = GameSessionState.NotStarted;

        if (!string.IsNullOrEmpty(startSceneName))
        {
            SceneManager.LoadScene(startSceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == startSceneName || scene.name == endSceneName || scene.name == winSceneName)
        {
            SetMenuCursor();
        }

        if (scene.name == gameSceneName)
        {
            SetGameplayCursor();
        }

        if (scene.name == gameSceneName && SessionState != GameSessionState.Playing)
        {
            StartGame();
        }
    }

    private static void SetGameplayCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void SetMenuCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

   
}

public static class XRRuntimeSupport
{
    private static readonly List<XRDisplaySubsystem> XRDisplaySubsystems = new List<XRDisplaySubsystem>();

    public static bool IsActive => XRSettings.enabled && (XRSettings.isDeviceActive || IsXRDisplayRunning());

    public static Vector2 GetMoveAxis()
    {
        if (TryGetAxis(XRNode.LeftHand, CommonUsages.primary2DAxis, out Vector2 axis))
        {
            return axis;
        }

        if (TryGetAxis(XRNode.RightHand, CommonUsages.primary2DAxis, out axis))
        {
            return axis;
        }

        return Vector2.zero;
    }

    public static bool GetJumpPressed()
    {
        return GetButton(XRNode.RightHand, CommonUsages.primaryButton) ||
               GetButton(XRNode.LeftHand, CommonUsages.primaryButton);
    }

    public static bool GetUISubmitPressed()
    {
        return GetButton(XRNode.RightHand, CommonUsages.triggerButton) ||
               GetButton(XRNode.LeftHand, CommonUsages.triggerButton) ||
               GetJumpPressed();
    }

    public static bool GetCrouchHeld()
    {
        return GetButton(XRNode.RightHand, CommonUsages.secondaryButton) ||
               GetButton(XRNode.LeftHand, CommonUsages.secondaryButton);
    }

    public static bool GetMenuPressed()
    {
        return GetButton(XRNode.LeftHand, CommonUsages.menuButton) ||
               GetButton(XRNode.RightHand, CommonUsages.menuButton) ||
               GetButton(XRNode.LeftHand, CommonUsages.secondaryButton);
    }

    public static bool TryGetHeadPose(out Vector3 localPosition, out Quaternion localRotation)
    {
        InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
        bool hasPosition = head.TryGetFeatureValue(CommonUsages.devicePosition, out localPosition);
        bool hasRotation = head.TryGetFeatureValue(CommonUsages.deviceRotation, out localRotation);
        return hasPosition || hasRotation;
    }

    public static bool TryGetControllerPose(XRNode node, out Vector3 position, out Quaternion rotation)
    {
        InputDevice controller = InputDevices.GetDeviceAtXRNode(node);
        bool hasPosition = controller.TryGetFeatureValue(CommonUsages.devicePosition, out position);
        bool hasRotation = controller.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);
        return hasPosition && hasRotation;
    }

    public static bool TryGetControllerWorldPose(XRNode node, out Vector3 position, out Quaternion rotation)
    {
        if (!TryGetControllerPose(node, out Vector3 localPosition, out Quaternion localRotation))
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        Transform trackingOrigin = GetTrackingOrigin();
        if (trackingOrigin == null)
        {
            position = localPosition;
            rotation = localRotation;
            return true;
        }

        position = trackingOrigin.TransformPoint(localPosition);
        rotation = trackingOrigin.rotation * localRotation;
        return true;
    }

    private static Transform GetTrackingOrigin()
    {
        Camera camera = Camera.main;
        return camera != null && camera.transform.parent != null ? camera.transform.parent : null;
    }

    public static void ConfigureCanvasForXR(Canvas canvas, Vector3 localPosition, Vector2 size, bool followCamera = true)
    {
        if (canvas == null || !IsActive || Camera.main == null)
        {
            return;
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 100;

        Transform canvasTransform = canvas.transform;
        if (followCamera)
        {
            canvasTransform.SetParent(Camera.main.transform, false);
            canvasTransform.localPosition = localPosition;
            canvasTransform.localRotation = Quaternion.identity;
        }
        else
        {
            canvasTransform.SetParent(null, false);
            canvasTransform.position = Camera.main.transform.TransformPoint(localPosition);
            canvasTransform.rotation = Camera.main.transform.rotation;
        }

        canvasTransform.localScale = Vector3.one * 0.0015f;

        RectTransform rectTransform = canvas.GetComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
        }
    }

    public static void PlaceCanvasInViewport(Canvas canvas, Vector2 viewportPosition, float distance, Vector2 size)
    {
        if (canvas == null || !IsActive || Camera.main == null)
        {
            return;
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 100;

        Transform canvasTransform = canvas.transform;
        canvasTransform.SetParent(null, false);
        canvasTransform.position = Camera.main.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, distance));
        canvasTransform.rotation = Camera.main.transform.rotation;
        canvasTransform.localScale = Vector3.one * 0.0015f;

        RectTransform rectTransform = canvas.GetComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
        }
    }

    private static bool TryGetAxis(XRNode node, InputFeatureUsage<Vector2> usage, out Vector2 value)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        return device.TryGetFeatureValue(usage, out value);
    }

    private static bool GetButton(XRNode node, InputFeatureUsage<bool> usage)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        return device.TryGetFeatureValue(usage, out bool isPressed) && isPressed;
    }

    private static bool IsXRDisplayRunning()
    {
        XRDisplaySubsystems.Clear();
        SubsystemManager.GetSubsystems(XRDisplaySubsystems);

        foreach (XRDisplaySubsystem subsystem in XRDisplaySubsystems)
        {
            if (subsystem != null && subsystem.running)
            {
                return true;
            }
        }

        return false;
    }
}

public class XRUIButtonPointer : MonoBehaviour
{
    private const float RayLength = 8f;
    private const string GameSceneName = "V1";
    private const float GameplayRayPitchOffset = 25f;
    private const float MenuRayPitchOffset = 0f;

    private static XRUIButtonPointer instance;

    private LineRenderer lineRenderer;
    private Button hoveredButton;
    private bool wasPrimaryPressed;

    public static bool IsPointingAtButton => instance != null && instance.hoveredButton != null;

    public static void EnsureExists()
    {
        if (!XRRuntimeSupport.IsActive || instance != null)
        {
            return;
        }

        GameObject pointerObject = new GameObject("XR UI Pointer");
        instance = pointerObject.AddComponent<XRUIButtonPointer>();
        DontDestroyOnLoad(pointerObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.003f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.cyan;
    }

    private void Update()
    {
        if (!XRRuntimeSupport.IsActive)
        {
            hoveredButton = null;
            lineRenderer.enabled = false;
            return;
        }

        GetPointerRay(out Vector3 origin, out Vector3 direction);
        UpdateHoveredButton(origin, direction);
        UpdateLine(origin, direction);
        HandleClick();
    }

    private static void GetPointerRay(out Vector3 origin, out Vector3 direction)
    {
        if (XRRuntimeSupport.TryGetControllerWorldPose(XRNode.RightHand, out origin, out Quaternion rotation) ||
            XRRuntimeSupport.TryGetControllerWorldPose(XRNode.LeftHand, out origin, out rotation))
        {
            origin = ClampOriginInFrontOfCamera(origin);
            float pitchOffset = SceneManager.GetActiveScene().name == GameSceneName ? GameplayRayPitchOffset : MenuRayPitchOffset;
            direction = rotation * Quaternion.Euler(pitchOffset, 0f, 0f) * Vector3.forward;
            return;
        }

        Camera camera = Camera.main;
        if (camera != null)
        {
            origin = camera.transform.position;
            direction = camera.transform.forward;
            return;
        }

        origin = Vector3.zero;
        direction = Vector3.forward;
    }

    private static Vector3 ClampOriginInFrontOfCamera(Vector3 origin)
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            return origin;
        }

        Vector3 cameraSpaceOrigin = camera.transform.InverseTransformPoint(origin);

        if (cameraSpaceOrigin.z > 0.05f && cameraSpaceOrigin.z < 1.2f)
        {
            return origin;
        }

        Vector3 fallbackLocalOrigin = new Vector3(0.25f, -0.2f, 0.35f);
        return camera.transform.TransformPoint(fallbackLocalOrigin);
    }

    private void UpdateHoveredButton(Vector3 origin, Vector3 direction)
    {
        TryFindButton(origin, direction, out hoveredButton, out _);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(hoveredButton != null ? hoveredButton.gameObject : null);
        }
    }

    private static bool TryFindButton(Vector3 origin, Vector3 direction, out Button hitButton, out Vector3 hitPoint)
    {
        hitButton = null;
        hitPoint = Vector3.zero;
        float closestDistance = float.MaxValue;
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (!button.interactable || !button.gameObject.activeInHierarchy)
            {
                continue;
            }

            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                continue;
            }

            Plane buttonPlane = new Plane(rectTransform.forward, rectTransform.position);
            Ray ray = new Ray(origin, direction.normalized);

            if (!buttonPlane.Raycast(ray, out float distance) || distance < 0f || distance > RayLength)
            {
                continue;
            }

            Vector3 point = ray.GetPoint(distance);
            Vector3 localPoint = rectTransform.InverseTransformPoint(point);

            if (!rectTransform.rect.Contains(new Vector2(localPoint.x, localPoint.y)))
            {
                continue;
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                hitButton = button;
                hitPoint = point;
            }
        }

        return hitButton != null;
    }

    private void UpdateLine(Vector3 origin, Vector3 direction)
    {
        lineRenderer.enabled = true;
        float length = TryFindButton(origin, direction, out _, out Vector3 hitPoint) ? Vector3.Distance(origin, hitPoint) : RayLength;
        lineRenderer.startColor = hoveredButton != null ? Color.green : Color.cyan;
        lineRenderer.endColor = lineRenderer.startColor;
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, origin + direction.normalized * length);
    }

    private void HandleClick()
    {
        bool primaryPressed = XRRuntimeSupport.GetUISubmitPressed();

        if (primaryPressed && !wasPrimaryPressed && hoveredButton != null)
        {
            hoveredButton.onClick.Invoke();
        }

        wasPrimaryPressed = primaryPressed;
    }
}
