using UnityEngine;
using System.IO;
using Best.HTTP;

public class JsonLogger : MonoBehaviour
{
    // 记录JSON数据到本地
    public void LogJsonToLocal(string jsonContent, string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            File.WriteAllText(filePath, jsonContent);
            Debug.Log($"JSON data successfully logged to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to log JSON data to {filePath}: {e.Message}");
        }
    }

    // 发送POST请求携带JSON数据
    public void SendJsonViaPost(string url, string jsonContent)
    {
        HTTPRequest request = new HTTPRequest(new System.Uri(url), HTTPMethods.Post, OnPostRequestComplete);
        request.AddHeader("Content-Type", "application/json");
        request.UploadSettings.UploadStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        request.Send();
        Debug.Log($"Sending POST request to {url} with JSON data: {jsonContent}");
    }

    private void OnPostRequestComplete(HTTPRequest request, HTTPResponse response)
    {
        if (response != null && response.IsSuccess)
        {
            Debug.Log($"POST request successful! Response: {response.DataAsText}");
        }
        else
        {
            Debug.LogError($"POST request failed! Error: {request.Exception?.Message ?? response?.Message}");
        }
    }
}