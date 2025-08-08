using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

// 定义单个订单数据结构
[Serializable]
public class OrderData
{
    public string orderId;
    public string ipAddress; // 客户端IP地址，通常由服务器获取或通过外部服务获取
    public string userId;
    public string macAddress; // 注意：Unity中直接获取MAC地址通常不推荐且困难，这里用DeviceId代替
    public string date;
    public string time;
}

// 定义订单历史列表，用于JSON序列化
[Serializable]
public class PurchaseHistory
{
    public List<OrderData> purchasehistory = new List<OrderData>();
}

public class PurchaseHistoryManager
{
    private static PurchaseHistoryManager _instance;
    private static readonly object _lock = new object();
    
    private string serverUrl = "http://your-server-endpoint.com/upload";
    private int retryInterval = 30000; // 30秒
    private string localDataFileName = "purchase_history.json";
    private string localDataPath;
    private PurchaseHistory currentHistory = new PurchaseHistory();
    private Timer sendTimer;
    
    private PurchaseHistoryManager()
    {
        localDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, localDataFileName);
        LoadLocalHistory();
        
        // 启动定时器，每30秒检查并发送数据
        sendTimer = new Timer(SendPurchaseHistoryCallback, null, retryInterval, retryInterval);
    }
    
    public static PurchaseHistoryManager Instance
    {
        get
        {C
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new PurchaseHistoryManager();
                }
                return _instance;
            }
        }
    }

    // 添加新的订单数据到本地记录
    public void AddPurchase(string orderId, string userId)
    {
        OrderData newOrder = new OrderData
        {
            orderId = orderId,
            // 注意：获取客户端IP地址在Unity中通常需要外部服务或服务器端获取
            // 这里暂时留空或使用占位符
            ipAddress = "", 
            userId = userId,
            macAddress = SystemInfo.deviceUniqueIdentifier, // 使用设备唯一标识符作为替代
            date = DateTime.Now.ToString("yyyy-MM-dd"),
            time = DateTime.Now.ToString("HH:mm:ss")
        };

        currentHistory.purchasehistory.Add(newOrder);
        SaveLocalHistory();
        Debug.Log($"订单 {orderId} 已添加到本地记录。");


    }

    // 加载本地订单历史
    private void LoadLocalHistory()
    {
        if (File.Exists(localDataPath))
        {
            string json = File.ReadAllText(localDataPath);
            currentHistory = JsonUtility.FromJson<PurchaseHistory>(json);
            Debug.Log($"从 {localDataPath} 加载了 {currentHistory.purchasehistory.Count} 条订单记录。");
        }
        else
        {
            currentHistory = new PurchaseHistory();
            Debug.Log("本地订单历史文件不存在，创建新的历史记录。");
        }
    }

    // 保存本地订单历史
    private void SaveLocalHistory()
    {
        string json = JsonUtility.ToJson(currentHistory, true);
        File.WriteAllText(localDataPath, json);
        Debug.Log($"订单历史已保存到 {localDataPath}。当前记录数: {currentHistory.purchasehistory.Count}");
    }

    // 清空本地订单历史
    private void ClearLocalHistory()
    {
        currentHistory.purchasehistory.Clear();
        SaveLocalHistory();
        Debug.Log("本地订单历史已清空。");
    }

    // 发送订单历史的协程
    private IEnumerator SendPurchaseHistoryCoroutine()
    {
        while (true)
        {
            // 如果当前没有待发送的订单，则等待一段时间再检查
            if (currentHistory.purchasehistory.Count == 0)
            {
                Debug.Log("没有待发送的订单数据，等待新数据...");
                yield return new WaitForSeconds(retryInterval); // 没有数据时也等待，避免无限循环占用CPU
                continue;
            }

            string jsonToSend = JsonUtility.ToJson(currentHistory, true);
            Debug.Log($"准备发送 {currentHistory.purchasehistory.Count} 条订单数据到服务器:\n{jsonToSend}");

            using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonToSend);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("订单数据发送成功！服务器响应: " + request.downloadHandler.text);
                    ClearLocalHistory(); // 发送成功后删除本地数据
                }
                else
                {
                    Debug.LogError($"订单数据发送失败: {request.error}. 将在 {retryInterval} 秒后重试...");
                    yield return new WaitForSeconds(retryInterval); // 失败后等待重试
                }
            }
            yield return null; // 每帧检查一次，避免阻塞
        }
    }

        string jsonToSend = JsonUtility.ToJson(currentHistory, true);
        Debug.Log($"准备发送 {currentHistory.purchasehistory.Count} 条订单数据到服务器:\n{jsonToSend}");

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonToSend);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("订单数据发送成功！服务器响应: " + request.downloadHandler.text);
                ClearLocalHistory(); // 发送成功后删除本地数据
            }
            else
            {
                Debug.LogError($"订单数据发送失败: {request.error}. 将在 {retryInterval} 秒后重试...");
                yield return new WaitForSeconds(retryInterval);
                StartCoroutine(SendPurchaseHistoryCoroutine()); // 失败后重试
            }
        }
    }

    // 外部调用示例
    // public void TestAddAndSend()
    // {
    //     AddPurchase("ORDER_123", "USER_ABC");
    //     AddPurchase("ORDER_456", "USER_DEF");
    // }
}