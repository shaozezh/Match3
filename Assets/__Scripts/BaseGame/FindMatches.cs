using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    [SerializeField] private Board board;
    public GameObject leftRocket;
    public GameObject rightRocket;
    public GameObject upRocket;
    public GameObject downRocket;
    public GameObject squareExplosion;
    public CurvedLineRenderer curvedLineManager;

    //public List<GameObject> currentMatches = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void FindAllMatch()
    {
        //StartCoroutine(FindAllMatches());
        FindAllMatches();
    }

    public IEnumerator FindAllMatches()
    {
        //yield return new WaitForSeconds(.1f);
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject current = board.allBlocks[i, j];
                if (current != null)
                {
                    if (current.tag == "star" || current.tag == "square" || current.tag == "col" || current.tag == "row")
                    {
                        continue;
                    }
                    Block currentBlock = current.GetComponent<Block>();
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject left = board.allBlocks[i - 1, j];
                        GameObject right = board.allBlocks[i + 1, j];
                        if (left != null && right != null)
                        {
                            Block leftBlock = left.GetComponent<Block>();
                            Block rightBlock = right.GetComponent<Block>();
                            if (left.tag == current.tag && right.tag == current.tag)
                            {
                                if (currentBlock.col - 1 == leftBlock.col && currentBlock.col + 1 == rightBlock.col)
                                {
                                    if (!currentBlock.isMatched)
                                    {
                                        if (j - 1 >= 0)
                                        {
                                            if (board.breakableTiles[i, j - 1] != null)
                                            {
                                                board.BreakTile(i, j - 1, 1);
                                            }
                                        }
                                        if (j + 1 <= board.height - 1)
                                        {
                                            if (board.breakableTiles[i, j + 1] != null)
                                            {
                                                board.BreakTile(i, j + 1, 1);
                                            }
                                        }
                                    }
                                    if (!leftBlock.isMatched)
                                    {
                                        if (j - 1 >= 0)
                                        {
                                            if (board.breakableTiles[i - 1, j - 1] != null)
                                            {
                                                board.BreakTile(i - 1, j - 1, 1);
                                            }
                                        }
                                        if (j + 1 <= board.height - 1)
                                        {
                                            if (board.breakableTiles[i - 1, j + 1] != null)
                                            {
                                                board.BreakTile(i - 1, j + 1, 1);
                                            }
                                        }
                                        if (i - 2 >= 0)
                                        {
                                            if (board.breakableTiles[i - 2, j] != null)
                                            {
                                                board.BreakTile(i - 2, j, 1);
                                            }
                                        }
                                    }
                                    if (!rightBlock.isMatched)
                                    {
                                        if (j - 1 >= 0)
                                        {
                                            if (board.breakableTiles[i + 1, j - 1] != null)
                                            {
                                                board.BreakTile(i + 1, j - 1, 1);
                                            }
                                        }
                                        if (j + 1 <= board.height - 1)
                                        {
                                            if (board.breakableTiles[i + 1, j + 1] != null)
                                            {
                                                board.BreakTile(i + 1, j + 1, 1);
                                            }
                                        }
                                        if (i + 2 <= board.width - 1)
                                        {
                                            if (board.breakableTiles[i + 2, j] != null)
                                            {
                                                board.BreakTile(i + 2, j, 1);
                                            }
                                        }
                                    }
                                    
                                    if (currentBlock.isSquareBomb)
                                    {
                                        currentBlock.isSquareBomb = false;
                                        GetSquarePieces(i, j);
                                    }
                                    if (leftBlock.isSquareBomb)
                                    {
                                        leftBlock.isSquareBomb = false;
                                        GetSquarePieces(i - 1, j);
                                    }
                                    if (rightBlock.isSquareBomb)
                                    {
                                        rightBlock.isSquareBomb = false;
                                        GetSquarePieces(i + 1, j);
                                    }
                                    if (currentBlock.isRowBomb)
                                    {
                                        currentBlock.isRowBomb = false;
                                        GetRowPieces(i, j);
                                    }
                                    if (leftBlock.isRowBomb)
                                    {
                                        leftBlock.isRowBomb = false;
                                        GetRowPieces(i - 1, j);
                                    }
                                    if (rightBlock.isRowBomb) 
                                    {
                                        rightBlock.isRowBomb = false;
                                        GetRowPieces(i + 1, j);
                                    }
                                    if (currentBlock.isColBomb)
                                    {
                                        currentBlock.isColBomb = false;
                                        GetColPieces(i, j);
                                    }
                                    if (leftBlock.isColBomb)
                                    {
                                        leftBlock.isColBomb = false;
                                        GetColPieces(i - 1, j);
                                    }
                                    if (rightBlock.isColBomb)
                                    {
                                        rightBlock.isColBomb = false;
                                        GetColPieces(i + 1, j);
                                    }
                                    leftBlock.isMatched = true;
                                    currentBlock.isMatched = true;
                                    rightBlock.isMatched = true; 
                                }
                            }
                        }
                    }
                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject down = board.allBlocks[i, j - 1];
                        GameObject up = board.allBlocks[i, j + 1];
                        if (down != null && up != null)
                        {
                            Block downBlock = down.GetComponent<Block>();
                            Block upBlock = up.GetComponent<Block>();
                            if (down.tag == current.tag && up.tag == current.tag)
                            {
                                if (currentBlock.row - 1 == downBlock.row && currentBlock.row + 1 == upBlock.row)
                                {
                                    if (!currentBlock.isMatched)
                                    {
                                        if (i - 1 >= 0)
                                        {
                                            if (board.breakableTiles[i - 1, j] != null)
                                            {
                                                board.BreakTile(i - 1, j, 1);
                                            }
                                        }
                                        if (i + 1 <= board.width - 1)
                                        {
                                            if (board.breakableTiles[i + 1, j] != null)
                                            {
                                                board.BreakTile(i + 1, j, 1);
                                            }
                                        }
                                    }
                                    if (!downBlock.isMatched)
                                    {
                                        if (i - 1 >= 0)
                                        {
                                            if (board.breakableTiles[i - 1, j - 1] != null)
                                            {
                                                board.BreakTile(i - 1, j - 1, 1);
                                            }
                                        }
                                        if (i + 1 <= board.width - 1)
                                        {
                                            if (board.breakableTiles[i + 1, j - 1] != null)
                                            {
                                                board.BreakTile(i + 1, j - 1, 1);
                                            }
                                        }
                                        if (j - 2 >= 0)
                                        {
                                            if (board.breakableTiles[i, j - 2] != null)
                                            {
                                                board.BreakTile(i, j - 2, 1);
                                            }
                                        }
                                    }
                                    if (!upBlock.isMatched)
                                    {
                                        if (i - 1 >= 0)
                                        {
                                            if (board.breakableTiles[i - 1, j + 1] != null)
                                            {
                                                board.BreakTile(i - 1, j + 1, 1);
                                            }
                                        }
                                        if (i + 1 <= board.width - 1)
                                        {
                                            if (board.breakableTiles[i + 1, j + 1] != null)
                                            {
                                                board.BreakTile(i + 1, j + 1, 1);
                                            }
                                        }
                                        if (j + 2 <= board.height - 1)
                                        {
                                            if (board.breakableTiles[i, j + 2] != null)
                                            {
                                                board.BreakTile(i, j + 2, 1);
                                            }
                                        }
                                    }
                                    if (currentBlock.isSquareBomb)
                                    {
                                        currentBlock.isSquareBomb = false;
                                        GetSquarePieces(i, j);
                                    }
                                    if (downBlock.isSquareBomb)
                                    {
                                        downBlock.isSquareBomb = false;
                                        GetSquarePieces(i, j - 1);
                                    }
                                    if (upBlock.isSquareBomb)
                                    {
                                        upBlock.isSquareBomb = false;
                                        GetSquarePieces(i, j + 1);
                                    }
                                    if (currentBlock.isColBomb)
                                    {
                                        currentBlock.isColBomb = false;
                                        GetColPieces(i, j);
                                    }
                                    if (downBlock.isColBomb)
                                    {
                                        downBlock.isColBomb = false;
                                        GetColPieces(i, j - 1);
                                    }
                                    if (upBlock.isColBomb)
                                    {
                                        upBlock.isColBomb = false;
                                        GetColPieces(i, j + 1);
                                    }
                                    if (currentBlock.isRowBomb)
                                    {
                                        currentBlock.isRowBomb = false;
                                        GetRowPieces(i, j);
                                    }
                                    if (downBlock.isRowBomb)
                                    {
                                        downBlock.isRowBomb = false;
                                        GetRowPieces(i, j - 1);
                                    }
                                    if (upBlock.isRowBomb)
                                    {
                                        upBlock.isRowBomb = false;
                                        GetRowPieces(i, j + 1);
                                    }
                                    downBlock.isMatched = true;
                                    currentBlock.isMatched = true;
                                    upBlock.isMatched = true; 
                                }
                            }
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(.2f);
    }

    public IEnumerator MatchColorPieces(string color, Transform starPiece)
    {
        List<Coroutine> runningCoroutines = new List<Coroutine>();
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allBlocks[i, j] != null && board.allBlocks[i, j].tag == color)
                {
                    
                    Coroutine curveCoroutine =  StartCoroutine(curvedLineManager.CurvesMove(starPiece.position, board.allBlocks[i, j], false));
                    runningCoroutines.Add(curveCoroutine);
                }
            }
        }
        foreach (Coroutine curveCoroutine in runningCoroutines)
        {
            yield return curveCoroutine;
        }
        //yield return new WaitForSeconds(0.2f);
        curvedLineManager.DestroyAllStars();
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allBlocks[i, j] != null)
                {
                    if (board.allBlocks[i, j].tag == color)
                    {
                        Block block = board.allBlocks[i, j].GetComponent<Block>();
                        block.isMatched = true;
                        //StartCoroutine(curvedLineManager.CurvesMove(starPiece.position, board.allBlocks[i, j]));
                        if (block.isRowBomb)
                        {
                            block.isRowBomb = false;
                            GetRowPieces(i, j);
                        }
                        else if (block.isColBomb)
                        {
                            block.isColBomb = false;
                            GetColPieces(i, j);
                        }
                        else if (block.isSquareBomb)
                        {
                            block.isSquareBomb = false;
                            GetSquarePieces(i, j);  
                        }
                        if (i > 0 && board.breakableTiles[i - 1, j] != null)
                        {
                            board.BreakTile(i - 1, j, 1);
                        }
                        if (i < board.width - 1 && board.breakableTiles[i + 1, j] != null)
                        {
                            board.BreakTile(i + 1, j, 1);
                        }
                        if (j > 0 && board.breakableTiles[i, j - 1] != null)
                        {
                            board.BreakTile(i, j - 1, 1);
                        }
                        if (j < board.height - 1 && board.breakableTiles[i, j + 1] != null)
                        {
                            board.BreakTile(i, j + 1, 1);
                        }
                    }
                }
            }
        }
    }

    public void GetSquarePieces(int col, int row)
    {
        GameObject tempExplosion = Instantiate(squareExplosion, board.allBlocks[col, row].transform.position, Quaternion.identity);
        Destroy(tempExplosion, .5f);
        for (int i = col - 1; i <= col + 1; i++)
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                if (i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    if (board.allBlocks[i, j] != null)
                    {
                        Block block = board.allBlocks[i, j].GetComponent<Block>();
                        if (i != col || j != row)
                        {
                            if (block.isRowBomb)
                            {
                                block.isRowBomb = false;
                                GetRowPieces(i, j);
                            }
                            else if (block.isColBomb)
                            {
                                block.isColBomb = false;
                                GetColPieces(i, j);
                            }
                            else if (block.isSquareBomb)
                            {
                                block.isSquareBomb = false;
                                GetSquarePieces(i, j);  
                            }
                            else if (block.isColorBomb)
                            {
                                //Debug.Log("Color Match");
                                //block.isColorBomb = false;
                                //MatchColorPieces(board.allBlocks[col, row].tag);
                                continue;
                            }
                        }
                        if (i > 0 && board.breakableTiles[i - 1, j] != null)
                        {
                            board.BreakTile(i - 1, j, 1);
                        }
                        if (i < board.width - 1 && board.breakableTiles[i + 1, j] != null)
                        {
                            board.BreakTile(i + 1, j, 1);
                        }
                        if (j > 0 && board.breakableTiles[i, j - 1] != null)
                        {
                            board.BreakTile(i, j - 1, 1);
                        }
                        if (j < board.height - 1 && board.breakableTiles[i, j + 1] != null)
                        {
                            board.BreakTile(i, j + 1, 1);
                        }
                        block.isMatched = true;
                    }
                    else if (board.breakableTiles[i, j] != null)
                    {
                        board.BreakTile(i, j, 1);
                    }
                }
            }
        }

    }

    public void GetColPieces(int col, int row)
    {
        Vector3 offset = new Vector3 (0f, 0.5f, 0f);
        GameObject tempRocket = Instantiate(upRocket, board.allBlocks[col, row].transform.position + offset, Quaternion.identity);
        StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(board.allBlocks[col, row].transform.position.x, 2 * Camera.main.transform.position.y + 8.5f)));
        tempRocket = Instantiate(downRocket, board.allBlocks[col, row].transform.position - offset, Quaternion.identity);
        StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(board.allBlocks[col, row].transform.position.x, -8.5f)));
                        
        //List<GameObject> blocks = new List<GameObject>();
        int damage = 1;
        for (int i = 0; i < board.height; i++)
        {
            if (board.allBlocks[col, i] != null)
            {
                Block block = board.allBlocks[col, i].GetComponent<Block>();
                if (block.isRowBomb)
                {
                    block.isRowBomb = false;
                    GetRowPieces(col, i);
                }
                else if (block.isSquareBomb)
                {
                    block.isSquareBomb = false;
                    GetSquarePieces(col, i);
                }
                else if (block.isColorBomb)
                {
                    //block.isColorBomb = false;
                    //MatchColorPieces(board.allBlocks[col, row].tag);
                    continue;
                }
                else if (block.isColBomb)
                {
                    block.isColBomb = false;
                    damage++;
                }
                //blocks.Add(board.allBlocks[col, i]);
                block.isMatched = true;
            }
        }
        for (int i = 0; i < board.height; i++)
        {
            if (board.allBlocks[col, i] != null)
            {
                if (col > 0 && board.breakableTiles[col - 1, i] != null)
                {
                    board.BreakTile(col - 1, i, damage);
                }
                if (col < board.width - 1 && board.breakableTiles[col + 1, i] != null)
                {
                    board.BreakTile(col + 1, i, damage);
                }
            }
            else if (board.breakableTiles[col, i] != null)
            {
                board.BreakTile(col, i, damage);
            }
        }

        //return blocks;
    }

    public void GetRowPieces(int col, int row)
    {
        Vector3 offset = new Vector3 (0.5f, 0f, 0f);
        GameObject tempRocket = Instantiate(leftRocket, board.allBlocks[col, row].transform.position - offset, Quaternion.identity);
        StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(Camera.main.transform.position.x - Camera.main.transform.position.y - 5f, board.allBlocks[col, row].transform.position.y)));
        tempRocket = Instantiate(rightRocket, board.allBlocks[col, row].transform.position + offset, Quaternion.identity);
        StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(Camera.main.transform.position.x + Camera.main.transform.position.y + 5f, board.allBlocks[col, row].transform.position.y)));
        //List<GameObject> blocks = new List<GameObject>();
        int damage = 1;
        for (int i = 0; i < board.width; i++)
        {
            if (board.allBlocks[i, row] != null)
            {
                Block block = board.allBlocks[i, row].GetComponent<Block>();
                if (block.isColBomb)
                {
                    block.isColBomb = false;
                    GetColPieces(i, row);
                }
                else if (block.isSquareBomb)
                {
                    block.isSquareBomb = false;
                    GetSquarePieces(i, row);
                }
                else if (block.isColorBomb)
                {
                    //block.isColorBomb = false;
                    //MatchColorPieces(board.allBlocks[col, row].tag);
                    continue;
                }
                else if (block.isRowBomb)
                {
                    block.isRowBomb = false;
                    damage++;
                }
                //blocks.Add(board.allBlocks[i, row]);
                block.isMatched = true;
            }
        }
        for (int i = 0; i < board.width; i++)
        {
            if (board.allBlocks[i, row] != null)
            {
                if (row > 0 && board.breakableTiles[i, row - 1] != null)
                {
                    board.BreakTile(i, row - 1, damage);
                }
                if (row < board.height - 1 && board.breakableTiles[i, row + 1] != null)
                {
                    board.BreakTile(i, row + 1, damage);
                }
            }
            else if (board.breakableTiles[i, row] != null)
            {
                board.BreakTile(i, row, damage);
            }
        }
        //return blocks;
    }

    public void CheckBombs()
    {
        if (board.currentBlock != null)
        {
            if (board.currentBlock.isMatched)
            {
                board.currentBlock.isMatched = false;
                if ((board.currentBlock.swipeAngle > -45f && board.currentBlock.swipeAngle <= 45f)
                    || (board.currentBlock.swipeAngle < -135f || board.currentBlock.swipeAngle >= 135f))
                {
                    board.currentBlock.MakeRowBomb();
                }
                else
                {
                    board.currentBlock.MakeColBomb();
                }
                
            }
            else if (board.currentBlock.otherBlock != null)
            {
                if (board.currentBlock.otherBlock.isMatched)
                {
                    board.currentBlock.otherBlock.isMatched = false;
                    if ((board.currentBlock.swipeAngle > -45f && board.currentBlock.swipeAngle <= 45f)
                        || (board.currentBlock.swipeAngle < -135f || board.currentBlock.swipeAngle >= 135f))
                    {
                        board.currentBlock.otherBlock.MakeRowBomb();
                    }
                    else
                    {
                        board.currentBlock.otherBlock.MakeColBomb();
                    }
                }
            }
        }
    }

    public IEnumerator MatchColorCol(string color, Transform starPiece)
    {
        List<Coroutine> runningCoroutines = new List<Coroutine>();
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allBlocks[i, j] != null && board.allBlocks[i, j].tag == color)
                {
                    
                    Coroutine curveCoroutine =  StartCoroutine(curvedLineManager.CurvesMove(starPiece.position, board.allBlocks[i, j], false));
                    runningCoroutines.Add(curveCoroutine);
                }
            }
        }
        foreach (Coroutine curveCoroutine in runningCoroutines)
        {
            yield return curveCoroutine;
        }
        //yield return new WaitForSeconds(0.2f);
        curvedLineManager.DestroyAllStars();
        for (int i = 0; i < board.width; i++)
        {
            bool setColTrue = false;
            int damage = 0;
            for (int j = 0; j < board.height; j++)
            {
                if (board.allBlocks[i, j] != null && board.allBlocks[i, j].tag == color)
                {
                    Vector3 offset = new Vector3 (0f, 0.5f, 0f);
                    GameObject tempRocket = Instantiate(upRocket, board.allBlocks[i, j].transform.position + offset, Quaternion.identity);
                    StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(board.allBlocks[i, j].transform.position.x, 2 * Camera.main.transform.position.y + 8.5f)));
                    tempRocket = Instantiate(downRocket, board.allBlocks[i, j].transform.position - offset, Quaternion.identity);
                    StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(board.allBlocks[i, j].transform.position.x, -8.5f)));
                    damage++;
                    setColTrue = true;
                    //break;
                    Block tempBlock = board.allBlocks[i, j].GetComponent<Block>();
                    tempBlock.MakeColBomb();
                    tempBlock.isColBomb = false;
                }
            }
            if (setColTrue)
            {
                for (int j = 0; j < board.height; j++)
                {
                    if (board.allBlocks[i, j] != null)
                    {
                        if (board.allBlocks[i, j].tag == "star")
                        {
                            continue;
                        }
                        board.allBlocks[i, j].GetComponent<Block>().isMatched = true;
                        if (i > 0 && board.breakableTiles[i - 1, j] != null)
                        {
                            board.BreakTile(i - 1, j, damage);
                        }
                        if (i < board.width - 1 && board.breakableTiles[i + 1, j] != null)
                        {
                            board.BreakTile(i + 1, j, damage);
                        }
                    }
                    else if (board.breakableTiles[i, j] != null)
                    {
                        board.BreakTile(i, j, damage);
                    }
                }

            }
        }
    }

    public IEnumerator MatchColorRow(string color, Transform starPiece)
    {
        List<Coroutine> runningCoroutines = new List<Coroutine>();
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allBlocks[i, j] != null && board.allBlocks[i, j].tag == color)
                {
                    
                    Coroutine curveCoroutine =  StartCoroutine(curvedLineManager.CurvesMove(starPiece.position, board.allBlocks[i, j], false));
                    runningCoroutines.Add(curveCoroutine);
                }
            }
        }
        foreach (Coroutine curveCoroutine in runningCoroutines)
        {
            yield return curveCoroutine;
        }
        //yield return new WaitForSeconds(0.2f);
        curvedLineManager.DestroyAllStars();
        for (int j = 0; j < board.height; j++)
        {
            bool setRowTrue = false;
            int damage = 0;
            for (int i = 0; i < board.width; i++)
            {
                if (board.allBlocks[i, j] != null && board.allBlocks[i, j].tag == color)
                {
                    //StartCoroutine(curvedLineManager.CurvesMove(starPiece.position, board.allBlocks[i, j]));
                    Vector3 offset = new Vector3 (0.5f, 0f, 0f);
                    GameObject tempRocket = Instantiate(leftRocket, board.allBlocks[i, j].transform.position - offset, Quaternion.identity);
                    StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(Camera.main.transform.position.x - Camera.main.transform.position.y - 5f, board.allBlocks[i, j].transform.position.y)));
                    tempRocket = Instantiate(rightRocket, board.allBlocks[i, j].transform.position + offset, Quaternion.identity);
                    StartCoroutine(tempRocket.GetComponent<BombExplosion>().MoveToDestination(new Vector2(Camera.main.transform.position.x + Camera.main.transform.position.y + 5f, board.allBlocks[i, j].transform.position.y)));
                    damage++;
                    setRowTrue = true;
                    Block tempBlock = board.allBlocks[i, j].GetComponent<Block>();
                    tempBlock.MakeRowBomb();
                    tempBlock.isRowBomb = false;
                    //break;
                }
            }
            if (setRowTrue)
            {
                for (int i = 0; i < board.width; i++)
                {
                    if (board.allBlocks[i, j] != null)
                    {
                        if (board.allBlocks[i, j].tag == "star")
                        {
                            continue;
                        }
                        board.allBlocks[i, j].GetComponent<Block>().isMatched = true;
                        if (j > 0 && board.breakableTiles[i, j - 1] != null)
                        {
                            board.BreakTile(i, j - 1, damage);
                        }
                        if (j < board.height - 1 && board.breakableTiles[i, j + 1] != null)
                        {
                            board.BreakTile(i, j + 1, damage);
                        }
                    }
                    else if (board.breakableTiles[i, j] != null)
                    {
                        board.BreakTile(i, j, damage);
                    }
                }
            }
        }
    }

    public IEnumerator MatchColorSquare(string color, Transform starPiece)
    {
        List<Coroutine> runningCoroutines = new List<Coroutine>();
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allBlocks[i, j] != null && board.allBlocks[i, j].tag == color)
                {
                    Coroutine curveCoroutine =  StartCoroutine(curvedLineManager.CurvesMove(starPiece.position, board.allBlocks[i, j], false));
                    runningCoroutines.Add(curveCoroutine);
                }
            }
        }
        foreach (Coroutine curveCoroutine in runningCoroutines)
        {
            yield return curveCoroutine;
        }
        //yield return new WaitForSeconds(0.2f);
        curvedLineManager.DestroyAllStars();
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allBlocks[i, j] != null && board.allBlocks[i, j].tag == color)
                {
                    //StartCoroutine(curvedLineManager.CurvesMove(starPiece.position, board.allBlocks[i, j]));
                    GameObject tempExplosion = Instantiate(squareExplosion, board.allBlocks[i, j].transform.position, Quaternion.identity);
                    Destroy(tempExplosion, .5f);
                    Block tempBlock = board.allBlocks[i, j].GetComponent<Block>();
                    tempBlock.MakeSquareBomb();
                    tempBlock.isSquareBomb = false;
                    Set3x3AreaTrue(i, j);
                }
            }
        }
    }

    private void Set3x3AreaTrue(int col, int row)
    {
        for (int i = col - 1; i <= col + 1; i++)
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                if (i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    if (board.allBlocks[i, j] != null)
                    {
                        if (board.allBlocks[i, j].tag == "star")
                        {
                            continue;
                        }
                        board.allBlocks[i, j].GetComponent<Block>().isMatched = true;
                        if (i > 0 && board.breakableTiles[i - 1, j] != null)
                        {
                            board.BreakTile(i - 1, j, 1);
                        }
                        if (i < board.width - 1 && board.breakableTiles[i + 1, j] != null)
                        {
                            board.BreakTile(i + 1, j, 1);
                        }
                        if (j > 0 && board.breakableTiles[i, j - 1] != null)
                        {
                            board.BreakTile(i, j - 1, 1);
                        }
                        if (j < board.height - 1 && board.breakableTiles[i, j + 1] != null)
                        {
                            board.BreakTile(i, j + 1, 1);
                        }
                    }
                    else if (board.breakableTiles[i, j] != null)
                    {
                        board.BreakTile(i, j, 1);
                    }
                }
            }
        }
    }

}
