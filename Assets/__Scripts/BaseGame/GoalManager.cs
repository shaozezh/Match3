using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GoalInfo
{
    public int numberNeeded;
    public int numberCollected;
    public Sprite goalSprite;
    public string matchValue;
}
public class GoalManager : MonoBehaviour
{
    public GoalInfo[] levelGoals;
    public List<GoalPanel> currentGoals = new List<GoalPanel>();
    public GameObject goalPrefab;
    public GameObject goalPrefabSmall;
    public Transform goalIntroParent;
    public Transform goalGameParent;
    public Board board;
    public GameObject winPanel;
    public FadePanelController fadePanelController;
    public EndGameManager endGameManager;
    public List<Vector2Int> nonSpecialPositions = new List<Vector2Int>();
    public Text countdownText;
    public FindMatches findMatches;
    private int goalsCompleted = 0;
    public ScoreManager scoreManager;
    public GameObject congratsPanel;
    public CurvedLineRenderer curvedLineManager;
    //public bool isWin = false;

    // Start is called before the first frame update
    void Start()
    {
        GetGoals();
        SetupGoals();
    }
    private void GetGoals()
    {
        if (board != null && board.world != null && board.level < board.world.levels.Length)
        {
            levelGoals = board.world.levels[board.level].levelGoals;
        }
    }
    private void SetupGoals()
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            levelGoals[i].numberCollected = 0;
            GameObject goal = Instantiate(goalPrefab, goalIntroParent);
            GoalPanel panel = goal.GetComponent<GoalPanel>();
            panel.sprite = levelGoals[i].goalSprite;
            panel.stringTag = "0/" + levelGoals[i].numberNeeded;
            GameObject gameGoal = Instantiate(goalPrefabSmall, goalGameParent);
            panel = gameGoal.GetComponent<GoalPanel>();
            currentGoals.Add(panel);
            panel.sprite = levelGoals[i].goalSprite;
            panel.stringTag = "0/" + levelGoals[i].numberNeeded;
        }
    }
    
    public void UpdateGoals()
    {
        goalsCompleted = 0;
        for (int i = 0; i < levelGoals.Length; i++)
        {
            int collectNum = levelGoals[i].numberCollected;
            int needNum = levelGoals[i].numberNeeded;
            if (collectNum >= needNum)
            {
                goalsCompleted++;
                currentGoals[i].text.text = "" + needNum + "/" + needNum;
            }
            else
            {
                currentGoals[i].text.text = "" + collectNum + "/" + needNum;
            }
        }
    }

    public IEnumerator CheckWin()
    {
        if (goalsCompleted >= levelGoals.Length && board.currentState != GameState.lose)
        {
            congratsPanel.SetActive(true);
            Debug.Log("level complete");
            board.currentState = GameState.win;
            if (endGameManager.currentCounterValue != 0)
            {
                yield return StartCoroutine(BonusTiles(endGameManager.currentCounterValue));
            }
            winPanel.SetActive(true);
            fadePanelController.GameOver();
            GameData.Instance.saveData.isActive[board.level + 1] = true;
            if (board.level < board.world.levels.Length)
            {
                if (scoreManager.score >= board.world.levels[board.level].scoreGoals[2])
                {
                    GameData.Instance.saveData.stars[board.level] = 3;
                }
                else if (scoreManager.score >= board.world.levels[board.level].scoreGoals[1])
                {
                    GameData.Instance.saveData.stars[board.level] = 2;
                }
                else
                {
                    GameData.Instance.saveData.stars[board.level] = 1;
                }
            }
            GameData.Instance.Save();
            //isWin = true;
        }
    }

    public IEnumerator BonusTiles(int num)
    {
        List<Coroutine> runningCoroutines = new List<Coroutine>();
        //yield return StartCoroutine(board.FillBoard(true));
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (CheckSpecialTile(i, j))
                {
                    nonSpecialPositions.Add(new Vector2Int(i, j));
                }
            }
        }
        for (int i = 0; i < num; i++)
        {
            countdownText.text = (num - i - 1).ToString();

            if (nonSpecialPositions.Count == 0) yield break;

            // Randomly select a position
            int randomIndex = Random.Range(0, nonSpecialPositions.Count);
            Vector2Int pos = nonSpecialPositions[randomIndex];
            nonSpecialPositions.RemoveAt(randomIndex);
            //randomIndex = Random.Range(0, 3);
            Coroutine curveCoroutine =  StartCoroutine(curvedLineManager.CurvesMove(countdownText.gameObject.transform.position, board.allBlocks[pos.x, pos.y], true));
            runningCoroutines.Add(curveCoroutine);
            yield return new WaitForSeconds(0.1f);
            
            //destroy here
            //curvedLineManager.DestroyAllStars();
            //yield return new WaitForSeconds(.15f);
        }
        //countdownText.text = "0";
        //curvedLineManager.DestroyAllStars();
        foreach (Coroutine curveCoroutine in runningCoroutines)
        {
            yield return curveCoroutine;
        }
        yield return new WaitForSeconds(0.2f);
        int timeCounter = 0;
        while (SpecialsOnBoard())
        {
            List<Coroutine> starCoroutines = new List<Coroutine>();
            for (int i = 0; i < board.width; i++)
            {
                for (int j = 0; j < board.height; j++)
                {
                    if (board.allBlocks[i, j] == null)
                    {
                        continue;
                    }
                    if (board.allBlocks[i, j].tag == "col")
                    {
                        findMatches.GetColPieces(i, j);
                    }
                    else if (board.allBlocks[i, j].tag == "row")
                    {
                        findMatches.GetRowPieces(i, j);
                    }
                    else if (board.allBlocks[i, j].tag == "square")
                    {
                        findMatches.GetSquarePieces(i, j);
                    }
                    else if (board.allBlocks[i, j].tag == "star" && timeCounter != 0)
                    {
                        Block colorBlock = board.allBlocks[i, j].GetComponent<Block>();
                        //yield return StartCoroutine(findMatches.MatchColorPieces(colorBlock.hiddenTag, board.allBlocks[i, j].transform));
                        Coroutine matchColorCoroutine =  StartCoroutine(findMatches.MatchColorPieces(colorBlock.hiddenTag, board.allBlocks[i, j].transform));
                        starCoroutines.Add(matchColorCoroutine);
                        colorBlock.isMatched = true;
                    }
                }
            }
            foreach (Coroutine matchColorCoroutine in starCoroutines)
            {
                yield return matchColorCoroutine;
            }
            yield return StartCoroutine(board.DestroyMatches(true));
            yield return new WaitForSeconds(.1f);
            timeCounter++;
        }
        
    }

    private bool CheckSpecialTile(int col, int row)
    {
        if (board.allBlocks[col, row] == null)
        {
            return false;
        }
        else
        {
            if (board.allBlocks[col, row].tag == "star" || board.allBlocks[col, row].tag == "col" || board.allBlocks[col, row].tag == "row" || board.allBlocks[col, row].tag == "square")
            {
                return false;
            }
        }
        return true;
    }

    private bool SpecialsOnBoard()
    {
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allBlocks[i, j] != null)
                {
                    if (board.allBlocks[i, j].tag == "col" || board.allBlocks[i, j].tag == "row" || board.allBlocks[i, j].tag == "square" || board.allBlocks[i, j].tag == "star")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void CompareGoal(string goalToCompare)
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            if (goalToCompare == levelGoals[i].matchValue)
            {
                levelGoals[i].numberCollected++;
            }
        }
    }
}
