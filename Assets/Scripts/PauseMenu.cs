using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject go_BaseUi;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!GameManager.isPause)
                CallMenu();
            else
                CloseMenu();
        }
    }

    private void CallMenu()
    {
        GameManager.isPause = true;
        go_BaseUi.SetActive(true);
       // Time.timeScale = 0f;
    }

    private void CloseMenu()
    {
        GameManager.isPause = false;
        go_BaseUi.SetActive(false);
       // Time.timeScale = 1f;
    }

    public void ClickExit()
    {
        Debug.Log("게임종료");
        Application.Quit();
    }
}
