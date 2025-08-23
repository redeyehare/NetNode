using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "Scriptable Objects/Config")]
public class Config : ScriptableObject
{
    public string pullUrl;
    public string serverUrl;
    //地址请求间隔
    public float getRequestInterval;
    //post请求间隔
    public float postRequestInterval;

    
    public string testPostUrl;

    public string configJsPath;
    public string dataJsPath;



}
