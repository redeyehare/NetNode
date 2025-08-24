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

    [Header("Configuration")]
    // 配置文件引用，包含服务器地址、请求间隔等配置信息
    public Config config;
    
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

    /// <summary>
    /// Unity Awake方法：在脚本实例被加载时调用
    /// 初始化JSON文件路径，用于存储应用数据
    /// 验证Config配置文件是否正确设置
    /// </summary>
    void Awake()
    {
        jsonFilePath = config.configJsPath;
        
        // 验证Config配置是否已设置
        ValidateConfig();
    }

    /// <summary>
    /// Unity Start方法：在第一帧更新之前调用
    /// 设置事件监听器
    /// </summary>
    void Start()
    {
        SetupEventListeners();
    }
    
    /// <summary>
    /// 初始化UI界面
    /// 设置输入框的占位符文本
    /// </summary>
    void InitializeUI()
    {
        // 设置输入框占位符
        if (phoneInputField.placeholder != null)
        {
            ((TextMeshProUGUI)phoneInputField.placeholder).text = placeholderText;
        }
    }
    /// <summary>
    /// 设置事件监听器
    /// 为确认按钮、编辑按钮和输入框添加点击和值变化事件监听
    /// </summary>
    void SetupEventListeners()
    {
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        editButton.onClick.AddListener(OnEditButtonClicked);
        
        // 监听输入变化以验证手机号格式
        phoneInputField.onValueChanged.AddListener(OnPhoneNumberChanged);
    }
    
    /// <summary>
    /// 手机号码输入变化事件处理
    /// 当用户在输入框中输入内容时触发，验证手机号格式并控制确认按钮的可用状态
    /// </summary>
    /// <param name="phoneNumber">用户输入的手机号码</param>
    void OnPhoneNumberChanged(string phoneNumber)
    {
        // 简单的手机号格式验证
        bool isValid = IsValidPhoneNumber(phoneNumber);
        confirmButton.interactable = isValid;
    }
    
    /// <summary>
    /// 验证手机号码格式是否有效
    /// 检查手机号是否符合中国手机号的基本格式要求：11位数字，以1开头
    /// </summary>
    /// <param name="phoneNumber">需要验证的手机号码</param>
    /// <returns>如果手机号格式有效返回true，否则返回false</returns>
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
    
    /// <summary>
    /// 确认按钮点击事件处理
    /// 验证用户输入的手机号码，如果有效则保存并切换到显示面板，否则显示错误信息
    /// </summary>
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
    
    /// <summary>
    /// 编辑按钮点击事件处理
    /// 切换到输入面板，清空输入框并选中输入框供用户重新输入
    /// </summary>
    void OnEditButtonClicked()
    {
        ShowInputPanel();
        // 清空输入框，让用户重新输入
        phoneInputField.text = "";
        phoneInputField.Select();
    }
    
    /// <summary>
    /// 显示输入面板
    /// 激活输入面板并自动选中输入框，方便用户输入
    /// 如果已有手机号，显示脱敏后的手机号；否则显示输入提示文本
    /// </summary>
    void ShowInputPanel()
    {
        inputPanel.SetActive(true);
        phoneInputField.Select();
        
        // 根据是否有手机号决定显示内容
        if (displayText != null)
        {
            if (!string.IsNullOrEmpty(currentPhoneNumber))
            {
                string maskedPhone = MaskPhoneNumber(currentPhoneNumber);
                displayText.text = "手机号: " + maskedPhone;
            }
            else
            {
                displayText.text = "请输入您的手机号码";
            }
        }
    }
    
    /// <summary>
    /// 显示手机号码面板
    /// 隐藏输入面板，在displayText中显示脱敏处理后的手机号码
    /// </summary>
    void ShowDisplayPanel()
    {
        // 隐藏输入面板
        inputPanel.SetActive(false);
        
        // 在displayText中显示脱敏手机号
        string maskedPhone = MaskPhoneNumber(currentPhoneNumber);
        if (displayText != null)
        {
            displayText.text = "手机号: " + maskedPhone ;
        }
    }
    
    /// <summary>
    /// 对手机号码进行脱敏处理
    /// 将手机号码中间4位数字替换为*号，保护用户隐私
    /// </summary>
    /// <param name="phoneNumber">需要脱敏的手机号码</param>
    /// <returns>脱敏后的手机号码字符串</returns>
    string MaskPhoneNumber(string phoneNumber)
    {
        if (phoneNumber.Length == 11)
        {
            return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(7);
        }
        return phoneNumber;
    }
    
    /// <summary>
    /// 保存手机号码到本地存储
    /// 将手机号码、设备UUID和当前日期标记保存到JSON文件中
    /// </summary>
    /// <param name="phoneNumber">需要保存的手机号码</param>
    public void SavePhoneNumber(string phoneNumber)
    {
        // 先加载现有数据以获取或生成UUID
        AppData currentData = LoadAppData();
        
        // 更新手机号码和mark字段
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        currentData.phoneNumber = phoneNumber;
        currentData.mark = today;
        
        // 使用JsonFileManager的WriteJson方法保存完整数据
        bool success = JsonFileManager.Instance.WriteJson(jsonFilePath, currentData);
        
        if (success)
        {
            Debug.Log($"Phone number, UUID and mark saved to {jsonFilePath}");
        }
        else
        {
            Debug.LogError($"Failed to save phone number to {jsonFilePath}");
        }
    }
    
    /// <summary>
    /// 从本地存储加载应用数据
    /// 使用JsonFileManager读取JSON文件中的AppData数据
    /// </summary>
    /// <returns>从文件中读取的AppData对象</returns>
    private AppData LoadAppData()
    {
        // 使用JsonFileManager的ReadJson方法读取数据
        AppData data = JsonFileManager.Instance.ReadJson<AppData>(jsonFilePath);
        
        return data;
    }


    /// <summary>
    /// 显示错误信息
    /// 在控制台输出警告信息，可扩展为UI提示
    /// </summary>
    /// <param name="message">需要显示的错误信息</param>
    void ShowErrorMessage(string message)
    {
        Debug.LogWarning(message);
        // 这里可以添加UI提示，比如Toast或者临时文本显示
    }
    
    /// <summary>
    /// Unity OnEnable方法：当对象变为启用和激活状态时调用
    /// 初始化UI界面，加载保存的应用数据，初始化UUID和mark字段，并根据数据状态决定显示哪个面板
    /// </summary>
    void OnEnable()
    {
        // 初始化UI界面
        InitializeUI();
        
        // 在启用时加载保存的手机号码
        AppData data = LoadAppData();
        
        // 如果生成了新的UUID或mark，需要保存
        bool needSave = false;
        if (string.IsNullOrEmpty(data.uuid))
        {
            data.uuid = SystemInfo.deviceUniqueIdentifier;
            needSave = true;
        }
        if (string.IsNullOrEmpty(data.mark))
        {
            data.mark = System.DateTime.Now.ToString("yyyy-MM-dd");
            needSave = true;
        }
        
        if (needSave)
        {
            JsonFileManager.Instance.WriteJson(jsonFilePath, data);
        }

        // 设置当前手机号码
        currentPhoneNumber = data.phoneNumber ?? "";
        Debug.Log($"Loaded phone number: {data.phoneNumber} with UUID: {data.uuid}");

        // 获取当前日期字符串
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");

        // 根据mark字段和手机号码是否存在来决定显示哪个面板
        if (!string.IsNullOrEmpty(data.phoneNumber) && data.mark == today)
        {
            ShowDisplayPanel();
        }
        else
        {
            // 设置输入框的初始值
            phoneInputField.text = data.phoneNumber ?? "";
            ShowInputPanel();
        }
    }
    
    /// <summary>
    /// 验证Config配置文件是否正确设置
    /// 检查必要的配置项是否存在和有效
    /// </summary>
    private void ValidateConfig()
    {
        if (config == null)
        {
            Debug.LogError("[PhoneNumberManager] Config 配置文件未设置！请在Inspector中分配 Config 对象。");
            return;
        }
        
        // 验证必要的URL配置
        if (string.IsNullOrEmpty(config.pullUrl))
        {
            Debug.LogWarning("[PhoneNumberManager] pullUrl 未配置");
        }
        
        if (string.IsNullOrEmpty(config.serverUrl))
        {
            Debug.LogWarning("[PhoneNumberManager] serverUrl 未配置");
        }
        
        if (string.IsNullOrEmpty(config.testPostUrl))
        {
            Debug.LogWarning("[PhoneNumberManager] testPostUrl 未配置");
        }
        
        // 验证请求间隔配置
        if (config.getRequestInterval <= 0)
        {
            Debug.LogWarning("[PhoneNumberManager] getRequestInterval 应大于0");
        }
        
        if (config.postRequestInterval <= 0)
        {
            Debug.LogWarning("[PhoneNumberManager] postRequestInterval 应大于0");
        }
        
        Debug.Log("[PhoneNumberManager] Config 配置验证完成");
    }
    
    /// <summary>
    /// Unity OnDestroy方法：当脚本实例被销毁时调用
    /// 清理所有事件监听器，防止内存泄漏
    /// </summary>
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