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
        if (testsavejson != null)
        {
            testsavejson.onClick.AddListener(() => {
                AddContextData("第一条测试数据"); // 添加测试数据
                SaveJsonToFile(Application.persistentDataPath + "/saved_data.json");
            });
        }
        LoadJsonFromFile(Application.persistentDataPath + "/saved_data.json");
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
                if (loadedData != null && loadedData.root != null)
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

    void Update()
    {
        
    }
}