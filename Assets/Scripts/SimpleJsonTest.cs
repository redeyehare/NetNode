using UnityEngine;

/// <summary>
/// 简单的JsonFileManager测试脚本
/// 专门测试UpdatePartialData功能
/// </summary>
public class SimpleJsonTest : MonoBehaviour
{
    [System.Serializable]
    public class TestData
    {
        public string name;
        public int level;
        public float score;
        public bool isActive;
    }

    void Start()
    {
        TestPartialUpdate();
    }

    [ContextMenu("测试部分更新")]
    public void TestPartialUpdate()
    {
        string filePath = Application.persistentDataPath + "/test_data.json";
        
        // 1. 创建初始数据
        TestData originalData = new TestData
        {
            name = "测试玩家",
            level = 1,
            score = 100f,
            isActive = true
        };
        
        Debug.Log("=== 开始测试部分更新功能 ===");
        
        // 2. 保存初始数据
        bool saveResult = JsonFileManager.Instance.WriteJson(filePath, originalData);
        Debug.Log($"保存初始数据: {saveResult}");
        
        // 3. 读取并显示初始数据
        TestData currentData = JsonFileManager.Instance.ReadJson<TestData>(filePath);
        Debug.Log($"初始数据 - 名称: {currentData.name}, 等级: {currentData.level}, 分数: {currentData.score}, 活跃: {currentData.isActive}");
        
        // 4. 创建部分更新数据（只更新level和score）
        TestData partialUpdate = new TestData
        {
            level = 5,    // 只更新等级
            score = 500f  // 只更新分数
            // name和isActive保持默认值，不会被更新
        };
        
        // 5. 执行部分更新
        bool updateResult = JsonFileManager.Instance.UpdatePartialData(filePath, partialUpdate);
        Debug.Log($"部分更新结果: {updateResult}");
        
        // 6. 读取并验证更新结果
        TestData updatedData = JsonFileManager.Instance.ReadJson<TestData>(filePath);
        Debug.Log($"更新后数据 - 名称: {updatedData.name}, 等级: {updatedData.level}, 分数: {updatedData.score}, 活跃: {updatedData.isActive}");
        
        // 7. 验证哪些字段被更新了
        bool nameUnchanged = updatedData.name == "测试玩家";
        bool levelUpdated = updatedData.level == 5;
        bool scoreUpdated = updatedData.score == 500f;
        bool activeUnchanged = updatedData.isActive == true;
        
        Debug.Log("=== 验证结果 ===");
        Debug.Log($"名称保持不变: {nameUnchanged}");
        Debug.Log($"等级已更新: {levelUpdated}");
        Debug.Log($"分数已更新: {scoreUpdated}");
        Debug.Log($"活跃状态保持不变: {activeUnchanged}");
        
        if (nameUnchanged && levelUpdated && scoreUpdated && activeUnchanged)
        {
            Debug.Log("✅ 部分更新功能测试通过！");
        }
        else
        {
            Debug.LogError("❌ 部分更新功能测试失败！");
        }
        
        Debug.Log("=== 测试结束 ===");
    }
}