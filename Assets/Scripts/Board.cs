using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Board : MonoBehaviour
{
    public static HashSet<(int, int)> checkList;
    public static HashSet<TileData> breakList;

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
        checkList = new HashSet<(int, int)>();
        breakList = new HashSet<TileData>();
        allTileData = new TileData[Width, Height];
        SetUp();
        //fixbug();
    }

    private void SetUp()
    {
        offset = new Vector2((width - 1) / 2.0f, (height - 1) / 2.0f) * marginSize;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                AllTileData[i, j] = new TileData();
                //tạo bảng gồm các ô
                Vector2 tempPotition = new Vector2(i, j) * marginSize;
                GameObject backgroundTile = Instantiate(tilePrefab, tempPotition - offset, Quaternion.identity) as GameObject;
                backgroundTile.transform.SetParent(transform);
                backgroundTile.name = "( " + i + ", " + j + " )";
                //tạo item trong mỗi ô
                GameObject dotToUse = CreateItem(i, j);
                GameObject dotGameObject = Instantiate(dotToUse, tempPotition - offset, Quaternion.identity);
                dotGameObject.transform.SetParent(backgroundTile.transform);
                dotGameObject.name = "( " + i + ", " + j + " )";
                dotGameObject.transform.localScale = Vector3.one * 0.9f;
                AllTileData[i, j].gameObject = dotGameObject;

                Dot dot = dotGameObject.GetComponent<Dot>();
                if (dot != null)
                {
                    dot.column = i;
                    dot.row = j;
                    dot.board = this;
                    AllTileData[i, j].dot = dot;
                }
            }
        }
    }

    public Vector2 GetWorldPosition(int i, int j)
    {
        return new Vector2(i * marginSize, j * marginSize) - offset;
    }

    public void CheckMatches()
    {
        foreach(var(i,j) in Board.checkList)
        {
            HashSet<TileData> horizontal = new HashSet<TileData>();
            HashSet<TileData> vertical = new HashSet<TileData>();
            horizontal.Add(allTileData[i, j]);
            vertical.Add(allTileData[i, j]);
            Debug.Log("tile:" + allTileData[i, j].dot.targetX+" " + allTileData[i,j].dot.targetY);
            int u = j + 1;
            int d = j - 1;
            while(u < height)
            {
                if (allTileData[i, u].dotType == allTileData[i, j].dotType)
                {
                    vertical.Add(allTileData[i, u]);
                } else break;
                u++;
            }
            while (d >= 0)
            {
                if (allTileData[i, d].dotType == allTileData[i, j].dotType)
                {
                    vertical.Add(allTileData[i, d]);
                }
                else break;
                d--;
            }
            StringBuilder s = new StringBuilder().Append("vertical\n");
            foreach (var x in vertical)
            {
                s.Append(x.dot.targetX + " " + x.dot.targetY + "\n");
            }
            Debug.Log(s.ToString());

            int l = i - 1;
            int r = i + 1;
            while (r < width)
            {
                if (allTileData[r, j].dotType == allTileData[i, j].dotType)
                {
                    horizontal.Add(allTileData[r, j]);
                }else break;
                r++;
            }
            while (l>=0)
            {
                if (allTileData[l, j].dotType == allTileData[i, j].dotType)
                {
                    horizontal.Add(allTileData[l, j]);
                }
                else break;
                l--;
            }
            StringBuilder sb = new StringBuilder().Append("horizontal\n");
            foreach (var x in vertical)
            {
                sb.Append(x.dot.targetX + " " + x.dot.targetY + "\n");
            }
            Debug.Log(sb.ToString());

            if (vertical.Count >= 2)
            {
                Board.breakList.UnionWith(vertical);
            }
            if (horizontal.Count >= 2)
            {
                Board.breakList.UnionWith(horizontal);
            }
            Debug.Log("Vertical:"+vertical.Count+" Horizontal:"+horizontal.Count);
        }
        BreakTile();
    }

    public void BreakTile()
    {
        foreach(var o in Board.breakList.ToList())
        {
            Destroy(o.gameObject);
        }
        Board.breakList.Clear();
    }

    public GameObject CreateItem(int x, int y)
    {
        List<GameObject> listItem = new List<GameObject>(dots);
        if (x >= 2)
        {
            TileData left1 = allTileData[x - 1, y];
            TileData left2 = allTileData[x - 2, y];
            if (left1.dotType == left2.dotType)
            {
                listItem.Remove(dots[left1.dotType]);
            }
        }
        if (y >= 3)
        {
            TileData down1 = allTileData[x, y - 1];
            TileData down2 = allTileData[x, y - 2];
            if (down1.dotType == down2.dotType)
            {
                listItem.Remove(dots[down1.dotType]);
            }
        }
        if (listItem.Count > 0)
        {
            int type = Random.Range(0, listItem.Count);
            GameObject chosenDot = listItem[type];
            allTileData[x, y].dotType = System.Array.IndexOf(dots, chosenDot);
            return listItem[type];
        }
        int idx = Random.Range(0, dots.Length);
        allTileData[x, y].dotType = idx;
        return dots[idx];
    }

    public void fixbug()
    {
        StringBuilder sb = new StringBuilder().Append("\n");
        for (int j = height - 1; j >= 0; j--)
        {
            for (int i = 0; i < width; i++)
            {
                sb.Append(allTileData[i, j].dotType + " ");
            }
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
}
