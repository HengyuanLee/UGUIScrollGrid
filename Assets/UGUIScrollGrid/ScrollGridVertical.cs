using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ScrollGridVertical : MonoBehaviour
{
    public GameObject tempCell;//模板cell，以此为目标，克隆出每个cell。
    private int cellCount;//要显示数据的总数。
    private float cellWidth;
    private float cellHeight;

    private List<System.Action<ScrollGridCell>> onCellUpdateList = new List<System.Action<ScrollGridCell>>();
    private ScrollRect scrollRect;

    private int row;//克隆cell的GameObject数量的行。
    private int col;//克隆cell的GameObject数量的列。

    private List<GameObject> cellList = new List<GameObject>();
    private bool inited;

    public void AddCellListener(System.Action<ScrollGridCell> call)
    {
        this.onCellUpdateList.Add(call);
        this.RefreshAllCells();
    }
    public void RemoveCellListener(System.Action<ScrollGridCell> call)
    {
        this.onCellUpdateList.Remove(call);
    }
    /// <summary>
    /// 设置ScrollGrid要显示的数据数量。
    /// </summary>
    /// <param name="count"></param>
    public void SetCellCount(int count)
    {
        this.cellCount = Mathf.Max(0, count);

        if (this.inited == false)
        {
            this.Init();
        }
        //重新调整content的高度，保证能够包含范围内的cell的anchoredPosition，这样才有机会显示。
        float newContentHeight = this.cellHeight * Mathf.CeilToInt((float)cellCount / this.col);
        float newMinY = -newContentHeight + this.scrollRect.viewport.rect.height;
        float maxY = this.scrollRect.content.offsetMax.y;
        newMinY += maxY;//保持位置
        newMinY = Mathf.Min(maxY, newMinY);//保证不小于viewport的高度。
        this.scrollRect.content.offsetMin = new Vector2(0, newMinY);
        this.CreateCells();
    }

    private void Init()
    {
        if (tempCell == null) {
            Debug.LogError("tempCell不能为空！");
            return;
        }
        this.inited = true;
        this.tempCell.SetActive(false);

        //创建ScrollRect下的viewpoint和content节点。
        this.scrollRect = gameObject.AddComponent<ScrollRect>();
        this.scrollRect.vertical = true;
        this.scrollRect.horizontal = false;
        GameObject viewport = new GameObject("viewport", typeof(RectTransform));
        viewport.transform.parent = transform;
        this.scrollRect.viewport = viewport.GetComponent<RectTransform>();
        GameObject content = new GameObject("content", typeof(RectTransform));
        content.transform.parent = viewport.transform;
        this.scrollRect.content = content.GetComponent<RectTransform>();

        //设置视野viewport的宽高和根节点一致。
        this.scrollRect.viewport.localScale = Vector3.one;
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
        this.scrollRect.viewport.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
        this.scrollRect.viewport.anchorMin = Vector2.zero;
        this.scrollRect.viewport.anchorMax = Vector2.one;

        //设置viewpoint的mask。
        this.scrollRect.viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        Image image = this.scrollRect.viewport.gameObject.AddComponent<Image>();
        Rect viewRect = this.scrollRect.viewport.rect;
        image.sprite = Sprite.Create(new Texture2D(1, 1), new Rect(Vector2.zero, Vector2.one), Vector2.zero);

        //获取模板cell的宽高。
        Rect tempRect = tempCell.GetComponent<RectTransform>().rect;
        this.cellWidth = tempRect.width;
        this.cellHeight = tempRect.height;

        //设置viewpoint约束范围内的cell的GameObject的行列数。
        this.col = (int)(this.scrollRect.viewport.rect.width / this.cellWidth);
        this.col = Mathf.Max(1, this.col);
        this.row = Mathf.CeilToInt(this.scrollRect.viewport.rect.height / this.cellHeight);

        //初始化content。
        this.scrollRect.content.localScale = Vector3.one;
        this.scrollRect.content.offsetMax = new Vector2(0, 0);
        this.scrollRect.content.offsetMin = new Vector2(0, 0);
        this.scrollRect.content.anchorMin = Vector2.zero;
        this.scrollRect.content.anchorMax = Vector2.one;
        this.scrollRect.onValueChanged.AddListener(this.OnValueChange);
        this.CreateCells();

    }
    /// <summary>
    /// 刷新每个cell的数据
    /// </summary>
    public void RefreshAllCells()
    {
        foreach (GameObject cell in this.cellList)
        {
            this.cellUpdate(cell);
        }
    }
    /// <summary>
    /// 创建每个cell，并且根据行列定它们的位置，最多创建能够在视野范围内看见的个数，加上一行隐藏待进入视野的cell。
    /// </summary>
    private void CreateCells() {
        for (int r = 0; r < this.row + 1; r++)
        {
            for (int l = 0; l < this.col; l++)
            {
                int index = r * this.col + l;
                if (index < this.cellCount)
                {
                    if (this.cellList.Count <= index)
                    {
                        GameObject newcell = GameObject.Instantiate<GameObject>(this.tempCell);
                        newcell.SetActive(true);
                        //cell节点锚点强制设为左上角，以此方便算出位置。
                        RectTransform cellRect = newcell.GetComponent<RectTransform>();
                        cellRect.anchorMin = new Vector2(0, 1);
                        cellRect.anchorMax = new Vector2(0, 1);

                        //分别算出每个cell的位置。
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

    /// <summary>
    /// 滚动过程中，重复利用cell
    /// </summary>
    /// <param name="pos"></param>
    private void OnValueChange(Vector2 pos)
    {
        foreach (GameObject cell in this.cellList)
        {
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            float dist = this.scrollRect.content.offsetMax.y + cellRect.anchoredPosition.y;
            float maxTop = this.cellHeight / 2;
            float minBottom = -((this.row + 1) * this.cellHeight) + this.cellHeight / 2;
            if (dist > maxTop)
            {
                float newY = cellRect.anchoredPosition.y - (this.row + 1) * this.cellHeight;
                //保证cell的anchoredPosition只在content的高的范围内活动，下同理
                if (newY > -this.scrollRect.content.rect.height)
                {
                    //重复利用cell，重置位置到视野范围内。
                    cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
                    this.cellUpdate(cell);
                }

            }
            else if (dist < minBottom)
            {
                float newY = cellRect.anchoredPosition.y + (this.row + 1) * this.cellHeight;
                if (newY < 0)
                {
                    cellRect.anchoredPosition = new Vector3(cellRect.anchoredPosition.x, newY);
                    this.cellUpdate(cell);
                }
            }
        }
    }
    /// <summary>
    /// 所有的数据的真实行数
    /// </summary>
    private int allRow { get { return Mathf.CeilToInt((float)this.cellCount / this.col); } }
    /// <summary>
    /// cell被刷新时调用，算出cell的位置并调用监听的回调方法（Action）。
    /// </summary>
    /// <param name="cell"></param>
    private void cellUpdate(GameObject cell)
    {
        RectTransform cellRect = cell.GetComponent<RectTransform>();
        int x = Mathf.CeilToInt((cellRect.anchoredPosition.x - this.cellWidth / 2) / this.cellWidth);
        int y = Mathf.Abs(Mathf.CeilToInt((cellRect.anchoredPosition.y + this.cellHeight / 2) / this.cellHeight));
        int index = y * this.col + x;
        ScrollGridCell scrollGridCell = cell.GetComponent<ScrollGridCell>();
        scrollGridCell.UpdatePos(x, y, index);
        if (index >= cellCount || y >= this.allRow)
        {
            //超出数据范围
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
