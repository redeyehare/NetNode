using System;
using System.Collections.Generic;

[Serializable]
public class ItemData
{
    public string name;
    public int num;
    public List<string> list;
}

[Serializable]
public class RootData
{
    public List<ItemData> data;
}