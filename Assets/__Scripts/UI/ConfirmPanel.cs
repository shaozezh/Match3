using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ConfirmPanel : MonoBehaviour
{
    public string levelToLoad;
    public Image[] stars;
    public Text highScoreText;
    public Text starText;
    //public GameData gameData;
    private int starsNum;
    private int highScore;
    public int level;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void LoadData()
    {
        starsNum = GameData.Instance.saveData.stars[level - 1];
        highScore = GameData.Instance.saveData.highScores[level - 1];
    }

    void SetText()
    {
        highScoreText.text = "" + highScore;
        starText.text = "" + starsNum + "/3";
    }
    public void Cancel()
    {
        this.gameObject.SetActive(false);
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = false;
        }
    }

    public void ShowStars()
    {
        LoadData();
        ActivateStars();
        SetText();
    }

    public void Play()
    {
        PlayerPrefs.SetInt("Current Level", level - 1);
        SceneManager.LoadScene(levelToLoad);
    }

    private void ActivateStars()
    {
        // Gonna work on reading binary file to activate
        for (int i = 0; i < starsNum; i++)
        {
            stars[i].enabled = true;
        }
    }

}
