using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (scene.name == startSceneName || scene.name == endSceneName)
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
