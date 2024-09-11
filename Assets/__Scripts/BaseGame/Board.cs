using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum GameState 
{
    wait,
    move,
    win,
    lose,
    pause

}

public enum Tiles
{
    Breakable,
    Blank,
    Normal
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public Tiles tiles;
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int width;
    public int height;
    public int offset;
    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject[] blocks;
    public bool[,] blankSpaces;
    public BackgroundTile[,] breakableTiles;
    public GameObject[,] allBlocks;
    public int activeCoroutines = 0;
    [SerializeField] private FindMatches findMatches;
    public GameObject[] destroyVFX;
    public Block currentBlock;
    public bool[,] isMatched;
    public float blockSize;
    public TileType[] boardLayout;
    public ScoreManager scoreManager;
    public int baseValue = 20;
    public int streakValue = 1;
    public int[] scoreGoals;
    public float lerpSpeed;
    public bool isPause = false;
    public List<GameObject> blockPool;
    public List<GameObject> lockedBlock;
    public GoalManager goalManager;
    public World world;
    public int level;
    public Sprite rowArrow;
    public Sprite colArrow;
    public Sprite squareBomb;

    void Awake()
    {
        if (PlayerPrefs.HasKey("Current Level"))
        {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if (world != null)
        {
            if (level < world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    blocks = world.levels[level].blocks;
                    scoreGoals = world.levels[level].scoreGoals;
                    boardLayout = world.levels[level].boardLayout;
                }
            }
        }
    }
    void Start()
    {
        breakableTiles = new BackgroundTile[width, height];
        blankSpaces = new bool[width, height];
        allBlocks = new GameObject[width, height];
        isMatched = new bool[width, height];
        SetUp();
        //blockSize = (allBlocks[1, 0].transform.position - allBlocks[0, 0].transform.position).magnitude / 2;
        blockSize = .5f;
        currentState = GameState.pause;
    }

