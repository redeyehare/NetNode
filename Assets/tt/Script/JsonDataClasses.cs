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

[Serializable]
public class AppConfig
{
    public string phoneNumber;
    public string uuid;
    public string mark = "";
}

/// <summary>
/// 上下文数据类，用于存储单个上下文信息
/// </summary>
[Serializable]
public class ContextData
{
    /// <summary>
    /// 上下文内容字符串
    /// </summary>
    public string context;
}

/// <summary>
/// JSON发送器的数据类，包含所有上下文数据的列表
/// </summary>
[Serializable]
public class JsonSenderData
{
    /// <summary>
    /// 上下文数据列表
    /// </summary>
    public List<ContextData> root = new List<ContextData>();
}