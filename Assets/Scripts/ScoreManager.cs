using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Manager")]
    public int kills;
    public int enemyKills;
    public Text playerKillCounter;
    public Text enemyKillCounter;
    public Text Maintext;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("kills"))
        {
            kills = PlayerPrefs.GetInt("0");
        }
        else if (PlayerPrefs.HasKey("enemyKills"))
        {
            enemyKills = PlayerPrefs.GetInt("0");
        }
    }
    private void Update()
    {
        StartCoroutine(WinOrLose());
    }
    IEnumerator WinOrLose()
    {
        playerKillCounter.text = "" + kills;
        enemyKillCounter.text = "" + enemyKills;

        if(kills >= 50)
        {
            Maintext.text = "Blue Team Victory";
            Maintext.color = Color.blue;
            PlayerPrefs.SetInt("kills", kills);
            Time.timeScale = 0f;
            yield return new WaitForSeconds(5f);
            Application.Quit();
        }
        else if(enemyKills >= 50)
        {
            Maintext.text = "Red Team Victory";
            Maintext.color = Color.red;
            PlayerPrefs.SetInt("enemyKills", enemyKills);
            Time.timeScale = 0f;
            yield return new WaitForSeconds(5f);
            Application.Quit();
        }
    }
    public void CharacterLose()
    {
        StartCoroutine(CharacterLoseCoroutine());
    }
    IEnumerator CharacterLoseCoroutine()
    {
        Debug.Log("CharacterLoseCoroutine ½ÇÇà!");
        Maintext.text = "Red Team Victory";
        Maintext.color = Color.red;
        PlayerPrefs.SetInt("enemyKills", enemyKills);
        Time.timeScale = 0f;
        yield return new WaitForSeconds(5f);
        Application.Quit();
    }
}
