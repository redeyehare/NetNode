using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Best.HTTP;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// JSON数据发送器类，用于管理JSON数据的收集、发送和保存
/// </summary>
public class JsonDataSender : MonoBehaviour
{
    /// <summary>
    /// 测试保存JSON按钮，用于触发保存操作
    /// </summary>
    public Button testsavejson;
    public Config config;
    
    /// <summary>
    /// POST请求的URL地址，默认为httpbingo.org/post
    /// </summary>
    public string postUrl = "https://httpbingo.org/post"; // 默认POST请求URL

    /// <summary>
    /// 私有数据对象，存储所有要发送的JSON数据
    /// </summary>
    private JsonSenderData _data = new JsonSenderData();

    /// <summary>
    /// 添加上下文数据到数据列表中
    /// </summary>
    /// <param name="context">要添加的上下文内容字符串</param>
    public void AddContextData(string context)
    {
        _data.root.Add(new ContextData { context = context });
        Debug.Log($"Added context: {context}. Current data count: {_data.root.Count}");
    }

    /// <summary>
    /// 触发发送当前JSON数据
    /// </summary>
    public void TriggerSend()
    {
        SendCurrentJsonData();
    }

    /// <summary>
    /// 定期发送JSON数据的协程，每10秒发送一次
    /// </summary>
    /// <returns>协程迭代器</returns>
    private IEnumerator SendJsonPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            SendCurrentJsonData();
        }
    }

    /// <summary>
    /// 发送当前JSON数据到指定的POST URL
    /// </summary>
    private void SendCurrentJsonData()
    {
        if (_data.root.Count > 0)
        {
            string jsonToSend = JsonUtility.ToJson(_data);
            Debug.Log($"Sending JSON: {jsonToSend}");
            SendJsonViaPost(postUrl, jsonToSend);
        }
        else
        {
            Debug.Log("No data to send.");
        }
    }

    /// <summary>
    /// 通过POST请求发送JSON数据到指定URL
    /// </summary>
    /// <param name="url">目标URL地址</param>
    /// <param name="jsonContent">要发送的JSON内容字符串</param>
    public void SendJsonViaPost(string url, string jsonContent)
    {
        HTTPRequest request = new HTTPRequest(new System.Uri(url), HTTPMethods.Post, OnPostRequestComplete);
        request.AddHeader("Content-Type", "application/json");
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        // 设置连接超时为10秒，总超时为30秒，防止HTTP2 Ping超时
        request.TimeoutSettings.ConnectTimeout = TimeSpan.FromSeconds(10);
        request.TimeoutSettings.Timeout = TimeSpan.FromSeconds(30);
        request.Send();
        Debug.Log($"Sending POST request to {url} with JSON data (truncated for brevity)..");
    }

    /// <summary>
    /// POST请求完成后的回调函数
    /// </summary>
    /// <param name="request">HTTP请求对象</param>
    /// <param name="response">HTTP响应对象</param>
    private void OnPostRequestComplete(HTTPRequest request, HTTPResponse response)
    {
        if (response != null && response.IsSuccess)
        {
            Debug.Log($"POST request successful! Response (truncated for brevity)..");
            _data.root.Clear(); // Clear data after successful sending
            SaveJsonToFile(Application.persistentDataPath + "/saved_data.json"); // Save the cleared state
        }
        else
        {
            Debug.LogError($"POST request failed! Error: {request.Exception?.Message ?? response?.Message}");
        }
    }

    /// <summary>
    /// Unity Start方法，在游戏对象初始化时调用
    /// </summary>
    void Start()
    {
        // 加载已保存的JSON数据
        LoadJsonFromFile(Application.persistentDataPath + "/saved_data.json");

        // 获取当前日期字符串
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        Debug.Log($"系统当前日期: {today}");

        if (testsavejson != null)
        {
            // 使用封装的 JsonFileManager 读取 JSON 文件检测 mark 日期是否与当前日期一致
            AppConfig appData = JsonFileManager.Instance.ReadJson<AppConfig>(config.configJsPath);
            
            // 如果 mark 日期与当前日期不一致，则按钮可交互
            testsavejson.interactable = appData.mark != today;
            Debug.Log($"当前日期: {today}, JSON中的mark日期: {appData.mark}, 按钮交互状态: {testsavejson.interactable}");
            
            // 为按钮添加点击事件监听器
            testsavejson.onClick.AddListener(() => {
                // 获取 PhoneNumberManager 实例并调用 SavePhoneNumber 方法
                PhoneNumberManager phoneNumberManager = FindObjectOfType<PhoneNumberManager>();
                if (phoneNumberManager != null)
                {
                    // 假设你需要传递一个手机号码来更新mark，这里使用一个占位符或者从其他地方获取
                    // 如果 SavePhoneNumber 只需要更新 mark，可以考虑修改其签名或传递空字符串
                    phoneNumberManager.SavePhoneNumber(phoneNumberManager.phoneInputField.text); 
                    Debug.Log($"Mark数据已更新到PhoneNumberManager的JSON文件。");
                    
                    // 立即重新检测mark日期并更新按钮状态
                    string today = System.DateTime.Now.ToString("yyyy-MM-dd");
                    AppConfig appData = JsonFileManager.Instance.ReadJson<AppConfig>("appData.json");
                    // 如果 mark 日期与当前日期不一致，则按钮可交互
                    testsavejson.interactable = appData.mark != today;
                    Debug.Log($"按钮点击后重新检测 - 当前日期: {today}, JSON中的mark日期: {appData.mark}, 按钮交互状态: {testsavejson.interactable}");
                }
                else
                {
                    Debug.LogError("无法找到 PhoneNumberManager 实例。");
                }
            });
        }
        // 启动定期发送JSON数据的协程
        StartCoroutine(SendJsonPeriodically());
    }

    /// <summary>
    /// 将当前JSON数据保存到指定文件路径
    /// </summary>
    /// <param name="filePath">要保存的文件路径</param>
    public void SaveJsonToFile(string filePath)
    {
        string jsonToSave = JsonUtility.ToJson(_data);
        try
        {
            File.WriteAllText(filePath, jsonToSave);
            Debug.Log($"JSON data saved to: {filePath}. Current data count: {_data.root.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save JSON data to {filePath}: {e.Message}");
        }
    }

    /// <summary>
    /// 从指定文件路径加载JSON数据
    /// </summary>
    /// <param name="filePath">要加载的文件路径</param>
    private void LoadJsonFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                string jsonLoaded = File.ReadAllText(filePath);
                JsonSenderData loadedData = JsonUtility.FromJson<JsonSenderData>(jsonLoaded);
                if (loadedData != null)
                {
                    _data.root.Clear(); // Clear existing data before loading
                    _data.root.AddRange(loadedData.root); // Add loaded data
                    Debug.Log($"JSON data loaded from: {filePath}. Total items: {_data.root.Count}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load JSON data from {filePath}: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"No saved data found at: {filePath}");
        }
    }


}