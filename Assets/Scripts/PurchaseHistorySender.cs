using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class PurchaseHistorySender : MonoBehaviour
{
    public static PurchaseHistorySender Instance { get; private set; }

    private Queue<(string json, string url, Action onSuccess)> _sendQueue = new Queue<(string, string, Action)>();
    private bool _isSending = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void EnqueueData(string json, string url, Action onSuccess)
    {
        lock (_sendQueue)
        {
            _sendQueue.Enqueue((json, url, onSuccess));
        }
        if (!_isSending)
        {
            StartCoroutine(SendQueueCoroutine());
        }
    }

    private IEnumerator SendQueueCoroutine()
    {
        _isSending = true;
        while (_sendQueue.Count > 0)
        {
            (string json, string url, Action onSuccess) dataToSend;
            lock (_sendQueue)
            {
                dataToSend = _sendQueue.Dequeue();
            }

            using (UnityWebRequest request = new UnityWebRequest(dataToSend.url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(dataToSend.json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("订单数据发送成功！服务器响应: " + request.downloadHandler.text);
                    dataToSend.onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"订单数据发送失败: {request.error}");
                    // Optionally re-enqueue for retry or handle failure
                }
            }
        }
        _isSending = false;
    }
}