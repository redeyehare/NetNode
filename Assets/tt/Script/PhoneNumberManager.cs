using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class PhoneNumberManager : MonoBehaviour
{
    [Header("UI References")]
    // 手机号码输入框，用于用户输入手机号码
    public TMP_InputField phoneInputField;
    
    // 确认按钮，用户点击后确认输入的手机号码
    public Button confirmButton;
    // 编辑按钮，用户点击后重新进入手机号码输入界面
    public Button editButton;
    // 手机号码显示文本，用于显示当前或脱敏后的手机号码
    public TextMeshProUGUI displayText;
    // 输入面板，包含手机号码输入框和确认按钮
    public GameObject inputPanel;

    
    [Header("Settings")]
    // 输入框的占位符文本，提示用户输入内容
    public string placeholderText = "请输入手机号码";
    
    private string jsonFilePath;
    private string currentPhoneNumber = "";
    // 定义一个内部类来存储手机号码数据，以便进行JSON序列化
    [System.Serializable]
    public class AppData
    {
        public string phoneNumber;
        public string uuid;
        public string mark = "";
    }

    void Awake()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "appData.json");
    }

    void Start()
    {
        InitializeUI();
        SetupEventListeners();
    }
    
    void InitializeUI()
    {
        // 初始显示输入面板
        ShowInputPanel();
        
        // 设置输入框占位符
        if (phoneInputField.placeholder != null)
        {
            ((TextMeshProUGUI)phoneInputField.placeholder).text = placeholderText;
        }
    }
    void SetupEventListeners()
    {
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        editButton.onClick.AddListener(OnEditButtonClicked);
        
        // 监听输入变化以验证手机号格式
        phoneInputField.onValueChanged.AddListener(OnPhoneNumberChanged);
    }
    
    void OnPhoneNumberChanged(string phoneNumber)
    {
        // 简单的手机号格式验证
        bool isValid = IsValidPhoneNumber(phoneNumber);
        confirmButton.interactable = isValid;
    }
    
    bool IsValidPhoneNumber(string phoneNumber)
    {
        // 基本的中国手机号验证：11位数字，以1开头
        if (string.IsNullOrEmpty(phoneNumber))
            return false;
            
        if (phoneNumber.Length != 11)
            return false;
            
        if (!phoneNumber.StartsWith("1"))
            return false;
            
        // 检查是否全为数字
        foreach (char c in phoneNumber)
        {
            if (!char.IsDigit(c))
                return false;
        }
        
        return true;
    }
    
    void OnConfirmButtonClicked()
    {
        string inputPhone = phoneInputField.text.Trim();
        
        if (IsValidPhoneNumber(inputPhone))
        {
            currentPhoneNumber = inputPhone;
            ShowDisplayPanel();
            
            // 可以在这里添加保存到本地存储的逻辑
            SavePhoneNumber(currentPhoneNumber);
        }
        else
        {
            ShowErrorMessage("请输入有效的手机号码");
        }
    }
    
    void OnEditButtonClicked()
    {
        ShowInputPanel();
        phoneInputField.text = currentPhoneNumber;
        phoneInputField.Select();
    }
    
    void ShowInputPanel()
    {
        inputPanel.SetActive(true);
        phoneInputField.Select();
    }
    
    void ShowDisplayPanel()
    {
        inputPanel.SetActive(false);
        
        // 格式化显示手机号（中间4位用*号替代）
        string maskedPhone = MaskPhoneNumber(currentPhoneNumber);
        displayText.text = "当前手机号: " + maskedPhone;
    }
    
    string MaskPhoneNumber(string phoneNumber)
    {
        if (phoneNumber.Length == 11)
        {
            return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(7);
        }
        return phoneNumber;
    }
    
    public void SavePhoneNumber(string phoneNumber)
    // This method saves the phone number and updates the mark field.
    {
        AppData currentData = LoadAppData(); // 加载现有数据，LoadAppData现在会处理UUID的生成
        // 使用 FromJsonOverwrite 更新 phoneNumber
        // Restored mark field update logic
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        string patchJson = $"{{ \"phoneNumber\": \"{phoneNumber}\", \"mark\": \"{today}\" }}";
        JsonUtility.FromJsonOverwrite(patchJson, currentData);

        string json = JsonUtility.ToJson(currentData);
        File.WriteAllText(jsonFilePath, json);
        Debug.Log($"Phone number and UUID saved to {jsonFilePath}");
    }
    
    private AppData LoadAppData()
    {
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            AppData data = new AppData();
            JsonUtility.FromJsonOverwrite(json, data);
            // 确保加载时也处理UUID
            if (string.IsNullOrEmpty(data.uuid))
            {
                data.uuid = SystemInfo.deviceUniqueIdentifier;
                Debug.Log($"Generated new UUID: {data.uuid}");
            }
            if (string.IsNullOrEmpty(data.mark))
            {
                data.mark = System.DateTime.Now.ToString("yyyy-MM-dd");
                Debug.Log($"Initialized new mark: {data.mark}");
            }
            return data;
        }
        // 如果文件不存在，返回一个新的AppData对象，并生成UUID和mark
        return new AppData { uuid = SystemInfo.deviceUniqueIdentifier, mark = System.DateTime.Now.ToString("yyyy-MM-dd") };
    }

    private void LoadSavedPhoneNumber()
    {
        AppData data = LoadAppData();
        phoneInputField.text = data.phoneNumber;
        Debug.Log($"Loaded phone number: {data.phoneNumber} with UUID: {data.uuid}");
    }
    
    void ShowErrorMessage(string message)
    {
        Debug.LogWarning(message);
        // 这里可以添加UI提示，比如Toast或者临时文本显示
    }
    
    void OnEnable()
    {
        // 在启用时加载保存的手机号码
        LoadSavedPhoneNumber();

        // 获取当前日期字符串
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        AppData data = LoadAppData();

        // 根据mark字段和手机号码是否存在来决定显示哪个面板
        if (!string.IsNullOrEmpty(phoneInputField.text) && data.mark == today)
        {
            currentPhoneNumber = phoneInputField.text;
            ShowDisplayPanel();
        }
        else
        {
            ShowInputPanel();
        }
    }
    void OnDestroy()
    {
        // 清理事件监听
        if (confirmButton != null)
            confirmButton.onClick.RemoveAllListeners();
        if (editButton != null)
            editButton.onClick.RemoveAllListeners();
        if (phoneInputField != null)
            phoneInputField.onValueChanged.RemoveAllListeners();
    }
}