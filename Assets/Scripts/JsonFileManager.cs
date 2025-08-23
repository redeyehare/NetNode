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
    /// 读取JSON文件内容
    /// </summary>
    /// <typeparam name="T">目标数据类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <returns>读取到的数据，如果文件不存在或读取失败返回默认值</returns>
    public T ReadJson<T>(string filePath) where T : new()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"JSON文件不存在: {filePath}");
                return new T();
            }
            
            string jsonContent = File.ReadAllText(filePath);
            T data = JsonUtility.FromJson<T>(jsonContent);
            Debug.Log($"成功读取JSON文件: {filePath}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"读取JSON文件失败: {filePath}, 错误: {e.Message}");
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
            try
            {
                string jsonContent = JsonUtility.ToJson(data, true);
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(filePath, jsonContent);
                Debug.Log($"成功写入JSON文件: {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"写入JSON文件失败: {filePath}, 错误: {e.Message}");
                return false;
            }
        }
    }
    
    /// <summary>
    /// 修改JSON文件内容（读取-修改-写入的原子操作）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="modifyAction">修改数据的委托</param>
    /// <returns>是否修改成功</returns>
    public bool ModifyJson<T>(string filePath, Action<T> modifyAction) where T : new()
    {
        lock (_fileLock)
        {
            try
            {
                // 读取现有数据
                T data = ReadJson<T>(filePath);
                
                // 执行修改操作
                modifyAction(data);
                
                // 写回文件
                return WriteJson(filePath, data);
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
    /// <param name="getListFunc">获取列表的委托</param>
    /// <returns>是否添加成功</returns>
    public bool AddItemToList<TList, TItem>(string filePath, TItem item, Func<TList, System.Collections.Generic.List<TItem>> getListFunc) 
        where TList : new()
    {
        return ModifyJson<TList>(filePath, data => {
            var list = getListFunc(data);
            if (list == null)
            {
                list = new System.Collections.Generic.List<TItem>();
            }
            list.Add(item);
        });
    }
    
    /// <summary>
    /// 安全地清空JSON文件中的列表
    /// </summary>
    /// <typeparam name="TList">列表类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="getListFunc">获取列表的委托</param>
    /// <returns>是否清空成功</returns>
    public bool ClearList<TList>(string filePath, Func<TList, System.Collections.IList> getListFunc) 
        where TList : new()
    {
        return ModifyJson<TList>(filePath, data => {
            var list = getListFunc(data);
            if (list != null)
            {
                list.Clear();
            }
        });
    }
    
    /// <summary>
    /// 检查JSON文件是否存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件是否存在</returns>
    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
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
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"成功删除JSON文件: {filePath}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"删除JSON文件失败: {filePath}, 错误: {e.Message}");
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
        try
        {
            if (File.Exists(filePath))
            {
                return File.GetLastWriteTime(filePath);
            }
            return DateTime.MinValue;
        }
        catch (Exception e)
        {
            Debug.LogError($"获取文件修改时间失败: {filePath}, 错误: {e.Message}");
            return DateTime.MinValue;
        }
    }
    
    /// <summary>
    /// 部分更新JSON文件内容（使用FromJsonOverwrite，只修改指定字段）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <param name="partialData">包含要更新字段的部分数据对象</param>
    /// <returns>是否更新成功</returns>
    public bool UpdatePartialData<T>(string filePath, T partialData) where T : new()
    {
        lock (_fileLock)
        {
            try
            {
                // 读取现有数据
                T existingData = ReadJson<T>(filePath);
                
                // 使用FromJsonOverwrite只更新部分字段
                string partialJson = JsonUtility.ToJson(partialData);
                JsonUtility.FromJsonOverwrite(partialJson, existingData);
                
                // 写回文件
                return WriteJson(filePath, existingData);
            }
            catch (Exception e)
            {
                Debug.LogError($"部分更新JSON文件失败: {filePath}, 错误: {e.Message}");
                return false;
            }
        }
    }
}