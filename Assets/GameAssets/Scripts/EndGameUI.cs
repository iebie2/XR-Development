using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    public void ReStartGameButton()
    {
        GameManager.Instance.RestartGame();
    }
    public void StartScreenButton()
    {
        GameManager.Instance.ReturnToStartScreen();
    }
}
