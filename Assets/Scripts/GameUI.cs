using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        Debug.Log("Back to menu clicked!");
        SceneManager.LoadScene("MainMenu");
    }
}
