using UnityEngine;
using System;
using System.IO;
using System.Threading;

/// <summary>
/// JSON文件管理器 - 提供线程安全的JSON文件读写操作
/// 支持读取、修改和写入操作，确保数据一致性
/// </summary>
public class JsonFileManager
{
    private static readonly object _fileLock = new object();
    private static JsonFileManager _instance;
    
    public static JsonFileManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new JsonFileManager();
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// 获取完整的文件路径，如果是相对路径则加上Application.persistentDataPath前缀
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>完整的文件路径</returns>
    private string GetFullPath(string filePath)
    {
        // 如果是绝对路径（包含:或/开头），直接返回
        if (filePath.Contains(":") || filePath.StartsWith("/"))
        {
            return filePath;
        }
        
        // 如果是相对路径，加上Application.persistentDataPath前缀
        return Path.Combine(Application.persistentDataPath, filePath);
    }
    
    /// <summary>
    /// 读取JSON文件内容
    /// </summary>
    /// <typeparam name="T">目标数据类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <returns>读取到的数据，如果文件不存在或读取失败返回默认值</returns>
    public T ReadJson<T>(string filePath) where T : new()
    {
        string fullPath = GetFullPath(filePath);
        
        try
        {
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"JSON文件不存在: {fullPath}");
                return new T();
            }
            
            string jsonContent = File.ReadAllText(fullPath);
            T data = JsonUtility.FromJson<T>(jsonContent);
            Debug.Log($"成功读取JSON文件: {fullPath}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"读取JSON文件失败: {fullPath}, 错误: {e.Message}");
            return new T();
        }
    }
    
    /// <summary>
    /// 写入JSON文件内容
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="data">要写入的数据</param>
    /// <returns>是否写入成功</returns>
    public bool WriteJson<T>(string filePath, T data)
    {
        lock (_fileLock)
        {
            string fullPath = GetFullPath(filePath);
            
            try
            {
                string jsonContent = JsonUtility.ToJson(data, true);
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(fullPath, jsonContent);
                Debug.Log($"成功写入JSON文件: {fullPath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"写入JSON文件失败: {fullPath}, 错误: {e.Message}");
                return false;
            }
        }
    }
    

    
    /// <summary>
    /// 修改JSON文件内容（使用FromJsonOverwrite进行部分更新）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="partialData">包含要更新字段的部分数据对象</param>
    /// <returns>是否修改成功</returns>
    public bool ModifyJson<T>(string filePath, T partialData) where T : new()
    {
        lock (_fileLock)
        {
            try
            {
                // 读取现有数据
                T existingData = ReadJson<T>(filePath);
                
                // 使用FromJsonOverwrite只更新部分字段，保留原有数据
                string partialJson = JsonUtility.ToJson(partialData);
                JsonUtility.FromJsonOverwrite(partialJson, existingData);
                
                // 写回文件
                return WriteJson(filePath, existingData);
            }
            catch (Exception e)
            {
                Debug.LogError($"修改JSON文件失败: {filePath}, 错误: {e.Message}");
                return false;
            }
        }
    }
    
    /// <summary>
    /// 安全地添加数据到JSON文件中的列表
    /// </summary>
    /// <typeparam name="TList">列表类型</typeparam>
    /// <typeparam name="TItem">列表项类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="item">要添加的项</param>
    /// <param name="listPropertyName">列表属性名称</param>
    /// <returns>是否添加成功</returns>
    public bool AddItemToList<TList, TItem>(string filePath, TItem item, string listPropertyName) 
        where TList : new()
    {
        lock (_fileLock)
        {
            try
            {
                // 读取现有数据
                TList data = ReadJson<TList>(filePath);
                
                // 使用反射获取列表属性
                var property = typeof(TList).GetProperty(listPropertyName);
                if (property == null)
                {
                    Debug.LogError($"找不到属性: {listPropertyName}");
                    return false;
                }
                
                // 获取列表
                var list = property.GetValue(data) as System.Collections.Generic.List<TItem>;
                if (list == null)
                {
                    list = new System.Collections.Generic.List<TItem>();
                    property.SetValue(data, list);
                }
                
                // 添加项
                list.Add(item);
                
                // 写回文件
                return WriteJson(filePath, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"添加列表项失败: {filePath}, 错误: {e.Message}");
                return false;
            }
        }
    }
    
    /// <summary>
    /// 安全地清空JSON文件中的列表
    /// </summary>
    /// <typeparam name="TList">列表类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="listPropertyName">列表属性名称</param>
    /// <returns>是否清空成功</returns>
    public bool ClearList<TList>(string filePath, string listPropertyName) 
        where TList : new()
    {
        lock (_fileLock)
        {
            try
            {
                // 读取现有数据
                TList data = ReadJson<TList>(filePath);
                
                // 使用反射获取列表属性
                var property = typeof(TList).GetProperty(listPropertyName);
                if (property == null)
                {
                    Debug.LogError($"找不到属性: {listPropertyName}");
                    return false;
                }
                
                // 获取列表并清空
                var list = property.GetValue(data) as System.Collections.IList;
                if (list != null)
                {
                    list.Clear();
                }
                
                // 写回文件
                return WriteJson(filePath, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"清空列表失败: {filePath}, 错误: {e.Message}");
                return false;
            }
        }
    }
    
    /// <summary>
    /// 检查JSON文件是否存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件是否存在</returns>
    public bool FileExists(string filePath)
    {
        string fullPath = GetFullPath(filePath);
        return File.Exists(fullPath);
    }
    
    /// <summary>
    /// 删除JSON文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteFile(string filePath)
    {
        lock (_fileLock)
        {
            string fullPath = GetFullPath(filePath);
            
            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Debug.Log($"成功删除JSON文件: {fullPath}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"删除JSON文件失败: {fullPath}, 错误: {e.Message}");
                return false;
            }
        }
    }
    
    /// <summary>
    /// 获取JSON文件的最后修改时间
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>最后修改时间，如果文件不存在返回DateTime.MinValue</returns>
    public DateTime GetLastModifiedTime(string filePath)
    {
        string fullPath = GetFullPath(filePath);
        
        try
        {
            if (File.Exists(fullPath))
            {
                return File.GetLastWriteTime(fullPath);
            }
            return DateTime.MinValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"获取文件修改时间失败: {fullPath}, 错误: {e.Message}");
            return DateTime.MinValue;
        }
    }
    

}