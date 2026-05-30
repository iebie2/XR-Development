using UnityEngine;

public class StartScreenUI : MonoBehaviour
{
    public void StartGameButton()
    {
        GameManager.Instance.StartGame();
    }
}