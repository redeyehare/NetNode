using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Best.HTTP;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class JsonDataSender : MonoBehaviour
{
    public Button testsavejson;
    [System.Serializable]
    public class ContextData
    {
        public string context;
    }

    [System.Serializable]
    public class RootData
    {
        public List<ContextData> root = new List<ContextData>();
    }
    public string postUrl = "https://httpbingo.org/post"; // 默认POST请求URL

    private RootData _data = new RootData();

    public void AddContextData(string context)
    {
        _data.root.Add(new ContextData { context = context });
        Debug.Log($"Added context: {context}. Current data count: {_data.root.Count}");
    }

    public void TriggerSend()
    {
        SendCurrentJsonData();
    }

    private IEnumerator SendJsonPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            SendCurrentJsonData();
        }
    }

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

    public void SendJsonViaPost(string url, string jsonContent)
    {
        HTTPRequest request = new HTTPRequest(new System.Uri(url), HTTPMethods.Post, OnPostRequestComplete);
        request.AddHeader("Content-Type", "application/json");
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        request.Send();
        Debug.Log($"Sending POST request to {url} with JSON data (truncated for brevity)..");
    }

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

    void Start()
    {
        LoadJsonFromFile(Application.persistentDataPath + "/saved_data.json");

        // 获取当前日期字符串
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        Debug.Log($"系统当前日期: {today}");

        if (testsavejson != null)
        {
            // 直接读取 JSON 文件检测 mark 日期是否与当前日期一致
            string appDataPath = Path.Combine(Application.persistentDataPath, "appData.json");
            if (File.Exists(appDataPath))
            {
                try
                {
                    string jsonContent = File.ReadAllText(appDataPath);
                    Debug.Log($"读取到的 JSON 内容: {jsonContent}");
                    // 使用 JsonUtility 解析 JSON 获取 mark 字段
                    PhoneNumberManager.AppData appData = JsonUtility.FromJson<PhoneNumberManager.AppData>(jsonContent);
                    // 如果 mark 日期与当前日期一致，则按钮不可交互
                    testsavejson.interactable = (appData.mark != today);
                    Debug.Log($"当前日期: {today}, JSON中的mark日期: {appData.mark}, 按钮交互状态: {testsavejson.interactable}");
                }
                catch (System.Exception e)
                {
                    testsavejson.interactable = true;
                    Debug.LogWarning($"读取或解析 JSON 文件失败: {e.Message}");
                }
            }
            else
            {
                testsavejson.interactable = true;
                Debug.LogWarning("appData.json 文件不存在");
            }
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
                    string appDataPath = Path.Combine(Application.persistentDataPath, "appData.json");
                    if (File.Exists(appDataPath))
                    {
                        try
                        {
                            string jsonContent = File.ReadAllText(appDataPath);
                            PhoneNumberManager.AppData appData = JsonUtility.FromJson<PhoneNumberManager.AppData>(jsonContent);
                            testsavejson.interactable = (appData.mark != today);
                            Debug.Log($"按钮点击后重新检测 - 当前日期: {today}, JSON中的mark日期: {appData.mark}, 按钮交互状态: {testsavejson.interactable}");
                        }
                        catch (System.Exception e)
                        {
                            testsavejson.interactable = false;
                            Debug.LogWarning($"按钮点击后读取或解析 JSON 文件失败: {e.Message}");
                        }
                    }
                    else
                    {
                        testsavejson.interactable = false;
                        Debug.LogWarning("按钮点击后appData.json 文件不存在");
                    }
                }
                else
                {
                    Debug.LogError("无法找到 PhoneNumberManager 实例。");
                }
            });
        }
        StartCoroutine(SendJsonPeriodically());
    }

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

    private void LoadJsonFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                string jsonLoaded = File.ReadAllText(filePath);
                RootData loadedData = JsonUtility.FromJson<RootData>(jsonLoaded);
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