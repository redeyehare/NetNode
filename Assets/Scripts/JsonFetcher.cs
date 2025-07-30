using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class JsonFetcher : MonoBehaviour
{
    public string url = "https://raw.githubusercontent.com/redeyehare/test6/refs/heads/main/test.json";

    // 定义一个内部类来匹配JSON结构
    [System.Serializable]
    public class MyJsonData
    {
        // 注意：JsonUtility 无法直接解析以数字作为键的JSON（如 {"123":"value"}）。
        // 如果您的JSON键是数字，您需要将其改为字符串（如 {"key123":"value"}），
        // 或者使用第三方JSON库（如 Newtonsoft.Json）。
        // 这里假设您的JSON键是合法的C#标识符，例如 "key"
        public string key; // 假设JSON是 {"key":"value"}
    }

    void Start()
    {
        StartCoroutine(GetJsonData());
    }

    IEnumerator GetJsonData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string jsonText = webRequest.downloadHandler.text;
                Debug.Log("Received JSON: " + jsonText);

                // 尝试移除前缀 "JSON: "
                if (jsonText.StartsWith("JSON: "))
                {
                    jsonText = jsonText.Substring("JSON: ".Length);
                }

                try
                {
                    // 尝试解析JSON
                    MyJsonData data = JsonUtility.FromJson<MyJsonData>(jsonText);
                    Debug.Log("Parsed JSON 'key' value: " + data.key);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to parse JSON: " + e.Message + "\nJSON string: " + jsonText);
                    Debug.LogError("Hint: Unity's JsonUtility cannot parse JSON with numeric keys directly (e.g., {\"123\":\"value\"}). Please ensure your JSON keys are valid C# identifiers (e.g., {\"myKey\":\"value\"}) or consider using a third-party JSON library.");
                }
            }
        }
    }
}