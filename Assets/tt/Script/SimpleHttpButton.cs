using UnityEngine;
using UnityEngine.UI;
using Best.HTTP;
using TMPro;
using System.Collections.Generic;

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

    private void CopyToClipboard(string text)
    {
        TextEditor textEditor = new TextEditor();
        textEditor.text = text;
        textEditor.SelectAll();
        textEditor.Copy();
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
                                        string currentGroupName = itemData.name;
                                        int currentItemNumber = i + 1;
                                        itemButton.onClick.RemoveAllListeners(); // 移除所有旧的监听器
                                        itemButton.onClick.AddListener(() => {
                                            Debug.Log($"Button clicked! Group: {currentGroupName}, Number: {currentItemNumber}");
                                            if (itemData.list != null && currentItemNumber - 1 < itemData.list.Count)
                                            {
                                                CopyToClipboard(itemData.list[currentItemNumber - 1]);
                                                Debug.Log($"Copied to clipboard: {itemData.list[currentItemNumber - 1]}");
                                            }
                                            else
                                            {
                                                Debug.LogWarning($"List item not found for Group: {currentGroupName}, Number: {currentItemNumber}");
                                            }
                                        });
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

