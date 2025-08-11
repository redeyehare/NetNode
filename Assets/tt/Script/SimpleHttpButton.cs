using UnityEngine;
using UnityEngine.UI;
using Best.HTTP;

public class SimpleHttpButton : MonoBehaviour
{
    public Button requestButton;
    public InputField urlInput;
    public Text responseText;

    private void Start()
    {
        if (requestButton != null)
        {
            requestButton.onClick.AddListener(OnButtonClick);
        }
        
        // 设置默认URL
        if (urlInput != null && string.IsNullOrEmpty(urlInput.text))
        {
            urlInput.text = "https://httpbingo.org/get";
        }
    }

    private void OnButtonClick()
    {
        if (urlInput == null || string.IsNullOrEmpty(urlInput.text))
            return;

        string url = urlInput.text.Trim();
        if (!url.StartsWith("http"))
            url = "https://" + url;

        SendGetRequest(url);
    }

    private void SendGetRequest(string url)
    {
        var request = HTTPRequest.CreateGet(url, OnRequestComplete);
        request.Send();
    }

    private void OnRequestComplete(HTTPRequest request, HTTPResponse response)
    {
        if (response != null && response.IsSuccess)
        {
            string content = response.DataAsText;
            
            if (responseText != null)
            {
                responseText.text = content.Length > 500 
                    ? content.Substring(0, 500) + "..." 
                    : content;
            }
            
            Debug.Log($"GET成功: {url}\n响应: {content}");
        }
        else
        {
            Debug.LogError($"GET失败: {request.State}");
        }
    }
}