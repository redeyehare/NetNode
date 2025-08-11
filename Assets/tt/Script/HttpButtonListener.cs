using UnityEngine;
using UnityEngine.UI;
using Best.HTTP;
using System;

public class HttpButtonListener : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Button requestButton;
    [SerializeField] private InputField urlInputField;
    [SerializeField] private Text responseText;
    [SerializeField] private Text statusText;

    [Header("设置")]
    [SerializeField] private float timeout = 10f;

    private void OnEnable()
    {
        if (requestButton != null)
        {
            requestButton.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("请求按钮未设置！");
        }

        if (urlInputField == null)
        {
            Debug.LogWarning("URL输入框未设置！");
        }

        if (responseText == null)
        {
            Debug.LogWarning("响应文本组件未设置！");
        }

        if (statusText == null)
        {
            Debug.LogWarning("状态文本组件未设置！");
        }
    }

    private void OnDisable()
    {
        if (requestButton != null)
        {
            requestButton.onClick.RemoveListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        if (string.IsNullOrEmpty(urlInputField?.text))
        {
            UpdateStatus("请输入有效的URL", Color.red);
            return;
        }

        string url = urlInputField.text.Trim();
        
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        UpdateStatus("正在发送请求...", Color.yellow);
        
        SendGetRequest(url);
    }

    private void SendGetRequest(string url)
    {
        try
        {
            var request = HTTPRequest.CreateGet(url, OnRequestComplete);
            request.Timeout = TimeSpan.FromSeconds(timeout);
            request.Send();
        }
        catch (Exception ex)
        {
            UpdateStatus($"请求创建失败: {ex.Message}", Color.red);
        }
    }

    private void OnRequestComplete(HTTPRequest request, HTTPResponse response)
    {
        if (response != null)
        {
            if (response.IsSuccess)
            {
                string responseContent = response.DataAsText;
                UpdateStatus($"请求成功 (状态码: {response.StatusCode})", Color.green);
                UpdateResponse(responseContent);
                
                Debug.Log($"GET请求完成:\nURL: {request.CurrentUri}\n状态码: {response.StatusCode}\n响应长度: {responseContent.Length}字符");
            }
            else
            {
                UpdateStatus($"请求失败 (状态码: {response.StatusCode})", Color.red);
                UpdateResponse(response.DataAsText);
            }
        }
        else
        {
            UpdateStatus($"请求失败: {request.State}", Color.red);
            UpdateResponse("无响应数据");
        }
    }

    private void UpdateStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        else
        {
            Debug.Log(message);
        }
    }

    private void UpdateResponse(string content)
    {
        if (responseText != null)
        {
            // 限制显示长度，避免过长
            string displayContent = content.Length > 1000 
                ? content.Substring(0, 1000) + "..." 
                : content;
            
            responseText.text = displayContent;
        }
        else
        {
            Debug.Log($"响应内容: {content}");
        }
    }

    // 公共方法供外部调用
    public void SetUrl(string url)
    {
        if (urlInputField != null)
        {
            urlInputField.text = url;
        }
    }

    public void ClearResponse()
    {
        if (responseText != null)
        {
            responseText.text = "";
        }
        
        if (statusText != null)
        {
            statusText.text = "就绪";
            statusText.color = Color.white;
        }
    }
}