    public void GenerateBackgroundTiles()
    {
        for (int i = 0; i < boardLayout.Length; i++)
        {
            if (boardLayout[i].tiles == Tiles.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
            else if (boardLayout[i].tiles == Tiles.Breakable)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
                Vector2 tempPos = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, tempPos, Quaternion.identity, this.transform);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void SetUp()
    {
        GenerateBackgroundTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (blankSpaces[i,j])
                {
                    if (breakableTiles[i,j] != null)
                    {
                        Vector3 zeroPos = new Vector3(0f, 0f, -30f);
                        GameObject zeroBlock = Instantiate(blocks[0], zeroPos, Quaternion.identity, this.transform);
                        zeroBlock.GetComponent<Block>().enabled = false;
                        zeroBlock.name = " ";
                        lockedBlock.Add(zeroBlock);
                        zeroPos = new Vector2(i, j);
                        Instantiate(tilePrefab, zeroPos, Quaternion.identity, this.transform);
                    }
                    continue;
                }
                Vector2 tempPos = new Vector2(i, j);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPos, Quaternion.identity, this.transform);
                backgroundTile.name = "( " + i + ", "  + j +" )";
                tempPos.y += offset;
                int blockToUse = Random.Range(0, blocks.Length);
                int maxIterations = 0;
                while(MatchesAt(i, j, blocks[blockToUse]) && maxIterations < 100)
                {
                    maxIterations++;
                    blockToUse = Random.Range(0, blocks.Length);
                }
                GameObject block = Instantiate(blocks[blockToUse], tempPos, Quaternion.identity, this.transform);
                block.GetComponent<Block>().col = i;
                block.GetComponent<Block>().row = j;
                block.name = "( " + i + ", "  + j +" )";
                allBlocks[i, j] = block;
            }
        }
        if (IsDeadlocked())
        {
            ShuffleBoard();
        }
    }

    private bool MatchesAt(int col, int row, GameObject block)
    {
        if (col > 1 && row > 1)
        {
            if (allBlocks[col - 1, row] != null && allBlocks[col - 2, row] != null)
            {
                if (allBlocks[col - 1, row].tag == block.tag && allBlocks[col - 2, row].tag == block.tag)
                {
                    return true;
                }
            }
            if (allBlocks[col, row - 1] != null && allBlocks[col, row - 2] != null)
            {
                if (allBlocks[col, row - 1].tag == block.tag && allBlocks[col, row - 2].tag == block.tag)
                {
                    return true;
                }
            }
        }
        else if (col <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allBlocks[col, row - 1] != null && allBlocks[col, row - 2] != null)
                {
                    if (allBlocks[col, row - 1].tag == block.tag && allBlocks[col, row - 2].tag == block.tag)
                    {
                        return true;
                    }
                }
                
            }
            if (col > 1)
            {
                if (allBlocks[col - 1, row] != null && allBlocks[col - 2, row] != null)
                {
                    if (allBlocks[col - 1, row].tag == block.tag && allBlocks[col - 2, row].tag == block.tag)
                    {
                        return true;
                    }
                }
                
            }
        }
        return false;
    }

    private IEnumerator DestroyMatchesAt(/*int col, int row, */bool isBonus)
    {
        List<Coroutine> coroutines = new List<Coroutine>();
        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allBlocks[col, row] == null)
                {
                    continue;
                }
                Block block = allBlocks[col, row].GetComponent<Block>();
                if (block.isMatched)
                {
                    block.enabled = false;
                    /*if (block.transform.childCount > 0)
                    {
                        if (block.transform.GetChild(0).tag == "row")
                        {
                            GameObject tempRocket = Instantiate(leftRocket, block.transform.position, Quaternion.identity);
                            coroutines.Add(StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(Camera.main.transform.position.x - Camera.main.transform.position.y, block.transform.position.y))));
                            tempRocket = Instantiate(rightRocket, block.transform.position, Quaternion.identity);
                            coroutines.Add(StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(Camera.main.transform.position.x + Camera.main.transform.position.y, block.transform.position.y))));
                        }
                        else if (block.transform.GetChild(0).tag == "col")
                        {
                            GameObject tempRocket = Instantiate(upRocket, block.transform.position, Quaternion.identity);
                            coroutines.Add(StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(block.transform.position.x, 2 * Camera.main.transform.position.y + 3.5f))));
                            tempRocket = Instantiate(downRocket, block.transform.position, Quaternion.identity);
                            coroutines.Add(StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(block.transform.position.x, -3.5f))));
                        }
                    }*/
                    for (int k = 0; k < destroyVFX.Length; k++)
                    {
                        if (allBlocks[col, row].tag == destroyVFX[k].tag)
                        {
                            GameObject vfx = Instantiate(destroyVFX[k], allBlocks[col, row].transform.position, Quaternion.identity);
                            Destroy(vfx, .2f);
                            break;
                        }
                    }
                    if (allBlocks[col, row].tag == "col" || allBlocks[col, row].tag == "row")
                    {
                        scoreManager.IncreaseScore(4 * baseValue * streakValue);
                    }
                    else if (allBlocks[col, row].tag == "square")
                    {
                        scoreManager.IncreaseScore(5 * baseValue * streakValue);
                    }
                    else if (allBlocks[col, row].tag == "star")
                    {
                        scoreManager.IncreaseScore(6 * baseValue * streakValue);
                    }
                    else
                    {
                        scoreManager.IncreaseScore(baseValue * streakValue);
                    }
                    //allBlocks[col, row].GetComponent<Block>().enabled = false;
                    //allBlocks[col, row].GetComponent<Block>().isMatched = false;
                    if (!isBonus)
                    {
                        goalManager.CompareGoal(allBlocks[col, row].tag);
                        goalManager.UpdateGoals();
                        //yield return StartCoroutine(goalManager.UpdateGoals());
                    }
                    allBlocks[col, row].transform.position = new Vector3(0f,0f,-30f);
                    blockPool.Add(allBlocks[col, row]);
                    allBlocks[col, row] = null;
                    currentBlock = null;
                }
            }
        }
        if (coroutines.Count != 0)
        {
            foreach (var coroutine in coroutines)
            {
                //Debug.Log(coroutine);
                yield return coroutine;
            }
        }
    }

    public IEnumerator DestroyMatches(bool isBonus)
    {
        isPause = true;
        yield return StartCoroutine(CheckBombMatches());
        yield return StartCoroutine(DestroyMatchesAt(isBonus));
        yield return new WaitForSeconds(.2f);
        yield return StartCoroutine(DecreaseNonBlankRow());
        isPause = false;
        yield return new WaitForSeconds(.1f);
        yield return StartCoroutine(FillBoard(isBonus));
    }

    public IEnumerator CheckBombMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                isMatched[i, j] = false;
            }
        }

        var coroutine1 = StartCoroutine(CheckNumMatches(5));
        var coroutine2 = StartCoroutine(CheckLShapeMatches());
        var coroutine3 = StartCoroutine(CheckNumMatches(4));

        yield return coroutine1;
        yield return coroutine2;
        yield return coroutine3;
        //yield return new WaitForSeconds(.1f);
    }

    private IEnumerator CheckNumMatches(int num)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!isMatched[i, j])
                {
                    if (CheckHorizontalMatch(i, j, num))
                    {
                        if (num == 5)
                        {
                            /*if (allBlocks[i + num/2, j].transform.childCount != 0)
                            {
                                foreach (Transform child in allBlocks[i + num/2, j].transform)
                                {
                                    Destroy(child.gameObject);
                                }
                            }*/
                            allBlocks[i + num/2, j].GetComponent<Block>().BecomeStar();
                            Vector2 targetPosition = allBlocks[i + num / 2, j].transform.position;
                            GameObject[] blocksToMove = new GameObject[5];
                            for (int k = 0; k < 5; k++)
                            {
                                blocksToMove[k] = allBlocks[i + k, j];
                            }
                            yield return StartCoroutine(MoveBlocksToBomb(targetPosition, blocksToMove));
                        }
                        else if (num == 4)
                        {
                            if (currentBlock != null && (allBlocks[i + num/2 - 1, j] == currentBlock.gameObject || (currentBlock.otherBlock != null && allBlocks[i + num/2 - 1, j] == currentBlock.otherBlock.gameObject)))
                            {
                                /*if (allBlocks[i + num/2 - 1, j].transform.childCount != 0)
                                {
                                    foreach (Transform child in allBlocks[i + num/2 - 1, j].transform)
                                    {
                                        Destroy(child.gameObject);
                                    }
                                }*/
                                allBlocks[i + num/2 - 1, j].GetComponent<Block>().MakeColBomb();
                                //allBlocks[i + num/2 - 1, j].GetComponent<Block>().isMatched = false;
                                Vector2 targetPosition = allBlocks[i + num / 2 - 1, j].transform.position;
                                GameObject[] blocksToMove = new GameObject[4];
                                for (int k = 0; k < 4; k++)
                                {
                                    blocksToMove[k] = allBlocks[i + k, j];
                                }
                                yield return StartCoroutine(MoveBlocksToBomb(targetPosition, blocksToMove));
                            }
                            else
                            {
                                /*if (allBlocks[i + num/2, j].transform.childCount != 0)
                                {
                                    foreach (Transform child in allBlocks[i + num/2, j].transform)
                                    {
                                        Destroy(child.gameObject);
                                    }
                                }*/
                                allBlocks[i + num/2, j].GetComponent<Block>().MakeColBomb();
                                //allBlocks[i + num/2, j].GetComponent<Block>().isMatched = false;
                                Vector2 targetPosition = allBlocks[i + num / 2, j].transform.position;
                                GameObject[] blocksToMove = new GameObject[4];
                                for (int k = 0; k < 4; k++)
                                {
                                    blocksToMove[k] = allBlocks[i + k, j];
                                }
                                yield return StartCoroutine(MoveBlocksToBomb(targetPosition, blocksToMove));
                            }
                        }
                    }
                    if (CheckVerticalMatch(i, j, num))
                    {
                        if (num == 5)
                        {
                            /*if (allBlocks[i, j + num/2].transform.childCount != 0)
                            {
                                foreach (Transform child in allBlocks[i, j + num/2].transform)
                                {
                                    Destroy(child.gameObject);
                                }
                            }*/
                            allBlocks[i, j + num/2].GetComponent<Block>().BecomeStar();
                            Vector2 targetPosition = allBlocks[i, j + num / 2].transform.position;
                            GameObject[] blocksToMove = new GameObject[5];
                            for (int k = 0; k < 5; k++)
                            {
                                blocksToMove[k] = allBlocks[i, j + k];
                            }
                            yield return StartCoroutine(MoveBlocksToBomb(targetPosition, blocksToMove));
                        }
                        else if (num == 4)
                        {
                            if (currentBlock != null && (allBlocks[i, j + num/2 - 1] == currentBlock.gameObject || (currentBlock.otherBlock != null && allBlocks[i, j + num/2 - 1] == currentBlock.otherBlock.gameObject)))
                            {
                                /*if (allBlocks[i, j + num/2 - 1].transform.childCount != 0)
                                {
                                    foreach (Transform child in allBlocks[i, j + num/2 - 1].transform)
                                    {
                                        Destroy(child.gameObject);
                                    }
                                }*/
                                allBlocks[i, j + num/2 - 1].GetComponent<Block>().MakeRowBomb();
                                //allBlocks[i, j + num/2 - 1].GetComponent<Block>().isMatched = false;
                                Vector2 targetPosition = allBlocks[i, j + num / 2 - 1].transform.position;
                                GameObject[] blocksToMove = new GameObject[4];
                                for (int k = 0; k < 4; k++)
                                {
                                    blocksToMove[k] = allBlocks[i, j + k];
                                }
                                yield return StartCoroutine(MoveBlocksToBomb(targetPosition, blocksToMove));
                            }
                            else
                            {
                                /*if (allBlocks[i, j + num/2].transform.childCount != 0)
                                {
                                    foreach (Transform child in allBlocks[i, j + num/2].transform)
                                    {
                                        Destroy(child.gameObject);
                                    }
                                }*/
                                allBlocks[i, j + num/2].GetComponent<Block>().MakeRowBomb();
                                //allBlocks[i, j + num/2].GetComponent<Block>().isMatched = false;
                                Vector2 targetPosition = allBlocks[i, j + num / 2].transform.position;
                                GameObject[] blocksToMove = new GameObject[4];
                                for (int k = 0; k < 4; k++)
                                {
                                    blocksToMove[k] = allBlocks[i, j + k];
                                }
                                yield return StartCoroutine(MoveBlocksToBomb(targetPosition, blocksToMove));
                            }

                            
                        }
                    }
                }
            }
        }
        //yield break;
    }
    
    public IEnumerator MoveBlocksToBomb(Vector2 bombPosition, GameObject[] blocks)
    {
        List<Coroutine> coroutines = new List<Coroutine>();
        foreach (var block in blocks)
        {
            if (block != null)
            {
                // Start a coroutine for each block and add it to the list
                coroutines.Add(StartCoroutine(LerpPosition(block.transform, bombPosition)));
            }
        }
        yield return new WaitForSeconds(.1f);

        // Wait until all coroutines have finished
        foreach (var coroutine in coroutines)
        {
            yield return coroutine;
        }
    }

    private IEnumerator LerpPosition(Transform block, Vector2 targetPosition)
    {
        float time = 0;
        Vector2 startPosition = block.position;

        while (time < 1 && block != null)
        {
            time += Time.deltaTime * lerpSpeed;
            block.position = Vector2.Lerp(startPosition, targetPosition, time);
            yield return null;
        }

        // Ensure the block is exactly at the target position at the end
        if (block != null)
            block.position = targetPosition;
    }


    private IEnumerator CheckLShapeMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!isMatched[i, j])
                {
                    if (i - 2 >= 0 && j - 2 >= 0)
                    {
                        if (CheckLShapeMatch(i, j, "topRight"))
                        {
                            /*if (allBlocks[i, j].transform.childCount != 0)
                            {
                                foreach (Transform child in allBlocks[i, j].transform)
                                {
                                    Destroy(child.gameObject);
                                }
                            }*/
                            allBlocks[i, j].GetComponent<Block>().MakeSquareBomb();
                            //allBlocks[i, j].GetComponent<Block>().isMatched = false;
                            isMatched[i, j] = true;
                            isMatched[i - 1, j] = true;
                            isMatched[i - 2, j] = true;
                            isMatched[i, j - 1] = true; 
                            isMatched[i, j - 2] = true;
                            GameObject[] blocksToMove = new GameObject[5];
                            blocksToMove[0] = allBlocks[i - 2, j];
                            blocksToMove[1] = allBlocks[i - 1, j];
                            blocksToMove[2] = allBlocks[i, j];
                            blocksToMove[3] = allBlocks[i, j - 1];
                            blocksToMove[4] = allBlocks[i, j - 2];
                            yield return StartCoroutine(MoveBlocksToBomb(allBlocks[i, j].transform.position, blocksToMove));
                        }
                    }
                    if (i - 2 >= 0 && j + 2 < height)
                    {
                        if (CheckLShapeMatch(i, j, "botRight"))
                        {
                            /*if (allBlocks[i, j].transform.childCount != 0)
                            {
                                foreach (Transform child in allBlocks[i, j].transform)
                                {
                                    Destroy(child.gameObject);
                                }
                            }*/
                            allBlocks[i, j].GetComponent<Block>().MakeSquareBomb();
                            //allBlocks[i, j].GetComponent<Block>().isMatched = false;
                            isMatched[i, j] = true;
                            isMatched[i - 1, j] = true;
                            isMatched[i - 2, j] = true;
                            isMatched[i, j + 1] = true;
                            isMatched[i, j + 2] = true;
                            GameObject[] blocksToMove = new GameObject[5];
                            blocksToMove[0] = allBlocks[i - 2, j];
                            blocksToMove[1] = allBlocks[i - 1, j];
                            blocksToMove[2] = allBlocks[i, j];
                            blocksToMove[3] = allBlocks[i, j + 1];
                            blocksToMove[4] = allBlocks[i, j + 2];
                            yield return StartCoroutine(MoveBlocksToBomb(allBlocks[i, j].transform.position, blocksToMove));
                        }
                    }
                    if (i + 2 < width && j - 2 >= 0)
                    {
                        if (CheckLShapeMatch(i, j, "topLeft"))
                        {
                            /*if (allBlocks[i, j].transform.childCount != 0)
                            {
                                foreach (Transform child in allBlocks[i, j].transform)
                                {
                                    Destroy(child.gameObject);
                                }
                            }*/
                            allBlocks[i, j].GetComponent<Block>().MakeSquareBomb();
                            //allBlocks[i, j].GetComponent<Block>().isMatched = false;
                            isMatched[i, j] = true;
                            isMatched[i + 1, j] = true;
                            isMatched[i + 2, j] = true;
                            isMatched[i, j - 1] = true;
                            isMatched[i, j - 2] = true;
                            GameObject[] blocksToMove = new GameObject[5];
                            blocksToMove[0] = allBlocks[i + 2, j];
                            blocksToMove[1] = allBlocks[i + 1, j];
                            blocksToMove[2] = allBlocks[i, j];
                            blocksToMove[3] = allBlocks[i, j - 1];
                            blocksToMove[4] = allBlocks[i, j - 2];
                            yield return StartCoroutine(MoveBlocksToBomb(allBlocks[i, j].transform.position, blocksToMove));
                        }
                    }
                    if (i + 2 < width && j + 2 < height)
                    {
                        if (CheckLShapeMatch(i, j, "botLeft"))
                        {
                            /*if (allBlocks[i, j].transform.childCount != 0)
                            {
                                foreach (Transform child in allBlocks[i, j].transform)
                                {
                                    Destroy(child.gameObject);
                                }
                            }*/
                            allBlocks[i, j].GetComponent<Block>().MakeSquareBomb();
                            //allBlocks[i, j].GetComponent<Block>().isMatched = false;
                            isMatched[i, j] = true;
                            isMatched[i + 1, j] = true;
                            isMatched[i + 2, j] = true;
                            isMatched[i, j + 1] = true;
                            isMatched[i, j + 2] = true;
                            GameObject[] blocksToMove = new GameObject[5];
                            blocksToMove[0] = allBlocks[i + 2, j];
                            blocksToMove[1] = allBlocks[i + 1, j];
                            blocksToMove[2] = allBlocks[i, j];
                            blocksToMove[3] = allBlocks[i, j + 1];
                            blocksToMove[4] = allBlocks[i, j + 2];
                            yield return StartCoroutine(MoveBlocksToBomb(allBlocks[i, j].transform.position, blocksToMove));
                        }
                    }
                }
            }
        }
        //yield break;
    }

    private bool CheckHorizontalMatch(int col, int row, int length)
    {
        if (col <= width - length)
        {
            if (allBlocks[col, row] == null)
            {
                return false;
            }
            string color = allBlocks[col, row].tag;
            if (color == "star")
            {
                return false;
            }
            for (int i = 1; i < length; i++)
            {
                if (allBlocks[col + i, row] == null || allBlocks[col + i, row].tag != color || isMatched[col + i, row])
                {
                    return false;
                } 
            }
            for (int i = 0; i < length; i++)
            {
                isMatched[col + i, row] = true;
            }
            return true;
        }
        return false;
    }

    private bool CheckVerticalMatch(int col, int row, int length)
    {
        if (row <= height - length)
        {
            if (allBlocks[col, row] == null)
            {
                return false;
            }
            string color = allBlocks[col, row].tag;
            if (color == "star")
            {
                return false;
            }
            for (int i = 1; i < length; i++)
            {
                if (allBlocks[col, row + i] == null || allBlocks[col, row + i].tag != color || isMatched[col, row + i])
                {
                    return false;
                } 
            }
            for (int i = 0; i < length; i++)
            {
                isMatched[col, row + i] = true;
            }
            return true;
        }
        return false;
    }

    private bool CheckLShapeMatch(int col, int row, string direction)
    {
        if (allBlocks[col, row] == null)
        {
            return false;
        }
        string color = allBlocks[col, row].tag;
        if (color == "star")
        {
            return false;
        }
        switch (direction)
        {
            case "botLeft":
                return allBlocks[col + 1, row] != null && allBlocks[col + 2, row] != null && allBlocks[col, row + 1] != null && allBlocks[col, row + 2] != null &&
                allBlocks[col + 1, row].tag == color && allBlocks[col + 2, row].tag == color && allBlocks[col, row + 1].tag == color && allBlocks[col, row + 2].tag == color 
                && !isMatched[col + 1, row] && !isMatched[col + 2, row] && !isMatched[col, row + 1] && !isMatched[col, row + 2];
            case "botRight":
                return allBlocks[col - 1, row] != null && allBlocks[col - 2, row] != null && allBlocks[col, row + 1] != null && allBlocks[col, row + 2] != null &&
                allBlocks[col - 1, row].tag == color && allBlocks[col - 2, row].tag == color && allBlocks[col, row + 1].tag == color && allBlocks[col, row + 2].tag == color
                && !isMatched[col - 1, row] && !isMatched[col - 2, row] && !isMatched[col, row + 1] && !isMatched[col, row + 2];
            case "topLeft":
                return allBlocks[col + 1, row] != null && allBlocks[col + 2, row] != null && allBlocks[col, row - 1] != null && allBlocks[col, row - 2] != null &&
                allBlocks[col + 1, row].tag == color && allBlocks[col + 2, row].tag == color && allBlocks[col, row - 1].tag == color && allBlocks[col, row - 2].tag == color
                && !isMatched[col + 1, row] && !isMatched[col + 2, row] && !isMatched[col, row - 1] && !isMatched[col, row - 2];
            case "topRight":
                return allBlocks[col - 1, row] != null && allBlocks[col - 2, row] != null && allBlocks[col, row - 1] != null && allBlocks[col, row - 2] != null &&
                allBlocks[col - 1, row].tag == color && allBlocks[col - 2, row].tag == color && allBlocks[col, row - 1].tag == color && allBlocks[col, row - 2].tag == color
                && !isMatched[col - 1, row] && !isMatched[col - 2, row] && !isMatched[col, row - 1] && !isMatched[col, row - 2];
            default:
                return false;
        }
        
    }

    private IEnumerator DecreaseNonBlankRow()
    {
        activeCoroutines++;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j] && allBlocks[i, j] == null)
                {
                    for (int k = j + 1; k < height; k++)
                    {
                        if (allBlocks[i, k] != null)
                        {
                            allBlocks[i, k].GetComponent<Block>().row = j;
                            allBlocks[i, k] = null;
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(.1f);
        activeCoroutines--;
    }

    private void RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allBlocks[i, j] == null && !blankSpaces[i, j])
                {
                    Vector2 tempPos = new Vector2(i, j + offset);
                    int blockToUse = Random.Range(0, blocks.Length);
                    GameObject piece;
                    if (blockPool.Count > 0)
                    {
                        piece = blockPool[0];
                        piece.transform.position = tempPos;
                        piece.tag = blocks[blockToUse].tag;
                        piece.GetComponent<SpriteRenderer>().sprite = blocks[blockToUse].GetComponent<SpriteRenderer>().sprite;
                        piece.GetComponent<Block>().enabled = true;
                        piece.GetComponent<Block>().isMatched = false;
                        blockPool.RemoveAt(0);
                    }
                    else
                    {
                        piece = Instantiate(blocks[blockToUse], tempPos, Quaternion.identity, this.transform);
                    }
                    
                    allBlocks[i, j] = piece;
                    piece.GetComponent<Block>().col = i;
                    piece.GetComponent<Block>().row = j;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allBlocks[i, j] != null)
                {
                    if (allBlocks[i, j].GetComponent<Block>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public IEnumerator FillBoard(bool isBonus)
    {
        activeCoroutines++;
        RefillBoard();
        yield return new WaitForSeconds(.1f);
        yield return StartCoroutine(findMatches.FindAllMatches());
        if (MatchesOnBoard())
        {
            streakValue++;
            yield return StartCoroutine(DestroyMatches(isBonus));
        }
        if (!isBonus)
        {
            if (IsDeadlocked())
            {
                ShuffleBoard();
            }
        }
        streakValue = 1;
        activeCoroutines--;
    }

    public void WaitOrMove()
    {
        if (activeCoroutines != 0)
        {
            currentState = GameState.wait;
        }
        else
        {
            currentState = GameState.move;
        }
    }

    public void BreakTile(int col, int row, int damage)
    {
        breakableTiles[col, row].TakeDamage(damage);
        if (breakableTiles[col, row].CheckHealth())
        {
            goalManager.CompareGoal(breakableTiles[col, row].tag);
            goalManager.UpdateGoals();
            //StartCoroutine(goalManager.UpdateGoals());
            breakableTiles[col, row].gameObject.SetActive(false);
            breakableTiles[col, row] = null;
            blankSpaces[col, row] = false;
            //Vector2 tempPos = new Vector2(col, row);
            //GameObject backgroundTile = Instantiate(tilePrefab, tempPos, Quaternion.identity, this.transform);
            //backgroundTile.name = "( " + col + ", "  + row +" )";
            if (lockedBlock.Count > 0)
            {
                blockPool.Add(lockedBlock[0]);
                lockedBlock.RemoveAt(0);
            }
        }
    }

    private IEnumerator DelayCheck(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }

    private void SwitchPieces(int col, int row, Vector2 direction)
    {
        GameObject holder = allBlocks[col + (int)direction.x, row + (int)direction.y] as GameObject;
        allBlocks[col + (int)direction.x, row + (int)direction.y] = allBlocks[col, row];
        allBlocks[col, row] = holder;
    }

    private bool CheckForMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allBlocks[i, j] != null)
                {
                    string color = allBlocks[i, j].tag;
                    if (i < width - 2)
                    {
                        if (allBlocks[i + 1, j] != null && allBlocks[i + 2, j] != null)
                        {
                            
                            if (allBlocks[i + 1, j].tag == color && allBlocks[i + 2, j].tag == color)
                            {
                                return true;
                            }
                        }
                    }
                    if (j < height - 2)
                    {
                        if (allBlocks[i, j + 1] != null && allBlocks[i, j + 2] != null)
                        {
                            if (allBlocks[i, j + 1].tag == color && allBlocks[i, j + 2].tag == color)
                            {
                                return true;
                            }
                        }
                    }
                        
                }
            }
        }
        return false;
    }

    private bool SwitchAndCheck(int col, int row, Vector2 direction)
    {
        SwitchPieces(col, row, direction);
        bool matchFound = CheckForMatches();
        SwitchPieces(col, row, direction);
        return matchFound;
    }

    private bool IsDeadlocked()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allBlocks[i, j] != null)
                {
                    if (allBlocks[i, j].GetComponent<Block>().isColorBomb)
                    {
                        return false;
                    }
                    if (i < width - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if (j < height - 1)
                    {
                        if (SwitchAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private void ShuffleBoard()
    {
        List<GameObject> newBoard = new List<GameObject>();
        for (int i = 0; i < width; i++)
        {
            for (int j = height; j < height; j++)
            {
                if (allBlocks[i, j] != null)
                {
                    newBoard.Add(allBlocks[i, j]);
                }
            }
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = height; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {
                    int blockToUse = Random.Range(0, newBoard.Count);
                    int maxIterations = 0;
                    while(MatchesAt(i, j, newBoard[blockToUse]) && maxIterations < 100)
                    {
                        maxIterations++;
                        blockToUse = Random.Range(0, newBoard.Count);
                    }
                    Block block = newBoard[blockToUse].GetComponent<Block>();
                    block.col = i;
                    block.row = j;
                    allBlocks[i, j] = newBoard[blockToUse];
                    newBoard.RemoveAt(blockToUse);
                }
            }
        }
        int iterations = 0;
        while (IsDeadlocked() && iterations <= 100)
        {   
            iterations++;
            ShuffleBoardInternal();
            
        }
        Debug.Log(iterations);
        
    }

     private void ShuffleBoardInternal()
    {
        List<GameObject> newBoard = new List<GameObject>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allBlocks[i, j] != null)
                {
                    newBoard.Add(allBlocks[i, j]);
                }
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {
                    int blockToUse = Random.Range(0, newBoard.Count);
                    int maxIterations = 0;
                    while (MatchesAt(i, j, newBoard[blockToUse]) && maxIterations < 10)
                    {
                        maxIterations++;
                        blockToUse = Random.Range(0, newBoard.Count);
                    }
                    Block block = newBoard[blockToUse].GetComponent<Block>();
                    block.col = i;
                    block.row = j;
                    allBlocks[i, j] = newBoard[blockToUse];
                    newBoard.RemoveAt(blockToUse);
                }
            }
        }
    }

}
