using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
[System.Serializable]
public class BoolRow
{
    public bool[] cols;
}

public class Board : MonoBehaviour
{
    public static HashSet<(int, int)> checkList;
    public static HashSet<TileData> breakList;

    public BoolRow[] spawnMatrix;
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
    void OnValidate()
    {
        if (width <= 0 || height <= 0) return;

        if (spawnMatrix == null || spawnMatrix.Length != height)
        {
            InitSpawnMatrix();
        }
    }

    private void Start()
    {
        checkList = new HashSet<(int, int)>();
        breakList = new HashSet<TileData>();
        allTileData = new TileData[Width, Height];
        SetUp();
        //fixbug();
    }
    void InitSpawnMatrix()
    {
        spawnMatrix = new BoolRow[height];
        for (int y = 0; y < height; y++)
        {
            spawnMatrix[y] = new BoolRow();
            spawnMatrix[y].cols = new bool[width];
        }
    }

    private void SetUp()
    {
        offset = new Vector2((width - 1) / 2.0f, (height - 1) / 2.0f) * marginSize;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                AllTileData[i, j] = new TileData();
                if (spawnMatrix[j].cols[i]==false)
                {
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
                        dot.tileData = allTileData[i, j];
                        dot.board = this;
                        AllTileData[i, j].dot = dot;
                        AllTileData[i, j].x = i;
                        AllTileData[i, j].y = j;
                    }
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
            Debug.Log("tile:" + allTileData[i, j].dot.tileData.x+" " + allTileData[i,j].dot.tileData.y);
            int u = j + 1;
            int d = j - 1;
            while(u < height)
            {
                if (spawnMatrix[u].cols[i]==false && allTileData[i, u].dotType == allTileData[i, j].dotType)
                {
                    vertical.Add(allTileData[i, u]);
                } else break;
                u++;
            }
            while (d >= 0)
            {
                if (spawnMatrix[d].cols[i] == false && allTileData[i, d].dotType == allTileData[i, j].dotType)
                {
                    vertical.Add(allTileData[i, d]);
                }
                else break;
                d--;
            }
            StringBuilder s = new StringBuilder().Append("vertical\n");
            foreach (var x in vertical)
            {
                s.Append(x.dot.tileData.x + " " + x.dot.tileData.y + "\n");
            }
            Debug.Log(s.ToString());

            int l = i - 1;
            int r = i + 1;
            while (r < width)
            {
                if (spawnMatrix[j].cols[r] == false && allTileData[r, j].dotType == allTileData[i, j].dotType)
                {
                    horizontal.Add(allTileData[r, j]);
                }else break;
                r++;
            }
            while (l>=0)
            {
                if (spawnMatrix[j].cols[l] == false && allTileData[l, j].dotType == allTileData[i, j].dotType)
                {
                    horizontal.Add(allTileData[l, j]);
                }
                else break;
                l--;
            }
            StringBuilder sb = new StringBuilder().Append("horizontal\n");
            foreach (var x in vertical)
            {
                sb.Append(x.dot.tileData.x + " " + x.dot.tileData.y + "\n");
            }
            Debug.Log(sb.ToString());

            if (vertical.Count >= 3)
            {
                Board.breakList.UnionWith(vertical);
            }
            if (horizontal.Count >= 3)
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
        if (y >= 2)
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

    public void FillBoard()
    {

    }

    public void DropItem(int i,int j)
    {
        while(j<height && spawnMatrix[j].cols[i]==false)
        { 
            TileData tile1 = allTileData[i,j];
            TileData tile2 = allTileData[i,j-1];
            SwapTile(i,j,i,j-1);
        }
    }

    public void SwapTile(int x1, int y1,int x2,int y2)
    {
        TileData tile1 = AllTileData[x1,y1];
        TileData tile2 = AllTileData[x2,y2];

        allTileData[x1, y1] = tile2;
        allTileData[x2, y2] = tile1;

        tile2.x = x1;tile2.y = y1;
        tile1.x = x2;tile1.y = y2;

        if(tile1.gameObject!=null)StartCoroutine(SwapAnim(tile1.gameObject,x2,y2));
        if(tile2.gameObject!=null)StartCoroutine(SwapAnim(tile2.gameObject,x1,y1));
    }

    IEnumerator SwapAnim(GameObject o, int x, int y)
    {
        Vector2 targetPos = GetWorldPosition(x, y);
        TileData tile = allTileData[x, y];
        Debug.Log(tile.x+":"+x+ " " + tile.y+":"+y);
        while ((new Vector2(o.transform.position.x,o.transform.position.y)-targetPos).magnitude>0.01f) 
        {
            o.transform.position = Vector2.MoveTowards(o.transform.position, targetPos, Time.deltaTime*3f);
            yield return null;
        }
        o.transform.position = targetPos;
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
