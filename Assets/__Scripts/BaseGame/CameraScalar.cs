using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScalar : MonoBehaviour
{
    public Board board;
    public float cameraOffset;
    public float aspectRatio = .5625f;
    public float padding = 2f;
    public float yOffset;
    // Start is called before the first frame update
    void Start()
    {
        if (board != null)
        {
            RepositionCamera(board.width - 1, board.height - 1);
        }
    }

    private void RepositionCamera(float x, float y)
    {
        Vector3 tempPos = new Vector3(x / 2, y / 2 + yOffset, cameraOffset);
        transform.position = tempPos;
        if (board.width >= board.height)
        {
            Camera.main.orthographicSize = (board.width / 2) / aspectRatio + padding / 2;
        }
        else
        {
            Camera.main.orthographicSize = board.height / 2 + padding;
        }
    }
}
