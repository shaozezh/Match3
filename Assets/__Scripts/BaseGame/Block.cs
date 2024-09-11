using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Board Variables")]
    public int col;
    public int row;
    public int prevCol;
    public int prevRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;
    private FindMatches findMatches;
    private Board board;
    public Block otherBlock;
    private Vector2 firstTouchPos;
    private Vector2 finalTouchPos;
    private Vector2 tempPos;
    public float swipeAngle = 0f;
    public float duration = .4f;
    private float swipeThreshold;
    
    [Header("Powerup")]
    public bool isColorBomb;
    public bool isColBomb;
    public bool isRowBomb;
    public bool isSquareBomb;
    public GameObject colArrow;
    public GameObject rowArrow;
    public GameObject colorBomb;
    public GameObject squareBomb;
    public Sprite starImage;
    private EndGameManager endGameManager;
    public string hiddenTag;
    //private GameObject arrow;
    //public bool isPause = false;

    void OnEnable()
    {
        isColBomb = false;
        isRowBomb = false;
        isSquareBomb = false;
        isColorBomb = false;
        isMatched = false;
        /*if (transform.childCount != 0)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }*/
    }

    void Start()
    {
        isColBomb = false;
        isRowBomb = false;
        isSquareBomb = false;
        isColorBomb = false;
        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        endGameManager = FindObjectOfType<EndGameManager>();
        //Vector2 blockSize = board.allBlocks[1, 0].transform.position - board.allBlocks[0, 0].transform.position;
        swipeThreshold = board.blockSize;
    }

    //Debug only
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //isColorBomb = true;
            BecomeStar();
            //GameObject colorB = Instantiate(colorBomb, transform.position, Quaternion.identity, this.transform);
            //colorB.transform.parent.gameObject.tag = "star";
        }
        /*if (Input.GetMouseButtonDown(1))
        {
            isColBomb = true;
            GameObject colorB = Instantiate(colArrow, transform.position, Quaternion.identity, this.transform);
        }*/
    }

    void Update()
    {
        if (board.isPause)
        {
            return;
        }
        targetX = col;
        targetY = row;
        if (Mathf.Abs(targetX - transform.position.x) > .1f)
        {
            tempPos = new Vector2 (targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPos, duration);
            if (board.allBlocks[col, row] != this.gameObject)
            {
                board.allBlocks[col, row] = this.gameObject;
            }
            //findMatches.FindAllMatch();
        }
        else
        {
            tempPos = new Vector2 (targetX, transform.position.y);
            transform.position = tempPos;
            //board.allBlocks[col, row] = this.gameObject;
        }

        if (Mathf.Abs(targetY - transform.position.y) > .1f)
        {
            tempPos = new Vector2 (transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPos, duration);
            if (board.allBlocks[col, row] != this.gameObject)
            {
                board.allBlocks[col, row] = this.gameObject;
            }
            //findMatches.FindAllMatch();
        }
        else
        {
            tempPos = new Vector2 (transform.position.x, targetY);
            transform.position = tempPos;
            //board.allBlocks[col, row] = this.gameObject;
        }
        
    }

    public IEnumerator CheckMove()
    {
        board.currentState = GameState.wait;
        yield return new WaitForSeconds(.1f);
        //findMatches.FindAllMatch();
        if (otherBlock != null)
        {
            if (isColorBomb && !otherBlock.isColorBomb)
            {
                if (otherBlock.tag == "col")
                {
                    //string bombType = otherBlock.transform.GetChild(0).gameObject.tag;
                    isColorBomb = false;
                    yield return StartCoroutine(findMatches.MatchColorCol(otherBlock.hiddenTag, this.transform));
                    /*if (bombType == "col")
                    {
                        findMatches.MatchColorCol(otherBlock.gameObject.tag);
                    }
                    else if (bombType == "row")
                    {
                        findMatches.MatchColorRow(otherBlock.gameObject.tag);
                    }
                    else if (bombType == "square")
                    {
                        findMatches.MatchColorSquare(otherBlock.gameObject.tag);
                    }
                    else
                    {

                        Debug.Log(bombType);
                        
                    }*/
                    isMatched = true;
                }
                else if (otherBlock.tag == "row")
                {
                    isColorBomb = false;
                    yield return StartCoroutine(findMatches.MatchColorRow(otherBlock.hiddenTag, this.transform));
                    isMatched = true;
                }
                else if (otherBlock.tag == "square")
                {
                    isColorBomb = false;
                    yield return StartCoroutine(findMatches.MatchColorSquare(otherBlock.hiddenTag, this.transform));
                    isMatched = true;
                }
                else
                {
                    isColorBomb = false;
                    yield return StartCoroutine(findMatches.MatchColorPieces(otherBlock.gameObject.tag, this.transform));
                    isMatched = true;
                    if (col > 0 && board.breakableTiles[col - 1, row] != null)
                    {
                        board.BreakTile(col - 1, row, 1);
                    }
                    if (col < board.width - 1 && board.breakableTiles[col + 1, row] != null)
                    {
                        board.BreakTile(col + 1, row, 1);
                    }
                    if (row > 0 && board.breakableTiles[col, row - 1] != null)
                    {
                        board.BreakTile(col, row - 1, 1);
                    }
                    if (row < board.height - 1 && board.breakableTiles[col, row + 1] != null)
                    {
                        board.BreakTile(col, row + 1, 1);
                    }
                }
                //yield return new WaitForSeconds(.15f);
                
            }
            else if (otherBlock.isColorBomb && !isColorBomb)
            {
                if (tag == "col")
                {
                    //string bombType = transform.GetChild(0).gameObject.tag;
                    otherBlock.isColorBomb = false;
                    yield return StartCoroutine(findMatches.MatchColorCol(hiddenTag, otherBlock.transform));
                    /*if (bombType == "col")
                    {
                        findMatches.MatchColorCol(this.gameObject.tag);
                    }
                    else if (bombType == "row")
                    {
                        findMatches.MatchColorRow(this.gameObject.tag);
                    }
                    else if (bombType == "square")
                    {
                        findMatches.MatchColorSquare(this.gameObject.tag);
                    }
                    else
                    {
                        Debug.Log(bombType);
                    }*/
                    otherBlock.isMatched = true;
                }
                else if (tag == "row")
                {
                    otherBlock.isColorBomb = false;
                    yield return StartCoroutine(findMatches.MatchColorRow(hiddenTag, otherBlock.transform));
                    otherBlock.isMatched = true;
                }
                else if (tag == "square")
                {
                    otherBlock.isColorBomb = false;
                    yield return StartCoroutine(findMatches.MatchColorSquare(hiddenTag, otherBlock.transform));
                    otherBlock.isMatched = true;
                }
                else
                {
                    otherBlock.isColorBomb = false;
                    yield return StartCoroutine(findMatches.MatchColorPieces(this.gameObject.tag, otherBlock.transform));
                    otherBlock.isMatched = true;
                    if (otherBlock.col > 0 && board.breakableTiles[otherBlock.col - 1, otherBlock.row] != null)
                    {
                        board.BreakTile(otherBlock.col - 1, otherBlock.row, 1);
                    }
                    if (otherBlock.col < board.width - 1 && board.breakableTiles[otherBlock.col + 1, otherBlock.row] != null)
                    {
                        board.BreakTile(otherBlock.col + 1, otherBlock.row, 1);
                    }
                    if (otherBlock.row > 0 && board.breakableTiles[otherBlock.col, otherBlock.row - 1] != null)
                    {
                        board.BreakTile(otherBlock.col, otherBlock.row - 1, 1);
                    }
                    if (otherBlock.row < board.height - 1 && board.breakableTiles[otherBlock.col, otherBlock.row + 1] != null)
                    {
                        board.BreakTile(otherBlock.col, otherBlock.row + 1, 1);
                    }
                }
                //yield return new WaitForSeconds(.15f);
            }
            else if (isColorBomb && otherBlock.isColorBomb)
            {
                Debug.Log("clear the board");
                isColorBomb = false;
                otherBlock.isColorBomb = false;
                for (int i = 0; i < board.width; i++)
                {
                    for (int j = 0; j < board.height; j++)
                    {
                        if (board.allBlocks[i, j] != null)
                        {
                            if (board.allBlocks[i, j].GetComponent<Block>().isColorBomb)
                            {
                                continue;
                            }
                            board.allBlocks[i, j].GetComponent<Block>().isMatched = true;
                        }
                    }
                }              
                //yield return new WaitForSeconds(.15f);
                for (int i = 0; i < board.width; i++)
                {
                    for (int j = 0; j < board.height; j++)
                    {
                        if (board.breakableTiles[i, j] != null)
                        {
                            board.BreakTile(i, j, 3);
                        }
                    }
                }
                //yield return new WaitForSeconds(.15f);
            }
            else if (isColBomb || otherBlock.isColBomb || isRowBomb || otherBlock.isRowBomb || isSquareBomb || otherBlock.isSquareBomb)
            {
                if (isColBomb)
                {
                    findMatches.GetColPieces(col, row);
                }
                if (otherBlock.isColBomb)
                {
                    findMatches.GetColPieces(otherBlock.col, otherBlock.row);
                }
                if (isRowBomb)
                {
                    findMatches.GetRowPieces(col, row);
                }
                if (otherBlock.isRowBomb)
                {
                    findMatches.GetRowPieces(otherBlock.col, otherBlock.row);
                }
                if (isSquareBomb)
                {
                    findMatches.GetSquarePieces(col, row);
                }
                if (otherBlock.isSquareBomb)
                {
                    findMatches.GetSquarePieces(otherBlock.col, otherBlock.row);
                }
                //yield return new WaitForSeconds(.15f);
            }
            yield return StartCoroutine(findMatches.FindAllMatches());
            if (!isMatched && !otherBlock.isMatched)
            {
                otherBlock.col = col;
                otherBlock.row = row;
                col = prevCol;
                row = prevRow;
                yield return new WaitForSeconds(.2f);
                board.currentBlock = null;
                board.currentState = GameState.move;
                //Debug.Log("wrong move");
            }
            else
            {
                if (endGameManager != null)
                {
                    if (endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.DecreaseCounterValue();
                    }
                }
                yield return StartCoroutine(board.DestroyMatches(false));
                yield return StartCoroutine(board.goalManager.CheckWin());
                if (endGameManager != null)
                {
                    if (endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.CheckLoseCondition();
                    }
                }

            }
            otherBlock = null;
            
        }
    }

    private void OnMouseDown()
    {
        board.WaitOrMove();
        if (board.currentState == GameState.move)
        {
            firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(firstTouchPos, finalTouchPos) > swipeThreshold)
            {
                board.currentState = GameState.wait;
                board.currentBlock = this;
                CalculateAngle();
                
            }
        }
        
    }

    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPos.y - firstTouchPos.y, finalTouchPos.x - firstTouchPos.x) *  Mathf.Rad2Deg;
        if (swipeAngle > -45f && swipeAngle <= 45f && col < board.width - 1 && board.allBlocks[col + 1, row] != null)
        {
            //swipe right
            MoveBlocks(1, 0);
            StartCoroutine(CheckMove());
        }
        else if (swipeAngle > 45f && swipeAngle <= 135f && row < board.height - 1 && board.allBlocks[col, row + 1] != null)
        {
            //swipe up
            MoveBlocks(0, 1);
            StartCoroutine(CheckMove());
        }
        else if ((swipeAngle > 135f || swipeAngle <= -135f) && col > 0 && board.allBlocks[col - 1, row] != null)
        {
            // swipe left
            MoveBlocks(-1, 0);
            StartCoroutine(CheckMove());
        }
        else if (swipeAngle < -45f && swipeAngle >= -135f && row > 0 && board.allBlocks[col, row - 1] != null)
        {
            // swipe down
            MoveBlocks(0, -1);
            StartCoroutine(CheckMove());
        }
        
    }

    private void MoveBlocks(int colMove, int rowMove)
    {
        prevCol = col;
        prevRow = row;
        
        otherBlock = board.allBlocks[col + colMove, row + rowMove].GetComponent<Block>();
        otherBlock.col -= colMove;
        otherBlock.row -= rowMove;
        col += colMove;
        row += rowMove;
        
        
    }

    public void MakeRowBomb()
    {
        isRowBomb = true;
        GetComponent<SpriteRenderer>().sprite = board.rowArrow;
        hiddenTag = this.gameObject.tag;
        this.gameObject.tag = "row";
        isMatched = false;
        //arrow = Instantiate(rowArrow, transform.position, Quaternion.identity, this.transform);
    }

    public void MakeColBomb()
    {
        isColBomb = true;
        GetComponent<SpriteRenderer>().sprite = board.colArrow;
        hiddenTag = this.gameObject.tag;
        this.gameObject.tag = "col";
        isMatched = false;
        //arrow = Instantiate(colArrow, transform.position, Quaternion.identity, this.transform);
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        //GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity, this.transform);
    }

    public void MakeSquareBomb()
    {
        isSquareBomb = true;
        GetComponent<SpriteRenderer>().sprite = board.squareBomb;
        hiddenTag = this.gameObject.tag;
        this.gameObject.tag = "square";
        isMatched = false;
        //arrow = Instantiate(squareBomb, transform.position, Quaternion.identity, this.transform);
    }

    public void BecomeStar()
    {
        GetComponent<SpriteRenderer>().sprite = starImage;
        MakeColorBomb();
        hiddenTag = this.gameObject.tag;
        this.gameObject.tag = "star";
        isMatched = false;
        //Destroy(arrow);
    }
}
