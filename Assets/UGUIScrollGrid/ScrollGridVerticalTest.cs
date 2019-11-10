using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollGridVerticalTest : MonoBehaviour
{
    //将模板cell的GameObject的节点拉到这里。
    public GameObject tempCell;
    void Start()
    {
        ScrollGridVertical scrollGridVertical = gameObject.AddComponent<ScrollGridVertical>();
        //步骤一：设置模板cell。
        scrollGridVertical.tempCell = tempCell;
        //步骤二:设置cell刷新的事件监听。
        scrollGridVertical.AddCellListener(this.OnCellUpdate);
        //步骤三：设置数据总数。
        //如果数据有新的变化，重新直接设置即可。
        scrollGridVertical.SetCellCount(183);
    }
    /// <summary>
    /// 监听cell的刷新消息，修改cell的数据。
    /// </summary>
    /// <param name="cell"></param>
    private void OnCellUpdate(ScrollGridCell cell) {
        cell.gameObject.GetComponentInChildren<Text>().text = cell.index.ToString();
    }
}
