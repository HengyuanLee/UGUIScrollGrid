using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class ScrollGridHorizontalTest : MonoBehaviour
{
    public GameObject tempCell;
    void Start()
    {
        ScrollGridHorizontal scrollGridVertical = gameObject.AddComponent<ScrollGridHorizontal>();
        scrollGridVertical.tempCell = tempCell;
        scrollGridVertical.AddCellListener(this.OnCellUpdate);
        scrollGridVertical.SetCellCount(153);
    }

    private void OnCellUpdate(ScrollGridCell cell)
    {
        cell.gameObject.GetComponentInChildren<Text>().text = cell.index.ToString();
    }
}
