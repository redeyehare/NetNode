
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
    
    private void SavePhoneNumber(string phoneNumber)
    {
        AppData data = new AppData { phoneNumber = phoneNumber };
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(jsonFilePath, json);
        Debug.Log($"Phone number saved to {jsonFilePath}");
    }
    
    private string LoadSavedPhoneNumber()
    {
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            AppData data = JsonUtility.FromJson<AppData>(json);
            Debug.Log($"Phone number loaded from {jsonFilePath}");
            return data.phoneNumber;
        }
        return "";
    }
    
    void ShowErrorMessage(string message)
    {
        Debug.LogWarning(message);
        // 这里可以添加UI提示，比如Toast或者临时文本显示
    }
    
    void OnEnable()
    {
        LoadSavedPhoneNumber();
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