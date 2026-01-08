using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData
{
    public GameObject gameObject;
    public Dot dot;
    public int dotType;

    public TileData(){}
    public void SwapData(TileData data)
    {
        GameObject gameObject = this.gameObject;
        Dot dot = this.dot;
        int dotType = this.dotType;
        this.gameObject = data.gameObject;
        this.dot = data.dot;
        this.dotType = data.dotType;
        data.gameObject = gameObject;
        data.dot = dot;
        data.dotType = dotType;
    }
}
