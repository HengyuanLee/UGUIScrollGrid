using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScrollGridHorizontal : MonoBehaviour
{
    public GameObject tempCell;
    private int cellCount;
    private float cellWidth;
    private float cellHeight;
    private List<System.Action<ScrollGridCell>> onCellUpdateList = new List<System.Action<ScrollGridCell>>();

    private ScrollRect scrollRect;

    private int row;
    private int col;

    private bool inited;
    private List<GameObject> cellList = new List<GameObject>();

    public void AddCellListener(System.Action<ScrollGridCell> call)
    {
        this.onCellUpdateList.Add(call);
        this.RefreshAllCells();
    }
    public void RemoveCellListener(System.Action<ScrollGridCell> call)
    {
        this.onCellUpdateList.Remove(call);
    }
    public void SetCellCount(int count)
    {
        this.cellCount = Mathf.Max(0, count);

        if (this.inited == false)
        {
            this.Init();
        }
        float newContentWidth = this.cellWidth * Mathf.CeilToInt((float)this.cellCount / this.row);
        float newMaxX = newContentWidth - this.scrollRect.viewport.rect.width;//当minX==0时maxX的位置
        float minX = this.scrollRect.content.offsetMin.x;
        newMaxX += minX;
        newMaxX = Mathf.Max(minX, newMaxX);

        this.scrollRect.content.offsetMax = new Vector2(newMaxX, 0);
        this.CreateCells();
    }
    public void Init() { 
        if (tempCell == null) {
            Debug.LogError("tempCell不能为空！");
            return;
        }
        this.inited = true;
        this.tempCell.SetActive(false);

        this.scrollRect = gameObject.AddComponent<ScrollRect>();
        this.scrollRect.vertical = false;
        this.scrollRect.horizontal = true;
        GameObject viewport = new GameObject("viewport", typeof(RectTransform));
        viewport.transform.parent = transform;
        this.scrollRect.viewport = viewport.GetComponent<RectTransform>();
        GameObject content = new GameObject("content", typeof(RectTransform));
        content.transform.parent = viewport.transform;
        this.scrollRect.content = content.GetComponent<RectTransform>();

        this.scrollRect.viewport.localScale = Vector3.one;
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
        this.scrollRect.viewport.anchorMin = Vector2.zero;
        this.scrollRect.viewport.anchorMax = Vector2.one;

        this.scrollRect.viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        Image image = this.scrollRect.viewport.gameObject.AddComponent<Image>();
        Rect viewRect = this.scrollRect.viewport.rect;
        image.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(Vector2.zero, Vector2.one), Vector2.zero);
        Rect tempRect = tempCell.GetComponent<RectTransform>().rect;
        this.cellWidth = tempRect.width;
        this.cellHeight = tempRect.height;
        this.row = Mathf.FloorToInt(this.scrollRect.viewport.rect.height / this.cellHeight);
        this.row = Mathf.Max(1, this.row);
        this.col = Mathf.CeilToInt(this.scrollRect.viewport.rect.width / this.cellWidth);
        this.scrollRect.content.localScale = Vector3.one;
        this.scrollRect.content.offsetMax = new Vector2(0, 0);
        this.scrollRect.content.offsetMin = new Vector2(0, 0);
        this.scrollRect.content.anchorMin = Vector2.zero;
        this.scrollRect.content.anchorMax = Vector2.one;
        this.scrollRect.onValueChanged.AddListener(this.OnValueChange);
        this.CreateCells();
    }
    public void RefreshAllCells() {
        foreach (GameObject cell in this.cellList)
        {
            this.cellUpdate(cell);
        }
    }
    private void CreateCells()
    {
        for (int r = 0; r < this.row; r++)
        {
            for (int l = 0; l < this.col + 1; l++)
            {
                int index = r * (this.col+1) + l;
                if (index < this.cellCount)
                {
                    if (this.cellList.Count <= index)
                    {
                        GameObject newcell = GameObject.Instantiate<GameObject>(this.tempCell);
                        newcell.SetActive(true);
                        RectTransform cellRect = newcell.GetComponent<RectTransform>();
                        cellRect.anchorMin = new Vector2(0, 1);
                        cellRect.anchorMax = new Vector2(0, 1);

                        float x = this.cellWidth / 2 + l * this.cellWidth;
                        float y = -r * this.cellHeight - this.cellHeight / 2;
                        cellRect.SetParent(this.scrollRect.content);
                        cellRect.localScale = Vector3.one;
                        cellRect.anchoredPosition = new Vector3(x, y);
                        newcell.AddComponent<ScrollGridCell>().SetObjIndex(index);
                        this.cellList.Add(newcell);
                    }
                }
            }
        }
        this.RefreshAllCells();
    }

    private void OnValueChange(Vector2 pos)
    {
        foreach (GameObject cell in this.cellList)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            float dist = this.scrollRect.content.offsetMin.x + cellRect.anchoredPosition.x;
            float minLeft =  -this.cellWidth / 2;
            float maxRight = this.col * this.cellWidth + this.cellWidth / 2;
            //限定复用边界
            if (dist < minLeft)
            {
                //控制cell的anchoredPosition在content的范围内才重复利用。
                float newX = cellRect.anchoredPosition.x + (this.col + 1) * this.cellWidth;
                if (newX < this.scrollRect.content.rect.width)
                {
                    cellRect.anchoredPosition = new Vector3(newX, cellRect.anchoredPosition.y);
                    this.cellUpdate(cell);
                }
            }
            if (dist > maxRight)
            {
                float newX = cellRect.anchoredPosition.x - (this.col + 1) * this.cellWidth;
                if (newX > 0) {
                    cellRect.anchoredPosition = new Vector3(newX, cellRect.anchoredPosition.y);
                    this.cellUpdate(cell);
                }
            }
        }
    }
    private int allCol{ get { return Mathf.CeilToInt((float)this.cellCount / this.row); } }
    private void cellUpdate(GameObject cell)
    {
        RectTransform cellRect = cell.GetComponent<RectTransform>();
        int x = Mathf.CeilToInt((cellRect.anchoredPosition.x - cellWidth / 2) / cellWidth);
        int y = Mathf.Abs(Mathf.CeilToInt((cellRect.anchoredPosition.y + cellHeight / 2) / cellHeight));

        int index = y * allCol + x;
        ScrollGridCell scrollGridCell = cell.GetComponent<ScrollGridCell>();
        scrollGridCell.UpdatePos(x, y, index);
        if (index >= cellCount || x >= this.allCol)
        {
            cell.SetActive(false);
        }
        else
        {
            if (cell.activeSelf == false)
            {
                cell.SetActive(true);
            }
            foreach (var call in this.onCellUpdateList)
            {
                call(scrollGridCell);
            }
        }

    }
}
