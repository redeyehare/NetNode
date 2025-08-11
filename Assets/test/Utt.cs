using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Utt : VisualElement
{
    public new class UxmlFactory : UxmlFactory<Utt, UxmlTraits> { }

    public Utt()
    {
        // 1. 准备数据
        List<string> myData = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            myData.Add($"Item {i}");
        }

        // 2. 创建并配置 ListView
        var listView = new ListView
        {
            itemsSource = myData,
            makeItem = () => new Label(), // 精简 makeItem
            bindItem = (element, index) => (element as Label).text = myData[index],
            fixedItemHeight = 20, // 如果 UXML 中已设置，这里可以省略
            selectionType = SelectionType.Single,
            showBorder = true,
            // showAlternatingRowBackgrounds = AlternatingRowBackgrounds.All, // 移除此行
            reorderable = false, // 关闭手动排序
            // reorderMode = ListViewReorderMode.Animated,
            // showAddRemoveFooter = true,
            // headerTitle = "My List",
            // showFoldoutHeader = true
        };

        // 3. 订阅事件 (如果需要)
        listView.itemsChosen += (items) => Debug.Log($"Items chosen: {string.Join(", ", items)}");
        listView.selectionChanged += (items) => Debug.Log($"Selection changed: {string.Join(", ", items)}");

        // 4. 将 ListView 添加到当前 VisualElement (Utt)
        Add(listView);
    }
}
