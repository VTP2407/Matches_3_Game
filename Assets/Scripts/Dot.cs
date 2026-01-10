using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Dot : MonoBehaviour
{
    public Board board;
    public TileData tileData;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private void Start()
    {
       
    }


    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log($"CLICK DOT: {name} at ({column},{row})");
        //Debug.Log(firstTouchPosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log($"CLICK UP: {name} at ({column},{row})");
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
        Board.checkList.Clear();
        int newColumn = tileData.x + x;
        int newRow = tileData.y + y;
        if (newColumn < 0 || newRow < 0 || newColumn >= board.Width || newRow >= board.Height) return;

        board.SwapTile(newColumn, newRow, tileData.x,tileData.y);
    }

}
