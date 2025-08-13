using UnityEngine;
using UnityEngine.UI;
using Best.HTTP;
using TMPro;

public class SimpleHttpButton : MonoBehaviour
{
    public Button requestButton;
    public TMP_InputField urlInput;
    public Text responseText;
    [SerializeField]
    private Transform v2ray;
    [SerializeField]
    private Transform clash;
    [SerializeField]
    private Transform singbox;
[SerializeField]
    public GameObject item;

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
        request.Tag = url; // 将url存储在Tag中
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
            
            // 从request.Tag中获取url
            string requestUrl = request.Tag as string;
            Debug.Log($"GET成功: {requestUrl}\n响应: {content}");
        }
        else
        {
            Debug.LogError($"GET失败: {request.State}");
        }
    }
}