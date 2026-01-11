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
    public bool isRefilling;

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
                gridDatas[col, row].gameObject.transform.parent = tile.transform;
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

    public IEnumerator SwapCoroutine(int col1, int row1, int col2, int row2)
    {
        busy = true;

        if (!IsInside(col2, row2))
        {
            busy = false;
            yield break;
        }

        ItemData a = gridDatas[col1, row1];
        ItemData b = gridDatas[col2, row2];

        if (a == null || b == null)
        {
            busy = false;
            yield break;
        }

        SwapItem(col1, row1, col2, row2);

        yield return StartCoroutine(SwapTransformCoroutine(a.item,b.item));

        if (CheckMatches())
        {
            yield return StartCoroutine(ClearAndRefillCoroutine());
        }
        else
        {
            SwapItem(col2, row2, col1, row1);
            yield return StartCoroutine(SwapTransformCoroutine(a.item, b.item));
        }

        busy = false;
    }
    public bool IsInside(int col, int row)
    {
        return col >= 0 && col < width && row >= 0 && row < height;
    }


    public void SwapItem(int col1,int row1,int col2,int row2)
    {
        ItemData item1 = gridDatas[col1, row1];
        ItemData item2 = gridDatas[col2, row2];

        if (item1 == null || item2 == null) return;

        gridDatas[col2, row2] = item1;
        gridDatas[col1, row1] = item2;

        int tmpCol = item1.item.col;
        item1.item.col = item2.item.col;
        item2.item.col = tmpCol;

        int tmpRow = item1.item.row;
        item1.item.row = item2.item.row;
        item2.item.row = tmpRow;

        listCheck.Clear();
        listCheck.Add(item1);
        listCheck.Add(item2);
    }

    private IEnumerator SwapTransformCoroutine(Item i1, Item i2, float duration = 0.18f)
    {
        Vector3 startPos1 = i1.transform.position;
        Vector3 startPos2 = i2.transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t); // ease-out

            if (i1 != null) i1.transform.position = Vector3.Lerp(startPos1, startPos2, eased);
            if (i2 != null) i2.transform.position = Vector3.Lerp(startPos2, startPos1, eased);

            yield return null;
        }

        // chắc chắn chạm đúng target
        if (i1 != null) i1.transform.position = startPos2;
        if (i2 != null) i2.transform.position = startPos1;
    }

    public IEnumerator ClearAndRefillCoroutine()
    {
        BreakItem();
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(RefillGridCoroutine());
        yield return new WaitForSeconds(0.1f);
        if (CheckMatches())
            yield return StartCoroutine(ClearAndRefillCoroutine());
    }
    public bool CheckMatches()
    {
        listBreak.Clear();

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
    }

    public IEnumerator RefillGridCoroutine()
    {
        // Chặn input
        isRefilling = true;

        foreach (int col in colFills)
        {
            List<Item> movingItems = new List<Item>();

            // ===== 1️⃣ DỒN ITEM CŨ XUỐNG (1 PHÁT) =====
            int targetRow = 0;

            for (int row = 0; row < height; row++)
            {
                if (gridDatas[col, row] != null)
                {
                    if (row != targetRow)
                    {
                        ItemData id = gridDatas[col, row];

                        gridDatas[col, row] = null;
                        gridDatas[col, targetRow] = id;

                        id.item.col = col;
                        id.item.row = targetRow;

                        StartCoroutine(MoveItemCoroutine(id.item));
                        movingItems.Add(id.item);
                    }

                    targetRow++;
                }
            }

            // ===== 2️⃣ SINH ITEM MỚI Ở PHÍA TRÊN =====
            for (int row = targetRow; row < height; row++)
            {
                Item item = CreateNewItem(col, row);
                StartCoroutine(MoveItemCoroutine(item));
                movingItems.Add(item);
            }

            // ===== 3️⃣ CHỜ TẤT CẢ ITEM RƠI XONG =====
            yield return new WaitUntil(() =>
            {
                foreach (Item item in movingItems)
                {
                    if (item.isMoving) return false;
                }
                return true;
            });
        }

        // ===== 4️⃣ CHUẨN BỊ CHECK MATCH =====
        PrepareListCheck();

        isRefilling = false;
    }

    private void PrepareListCheck()
    {
        listCheck.Clear();

        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                if (gridDatas[col, row] != null)
                    listCheck.Add(gridDatas[col, row]);
            }
        }

        colFills.Clear();
        listBreak.Clear();
    }


    IEnumerator MoveItemCoroutine(Item item)
    {
        if (item == null) yield break;

        item.isMoving = true;

        Vector3 targetPos = GetWorldTransform(item.col, item.row);

        while (Vector3.Distance(item.transform.position, targetPos) > 0.01f)
        {
            item.transform.position = Vector3.MoveTowards(
                item.transform.position,
                targetPos,
                Time.deltaTime * item.speed
            );
            yield return null;
        }

        item.transform.position = targetPos;

        item.isMoving = false;
    }


    public Item CreateNewItem(int col,int row)
    {
        int type = Random.Range(0, itemPrefabs.Length);
        GameObject obj = Instantiate(
            itemPrefabs[type],
            GetWorldTransform(col, height),
            Quaternion.identity
        );

        Item item = obj.GetComponent<Item>();

        gridDatas[col, row] = new ItemData(obj, item);
        item.col = col;
        item.row = row;
        item.grid = this;
        item.type = type;

        return item;
    }
}
