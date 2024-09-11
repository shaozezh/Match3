using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameType
{
    Moves,
    Time
}

[System.Serializable]
public class EndGameRequirements
{
    public GameType gameType;
    public int counterValue;
}

public class EndGameManager : MonoBehaviour
{
    public EndGameRequirements requirements;
    public GameObject movesLabel;
    public GameObject timeLabel;
    public Text counter;
    public int currentCounterValue;
    private float timerSeconds;
    public bool isReady = false;
    public GoalManager goalManager;
    public Board board;
    public GameObject losePanel;
    public FadePanelController fadePanelController;

    // Start is called before the first frame update
    void Start()
    {
        SetGameRequirements();
        SetupLevel();
    }

    private void SetGameRequirements()
    {
        if (board != null && board.world != null && board.level < board.world.levels.Length)
        {
            requirements = board.world.levels[board.level].endGameRequirements;
        }
    }

    private void SetupLevel()
    {
        currentCounterValue = requirements.counterValue;
        if (requirements.gameType == GameType.Moves)
        {
            movesLabel.SetActive(true);
            timeLabel.SetActive(false);
        }
        else
        {
            timerSeconds = 1;
            movesLabel.SetActive(false);
            timeLabel.SetActive(true);
        }
        counter.text = "" + currentCounterValue;
    }

    public void DecreaseCounterValue()
    {
        currentCounterValue--;
        counter.text = "" + currentCounterValue;    
    }

    void Update()
    {
        if (board.currentState != GameState.pause && board.currentState != GameState.win)
        {
            if (requirements.gameType == GameType.Time && currentCounterValue > 0)
            {
                timerSeconds -= Time.deltaTime;
                if (timerSeconds <= 0)
                {
                    DecreaseCounterValue();
                    CheckLoseCondition();
                    timerSeconds = 1;
                }
            }
        }
    }

    public void CheckLoseCondition()
    {
        if (currentCounterValue == 0 && board.currentState != GameState.win)
        {
            losePanel.SetActive(true);
            board.currentState = GameState.lose;
            fadePanelController.GameOver();
            Debug.Log("try again");
        }
    }
}
