using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemData
{
    public GameObject gameObject;
    public Item item;

    public ItemData(GameObject gameObject, Item item)
    {
        this.gameObject = gameObject;
        this.item = item;
    }
}
public class Grid : MonoBehaviour
{
    public int height;
    public int width;
    public GameObject[] itemPrefabs;
    public GameObject tilePrefab;
    public float marginSize;
    public bool busy;

    private ItemData[,] gridDatas;

    private Vector2 offset;
    private HashSet<ItemData> listCheck;
    private HashSet<ItemData> listBreak;
    private HashSet<int> colFills;

    private void Start()
    {
        busy = false;
        colFills = new HashSet<int>();
        listCheck = new HashSet<ItemData>();
        listBreak = new HashSet<ItemData>();
        gridDatas = new ItemData[width,height];
        SetUp();
    }

    public void SetUp()
    {
        offset = new Vector2((width-1)/2f, (height-1)/2f)*marginSize;
        for(int row = 0; row < height; row++)
        {
            for(int col = 0; col < width; col++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector2(col, row)*marginSize - offset, Quaternion.identity);
                tile.transform.parent = transform;

                gridDatas[col, row] = CreateItem(col, row);
            }
        }
    }

    public ItemData CreateItem(int col,int row)
    {
        int type;
        int safety = 0;
        int maxTry = 50;

        do
        {
            type = Random.Range(0, itemPrefabs.Length);
            safety++;
        }
        while(IsMatchAt(col,row,type) && safety < maxTry);

        GameObject obj = Instantiate(itemPrefabs[type], GetWorldTransform(col, row), Quaternion.identity);

        Item item = obj.GetComponent<Item>();
        item.type = type;
        item.col = col;
        item.row = row;
        item.grid = this;

        return new ItemData(obj,item);

    }

    public bool IsMatchAt(int col, int row, int type)
    {
        //horizontal
        if (col >= 2)
        {
            if (gridDatas[col - 1, row].item.type == type && gridDatas[col - 2, row].item.type == type)
                return true;
        }
        //vertical
        if (row >= 2)
        {
            if (gridDatas[col, row-1].item.type == type && gridDatas[col, row-2].item.type == type)
                return true;
        }
        return false;
    }

    public Vector2 GetWorldTransform(int col,int row)
    {
        return new Vector2(col, row)*marginSize-offset;
    }

    public void SwapItem(int col1,int row1,int col2,int row2)
    {
        if (col1 < 0 || col1 >= width || row1 < 0 || row1 >= height ||
        col2 < 0 || col2 >= width || row2 < 0 || row2 >= height)
            return;

        ItemData item1 = gridDatas[col1, row1];
        ItemData item2 = gridDatas[col2, row2];

        if (item1 == null || item2 == null) return;

        gridDatas[col2, row2] = item1;
        gridDatas[col1, row1] = item2;

        SwapTransform(item1.item, item2.item);

        listCheck.Clear();
        listCheck.Add(item1);
        listCheck.Add(item2);
    }

    public void SwapTransform(Item i1, Item i2)
    {
        int tmp = i1.col;
        i1.col = i2.col;
        i2.col = tmp;

        tmp = i1.row;
        i1.row = i2.row;
        i2.row = tmp;

        Vector2 pos = i1.transform.position;
        i1.transform.position = i2.transform.position;
        i2.transform.position = pos;
    }

    public bool CheckMatches()
    {
        bool check = false;
        foreach(ItemData id in listCheck)
        {
            HashSet<ItemData> vertical = new HashSet<ItemData>();
            HashSet<ItemData> horizontal = new HashSet<ItemData>();

            int i = id.item.col;
            int j = id.item.row;
            int type = id.item.type;

            vertical.Add(gridDatas[i, j]);
            horizontal.Add(gridDatas[i, j]);

            // UP
            for (int y = j + 1; y < height && gridDatas[i, y]?.item?.type == type; y++)
                vertical.Add(gridDatas[i, y]);

            // DOWN
            for (int y = j - 1; y >= 0 && gridDatas[i, y]?.item?.type == type; y--)
                vertical.Add(gridDatas[i, y]);

            // RIGHT
            for (int x = i + 1; x < width && gridDatas[x, j]?.item?.type == type; x++)
                horizontal.Add(gridDatas[x, j]);

            // LEFT
            for (int x = i - 1; x >= 0 && gridDatas[x, j]?.item?.type == type; x--)
                horizontal.Add(gridDatas[x, j]);

            if (horizontal.Count >= 3)
                listBreak.UnionWith(horizontal);

            if (vertical.Count >= 3)
                listBreak.UnionWith(vertical);

            Debug.Log($"({i},{j}) H:{horizontal.Count} V:{vertical.Count}");
        }
        if (listBreak.Count > 0) 
        {
            BreakItem();
            check = true;
        }
        return check;
    }

    public void BreakItem()
    {
        foreach(var i in listBreak)
        {
            int col = i.item.col;
            int row = i.item.row;  

            colFills.Add(i.item.col);
            Destroy(i.gameObject);

            gridDatas[col, row] = null;
        }
        Debug.Log("BreakItem: "+"List Check:" + listCheck.Count + " List Break:" + listBreak.Count);
        RefillGrid();
    }

    public void RefillGrid()
    {
        foreach(int col in colFills)
        {
            for(int row = 0; row < height; row++)
            {
                if(gridDatas[col,row] == null)
                {
                    for(int k = row + 1; k < height; k++)
                    {
                        if (gridDatas[col, height - 1] == null)
                        {
                            CreateNewItem(col, height - 1);
                        }
                        if(gridDatas[col, k]?.gameObject != null)
                        {
                            MoveItem(col,k,col,row);
                            break;
                        }
                    }
                    if (gridDatas[col, height - 1] == null)
                    {
                        CreateNewItem(col, height - 1);
                    }
                }
            }
        }
        
        listCheck.Clear();

        foreach(int col in colFills)
        {
            for(int row = 0; row < height; row++)
            {
                if (gridDatas[col,row] != null)
                {
                    listCheck.Add(gridDatas[col,row]);
                }
            }
        }

        colFills.Clear();
        listBreak.Clear();

        if (CheckMatches() == false)
        {
            busy = false;
        }

        Debug.Log("List Check:"+listCheck.Count+" List Break:"+listBreak.Count);
    }

    public void MoveItem(int col1,int row1,int col2,int row2)
    {
        ItemData id = gridDatas[col1,row1];

        gridDatas[col1,row1] = null;
        gridDatas[col2,row2] = id;

        id.item.col = col2;
        id.item.row = row2;

        id.gameObject.transform.position = GetWorldTransform(col2,row2);
    }

    public void CreateNewItem(int col,int row)
    {
        int type = Random.Range(0, itemPrefabs.Length);
        GameObject obj = Instantiate(itemPrefabs[type], GetWorldTransform(col, row), Quaternion.identity);
        Item item = obj.GetComponent<Item>();
        if (item != null)
        {
            gridDatas[col, row] = new ItemData(obj,item);
            item.col = col;
            item.row = row;
            item.grid = this;
            item.type = type;
        }  
    }
}
