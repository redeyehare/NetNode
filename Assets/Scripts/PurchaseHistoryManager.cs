using UnityEngine;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;

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
    public string serverUrl = "http://your-server-endpoint.com/upload"; // 替换为你的服务器API地址
    public float retryInterval = 30f; // 重试间隔30秒
    public string localDataFileName = "purchase_history.json"; // 本地数据文件名

    private PurchaseHistory currentHistory = new PurchaseHistory();
    private string localDataPath;
    private Timer sendTimer;

    private PurchaseHistoryManager()
    {
        localDataPath = Path.Combine(Application.persistentDataPath, localDataFileName);
        LoadLocalHistory();
        
        // 启动定时器，每30秒检查并发送数据
        sendTimer = new Timer(SendPurchaseHistoryCallback, null, TimeSpan.FromSeconds(retryInterval), TimeSpan.FromSeconds(retryInterval));
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
            macAddress = SystemInfo.deviceUniqueIdentifier, // 使用设备唯一标识符
            date = DateTime.Now.ToString("yyyy-MM-dd"),
            time = DateTime.Now.ToString("HH:mm:ss")
        };

        lock (_lock)
        {
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

    // 定时器回调方法
    private void SendPurchaseHistoryCallback(object state)
    {
        lock (_lock)
        {
            if (currentHistory.purchasehistory.Count == 0)
            {
                Debug.Log("没有待发送的订单数据，等待新数据...");
                return;
            }
                
            string jsonToSend = JsonUtility.ToJson(currentHistory, true);
            Debug.Log($"准备发送 {currentHistory.purchasehistory.Count} 条订单数据到服务器:\n{jsonToSend}");

            // 由于UnityWebRequest必须在主线程中调用，这里需要一个机制来调度到主线程
            // 暂时只打印日志，实际发送逻辑需要通过UnityMainThreadDispatcher或其他方式实现
            // 或者将发送逻辑完全剥离到MonoBehaviour中
            // 为了满足用户需求，我将创建一个新的MonoBehaviour来处理发送
            // 并在这里调用它的发送方法
            PurchaseHistorySender.Instance.EnqueueData(jsonToSend, serverUrl, () => {
                // 发送成功后的回调
                ClearLocalHistory();
            });
        }
    }