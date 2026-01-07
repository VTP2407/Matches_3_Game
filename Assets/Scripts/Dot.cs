using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    public float swipeAngle = 0;
    public int row;
    public int column;
    public int targetX;
    public int targetY;
    public Board board;

    private Dot otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private void Start()
    {
        board = FindObjectOfType<Board>();
    }

    private void Update()
    {
        targetX = column;
        targetY = row;

        Vector2 targetPos = board.GetWorldPosition(targetX, targetY);
        transform.position = Vector2.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime*10f
        );
    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(firstTouchPosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        MovePieces();
    }

    void MovePieces()
    {
        Vector2 swipe = finalTouchPosition - firstTouchPosition;
        if (swipe.magnitude > 0.5f)
        {
            if (Mathf.Abs(swipe.x) >= Mathf.Abs(swipe.y))
            {
                if (swipe.x > 0) SwapTile(1, 0);//right
                else SwapTile(-1, 0);//left
            }
            else
            {
                if (swipe.y > 0) SwapTile(0, 1);//up
                else SwapTile(0, -1);//down
            }
        }
        Debug.Log(swipe + " " + swipe.magnitude);
    }

    void SwapTile(int x, int y)
    {
        int newColumn = column + x;
        int newRow = row + y;
        if (newColumn < 0 || newRow < 0 || newColumn >= board.Width || newRow >= board.Height) return;

        GameObject otherDotObject = board.AllTileData[newColumn, newRow].gameObject;
        if (otherDotObject != null)
        {
            otherDot = board.AllTileData[newColumn,newRow].dot;

            int oldColumn = column;
            int oldRow = row;
            otherDot.column = column;
            otherDot.row = row;
            column = newColumn;
            row = newRow;

            string name = otherDotObject.name;
            otherDotObject.name = this.gameObject.name;
            this.gameObject.name = name;

            board.AllTileData[oldColumn, oldRow].gameObject= otherDotObject;
            board.AllTileData[oldColumn, oldRow].dot = otherDot;

            board.AllTileData[newColumn, newRow].gameObject = gameObject;
            board.AllTileData[newColumn, newRow].dot = this; 
        }
    }

}
