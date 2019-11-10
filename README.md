# UGUIScrollGrid

unity中利用ugui制作scrollview有多个格子滑动时，最直接的做法是创建对应数量个格子节点，利用GameObject.Instanate创建节点本身就是性能开销很大的，如果有500个，1000个或者更多数据要显示，要创建这么多个节点，那么这卡顿一定很明显，这个数量级用这个做法实为下策。
如果接触过安卓/iOS原生app开发的应该记得它们的Scrollview / Tableview是有一套Item/Cell的复用机制的，就是当某个节点滑动出Scrollview 范围时，消失了不显示了，那么则移动到新的等待重新进入Scrollview 视野的位置重复利用，填充新的数据来显示，而不是创建新的节点来显示新的数据，从而节约性能的开销，所以即使显示十万条数据也不会卡顿。
通过这个思路，用Unity的UGUI实现了一遍，以此来提高显示大量数据时的Scrollview性能，这是十分有效的。
缺点：但也要注意的问题是，每个节点显示的数据总是随着Scrollview的滑动而变化的，也就是说节点和并不是某条数据绑定，而是动态变化的。所以，操作cell节点的UI引起数据变化时，需要我们谨慎操作，考虑到UI的cell节点所对应的的数据是哪条。
————————————————
博客说明：https://blog.csdn.net/u012740992/article/details/102994534

用法：

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
