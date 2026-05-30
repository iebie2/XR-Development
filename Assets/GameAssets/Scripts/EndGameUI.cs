using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

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
  

   