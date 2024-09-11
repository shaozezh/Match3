using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelButton : MonoBehaviour
{
    public bool isActive;
    public Sprite activeSprite;
    public Sprite lockedSprite;
    private Image buttonImage;
    private Button thisButton;
    private int starsNum;
    public Image[] stars;
    public Text levelText;
    public int level;
    public ConfirmPanel confirmPanel;
    
    //public GameData gameData;
    // Start is called before the first frame update
    void Start()
    {
        buttonImage = GetComponent<Image>();
        thisButton = GetComponent<Button>();
        LoadData();
        ActivateStars();
        ShowLevel();
        DecideSprite();
    }

    private void LoadData()
    {
        if (GameData.Instance.saveData.isActive[level - 1])
        {
            isActive = true;
        }
        else
        {
            isActive = false;
        }
        starsNum = GameData.Instance.saveData.stars[level - 1];
    }

    private void ActivateStars()
    {
        // Gonna work on reading binary file to activate
        for (int i = 0; i < starsNum; i++)
        {
            stars[i].enabled = true;
        }
    }

    private void DecideSprite()
    {
        if (isActive)
        {
            buttonImage.sprite = activeSprite;
            thisButton.enabled = true;
            levelText.enabled = true;
        }
        else
        {
            buttonImage.sprite = lockedSprite;
            thisButton.enabled = false;
            levelText.enabled = false;
        }
    }

    private void ShowLevel()
    {
        levelText.text = "" + level;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConfirmPanel(int level)
    {
        if (confirmPanel.gameObject.activeSelf)
        {
            return;
        }
        confirmPanel.level = level;
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.ShowStars();
    }
}
