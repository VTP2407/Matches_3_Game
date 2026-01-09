using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            board.AllTileData[oldColumn, oldRow].SwapData(board.AllTileData[newColumn, newRow]);

            Board.checkList.Add((oldColumn,oldRow) );
            Board.checkList.Add((newColumn,newRow) );
            StringBuilder s = new StringBuilder().Append("\n");
            foreach ( (int a,int b) in Board.checkList)
            {
                s.Append(a+" "+b+"\n");
            }
            Debug.Log(s.ToString());
            board.CheckMatches();
        }
    }

}
