using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private float marginSize;
    [SerializeField] private GameObject[] dots;
     
    private Vector2 offset;
    private TileData[,] allTileData;

    public TileData[,] AllTileData => allTileData;
    public int Width => width;
    public int Height => height;

    private void Start()
    {
        allTileData = new TileData[Width, Height];
        SetUp();
    }

    private void SetUp()
    {
        offset = new Vector2((width-1)/2.0f, (height-1)/2.0f) * marginSize;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                AllTileData[i, j] = new TileData();
                //tạo bảng gồm các ô
                Vector2 tempPotition = new Vector2(i, j)*marginSize;
                GameObject backgroundTile = Instantiate(tilePrefab, tempPotition - offset, Quaternion.identity) as GameObject;
                backgroundTile.transform.SetParent(transform);
                backgroundTile.name = "( " + i + ", " + j + " )";
                //tạo item trong mỗi ô
                int dotToUse = Random.Range(0, dots.Length);
                GameObject dotGameObject = Instantiate(dots[dotToUse], tempPotition - offset, Quaternion.identity);
                dotGameObject.transform.SetParent(backgroundTile.transform);
                dotGameObject.name = "( " + i + ", " + j + " )";
                dotGameObject.transform.localScale = Vector3.one * 0.9f;
                AllTileData[i, j].gameObject = dotGameObject;

                Dot dot = dotGameObject.GetComponent<Dot>();
                if(dot != null )
                {
                    dot.column = i;
                    dot.row = j;
                    dot.board = this;
                    AllTileData[i, j].dot = dot;
                }
            }
        }
    }

    public Vector2 GetWorldPosition(int i,int j)
    {
        return new Vector2(i*marginSize, j*marginSize)-offset;
    }

}
