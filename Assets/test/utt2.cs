using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

public class utt2 : VisualElement
{
    public new class UxmlFactory : UxmlFactory<utt2,UxmlTraits>{}
    public utt2()
    {
        // Create some list of data, here simply numbers in interval [1, 1000]
const int itemCount = 1000;
var items = new List<string>(itemCount);
for (int i = 1; i <= itemCount; i++)
    items.Add(i.ToString());

// The "makeItem" function will be called as needed
// when the ListView needs more items to render
Func<VisualElement> makeItem = () => new Label();

// As the user scrolls through the list, the ListView object
// will recycle elements created by the "makeItem"
// and invoke the "bindItem" callback to associate
// the element with the matching data item (specified as an index in the list)
Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = items[i];

var listView = new ListView();
        this.Add(listView);
listView.makeItem = makeItem;
listView.bindItem = bindItem;
listView.itemsSource = items;
listView.selectionType = SelectionType.Multiple;

// Callback invoked when the user double clicks an item
listView.itemsChosen += (selectedItems) =>
{
    Debug.Log("Items chosen: " + string.Join(", ", selectedItems));
};

// Callback invoked when the user changes the selection inside the ListView
listView.selectionChanged += (selectedItems) =>
{
    Debug.Log("Items selected: " + string.Join(", ", selectedItems));
};
    }
}
