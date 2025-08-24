using UnityEngine;
using UnityEngine.UI;
using Best.HTTP;
using TMPro;
using System.Collections.Generic;
using System;

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
    public List<Transform> Folder;

    private Transform singbox;
    [SerializeField]
    public GameObject item;
    [SerializeField]
    //测试按钮
    public Button test;
    private JsonLogger jsonLogger;

    public TextMeshProUGUI statusText;

    public Config config;


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

        jsonLogger = FindObjectOfType<JsonLogger>();
        if (jsonLogger == null)
        {
            GameObject loggerObject = new GameObject("JsonLogger");
            jsonLogger = loggerObject.AddComponent<JsonLogger>();
        }

        if (test != null)
        {
            test.onClick.AddListener(OnTestButtonClick);
        }
        // 启动时自动执行一次请求
        OnButtonClick();
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
        
        // 禁用HTTP/2，强制使用HTTP/1.1避免Ping超时问题
        var uri = new System.Uri(url);
        var hostSettings = Best.HTTP.Shared.HTTPManager.PerHostSettings.Get(uri.Host);
        hostSettings.HTTP2ConnectionSettings.EnableHTTP2Connections = false;
        
        // 调整超时设置
        request.TimeoutSettings.ConnectTimeout = TimeSpan.FromSeconds(15);
        request.TimeoutSettings.Timeout = TimeSpan.FromSeconds(45);
        request.Send();
    }

    private void CopyToClipboard(string text)
    {
        TextEditor textEditor = new TextEditor();
        textEditor.text = text;
        textEditor.SelectAll();
        textEditor.Copy();
    }

    /// <summary>
    /// 设置按钮点击事件处理器
    /// </summary>
    /// <param name="itemButton">要设置点击事件的按钮组件</param>
    /// <param name="itemData">包含列表数据的ItemData对象</param>
    /// <param name="groupName">按钮所属的组名称</param>
    /// <param name="itemNumber">按钮在组中的编号</param>
    private void SetupButtonClickHandler(Button itemButton, ItemData itemData, string groupName, int itemNumber)
    {
        if (itemButton == null) return;
        
        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() => {
            // 检查支付状态
            if (config != null && !string.IsNullOrEmpty(config.configJsPath))
            {
                string configData = JsonFileManager.Instance.ReadJson<AppConfig>(config.configJsPath).mark;
                string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");
                // 检查configJsPath是否等于当前日期
                if (configData != currentDate)
                {
                    if (statusText != null)
                    {
                        statusText.text = "请支付";
                    }
                    return;
                }
            }
            
            //Debug.Log($"Button clicked! Group: {groupName}, Number: {itemNumber}");
            if (itemData.list != null && itemNumber - 1 < itemData.list.Count)
            {
                CopyToClipboard(itemData.list[itemNumber - 1]);
                Debug.Log($"Copied to clipboard: {itemData.list[itemNumber - 1]}");
            }
            else
            {
                Debug.LogWarning($"List item not found for Group: {groupName}, Number: {itemNumber}");
            }
        });
    }

    /// <summary>
    /// 测试按钮点击事件处理
    /// </summary>
    private void OnTestButtonClick()
    {
        if (urlInput == null || string.IsNullOrEmpty(urlInput.text))
            return;

        string url = urlInput.text.Trim();
        if (!url.StartsWith("http"))
            url = "https://" + url;

        // 测试请求使用固定的测试URL
        string testUrl = "https://httpbingo.org/get";
        SendGetRequest(testUrl);
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

            // 尝试解析JSON数据
            try
            {
                // 由于JsonUtility.FromJson要求JSON根是一个对象，所以需要包装一下
                string wrappedContent = "{\"data\":" + content + "}";
                RootData rootData = JsonUtility.FromJson<RootData>(wrappedContent);

                if (rootData != null && rootData.data != null)
                {
                    foreach (var itemData in rootData.data)
                    {
                        foreach (var folderTransform in Folder)
                        {
                            if (folderTransform != null && folderTransform.name == itemData.name)
                            {
                                Debug.Log($"匹配到文件夹: {itemData.name}");
                                // 在匹配的Transform下实例化item
                                if (item != null)
                                {
                                    for (int i = 0; i < itemData.num; i++)
                                    {
                                        var newItem = Instantiate(item, folderTransform);
                                    // 获取Button组件并添加点击事件监听
                                    Button itemButton = newItem.GetComponentInChildren<Button>();
                                    if (itemButton != null)
                                    {
                                        SetupButtonClickHandler(itemButton, itemData, itemData.name, i + 1);
                                    }
                                    }
                                    // 在当前folderTransform下的所有item实例化完成后，强制更新布局
                                    LayoutRebuilder.ForceRebuildLayoutImmediate(folderTransform as RectTransform);
                                }
                                break;
                            }
                        }
                    }
                }
                // 在所有item实例化完成后，统一强制更新布局
                Canvas.ForceUpdateCanvases();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON解析失败: {e.Message}");
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

