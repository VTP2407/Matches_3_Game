using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int col;
    public int row;
    public int type;
    public Grid grid;

    private Vector2 firstTouch;
    private Vector2 lastTouch;
    public void SetUp(int x,int y)
    {
        col = x;
        row = y;
    }
    private void OnMouseDown()
    {
        firstTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        lastTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalcSwipe();
    }

    public void CalcSwipe()
    {
        if (grid.busy) return;
        Vector2 swipe = (lastTouch - firstTouch);
        if (swipe.magnitude > 0.5f)
        {
            if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
            {
                if (swipe.x > 0) { SwapItem(1, 0); Debug.Log("right"); }
                else { SwapItem(-1, 0); Debug.Log("left"); }
            }
            else
            {
                if (swipe.y > 0) { SwapItem(0, 1); Debug.Log("up"); }
                else {SwapItem(0, -1); Debug.Log("down"); }
            }
        }
    }

    public void SwapItem(int dx, int dy)
    {
        grid.busy = true;

        int oldCol = col;
        int oldRow = row;   

        int newCol = col + dx;
        int newRow = row + dy;
        grid.SwapItem(col, row, newCol, newRow);
        if (!grid.CheckMatches())
        {
            grid.SwapItem(oldCol, oldRow, newCol, newRow);
            grid.busy = false;
        }
    }
}
