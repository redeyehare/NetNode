using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// JsonFileManager基础测试类
/// 用于验证JSON文件管理器的基本功能
/// </summary>
public class JsonFileManagerTest : MonoBehaviour
{
    // 测试数据结构
    [System.Serializable]
    public class TestData
    {
        public string name;
        public int value;
        public List<string> items = new List<string>();
    }
    
    private string testFilePath;
    
    void Start()
    {
        testFilePath = Path.Combine(Application.persistentDataPath, "test_data.json");
        Debug.Log($"测试文件路径: {testFilePath}");
        
        // 运行基础测试
        RunBasicTests();
    }
    
    /// <summary>
    /// 运行基础功能测试
    /// </summary>
    public void RunBasicTests()
    {
        Debug.Log("=== 开始JsonFileManager基础测试 ===");
        
        // 测试1: 写入数据
        TestWriteData();
        
        // 测试2: 读取数据
        TestReadData();
        
        // 测试3: 修改数据
        TestModifyData();
        
        // 测试4: 列表操作
        TestListOperations();
        
        // 测试5: 文件操作
        TestFileOperations();
        
        Debug.Log("=== JsonFileManager基础测试完成 ===");
    }
    
    private void TestWriteData()
    {
        Debug.Log("\n--- 测试1: 写入数据 ---");
        
        TestData data = new TestData
        {
            name = "测试数据",
            value = 100,
            items = new List<string> { "项目1", "项目2" }
        };
        
        bool success = JsonFileManager.Instance.WriteJson(testFilePath, data);
        Debug.Log($"写入结果: {(success ? "成功" : "失败")}");
    }
    
    private void TestReadData()
    {
        Debug.Log("\n--- 测试2: 读取数据 ---");
        
        TestData data = JsonFileManager.Instance.ReadJson<TestData>(testFilePath);
        Debug.Log($"读取结果: 名称={data.name}, 值={data.value}, 项目数量={data.items.Count}");
    }
    
    private void TestModifyData()
    {
        Debug.Log("\n--- 测试3: 修改数据 ---");
        
        bool success = JsonFileManager.Instance.ModifyJson<TestData>(testFilePath, data => {
            data.value += 50;
            data.name = "修改后的数据";
            Debug.Log("在修改回调中更新了数据");
        });
        
        Debug.Log($"修改结果: {(success ? "成功" : "失败")}");
        
        // 验证修改结果
        TestData modifiedData = JsonFileManager.Instance.ReadJson<TestData>(testFilePath);
        Debug.Log($"修改后数据: 名称={modifiedData.name}, 值={modifiedData.value}");
    }
    
    private void TestListOperations()
    {
        Debug.Log("\n--- 测试4: 列表操作 ---");
        
        // 添加新项目
        bool addSuccess = JsonFileManager.Instance.AddItemToList<TestData, string>(testFilePath, "新项目", data => data.items);
        Debug.Log($"添加项目结果: {(addSuccess ? "成功" : "失败")}");
        
        // 验证添加结果
        TestData dataWithNewItem = JsonFileManager.Instance.ReadJson<TestData>(testFilePath);
        Debug.Log($"添加后项目数量: {dataWithNewItem.items.Count}");
        foreach (string item in dataWithNewItem.items)
        {
            Debug.Log($"  - {item}");
        }
        
        // 清空列表
        bool clearSuccess = JsonFileManager.Instance.ClearList<TestData>(testFilePath, data => data.items);
        Debug.Log($"清空列表结果: {(clearSuccess ? "成功" : "失败")}");
        
        // 验证清空结果
        TestData dataWithClearedList = JsonFileManager.Instance.ReadJson<TestData>(testFilePath);
        Debug.Log($"清空后项目数量: {dataWithClearedList.items.Count}");
    }
    
    private void TestFileOperations()
    {
        Debug.Log("\n--- 测试5: 文件操作 ---");
        
        // 检查文件是否存在
        bool exists = JsonFileManager.Instance.FileExists(testFilePath);
        Debug.Log($"文件存在: {exists}");
        
        // 获取修改时间
        DateTime modifiedTime = JsonFileManager.Instance.GetLastModifiedTime(testFilePath);
        Debug.Log($"文件修改时间: {modifiedTime}");
        
        // 删除文件
        bool deleteSuccess = JsonFileManager.Instance.DeleteFile(testFilePath);
        Debug.Log($"删除文件结果: {(deleteSuccess ? "成功" : "失败")}");
        
        // 验证删除结果
        bool stillExists = JsonFileManager.Instance.FileExists(testFilePath);
        Debug.Log($"删除后文件存在: {stillExists}");
    }
    
    /// <summary>
    /// 在Unity编辑器中测试的按钮方法
    /// </summary>
    [ContextMenu("运行JsonFileManager测试")]
    public void RunTestFromEditor()
    {
        RunBasicTests();
    }
}