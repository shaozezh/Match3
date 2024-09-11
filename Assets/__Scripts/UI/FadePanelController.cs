using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePanelController : MonoBehaviour
{
    public Animator panelAnim;
    public Animator gameInfoAnim;
    public EndGameManager endGameManager;
    public Board board;
    public void OK()
    {
        if (panelAnim != null && gameInfoAnim != null)
        {
            panelAnim.SetBool("Out", true);
            gameInfoAnim.SetBool("Out", true);
            StartCoroutine(StartTimer());
        }
        
    }

    public void GameOver()
    {
        panelAnim.SetBool("Out", false);
        panelAnim.SetBool("GameOver", true);
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(.1f);
        board.currentState = GameState.move;
        //endGameManager.isReady = true;
    }
}
