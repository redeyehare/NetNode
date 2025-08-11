# HTTP按钮监听器使用说明

## 概述
这个脚本允许你通过Unity UI按钮触发HTTP GET请求，URL从InputField中获取。

## 提供的脚本

### 1. HttpButtonListener.cs (完整版)
功能特点：
- ✅ 监听按钮点击事件
- ✅ 从InputField读取URL
- ✅ 使用Best HTTP进行GET请求
- ✅ 显示请求状态和响应
- ✅ 错误处理和超时设置
- ✅ URL格式验证

### 2. SimpleHttpButton.cs (简化版)
功能特点：
- ✅ 最简化的实现
- ✅ 基本GET请求功能
- ✅ 适合快速原型开发

## 使用方法

### 步骤1：场景设置
1. 在Unity场景中创建一个Button
2. 创建一个InputField用于输入URL
3. 创建Text组件显示响应和状态（可选）

### 步骤2：脚本配置
1. 将 `HttpButtonListener.cs` 或 `SimpleHttpButton.cs` 挂载到一个GameObject上
2. 在Inspector中拖拽关联组件：
   - **Request Button**: 你的按钮组件
   - **URL Input Field**: 输入URL的InputField
   - **Response Text**: 显示响应的Text组件（可选）
   - **Status Text**: 显示状态的Text组件（可选）

### 步骤3：测试
1. 运行场景
2. 在InputField中输入URL（如：https://httpbingo.org/get）
3. 点击按钮发送GET请求

## 代码示例

### 基本使用
```csharp
// 发送GET请求
var request = HTTPRequest.CreateGet("https://httpbingo.org/get", (req, resp) => {
    if (resp != null && resp.IsSuccess)
    {
        Debug.Log("响应: " + resp.DataAsText);
    }
});
request.Send();
```

### 带参数的请求
```csharp
// 带查询参数的GET请求
string url = "https://httpbingo.org/get?param1=value1&param2=value2";
SendGetRequest(url);
```

## 注意事项

1. **URL格式**：脚本会自动添加https://前缀
2. **错误处理**：网络错误会显示在状态文本中
3. **响应限制**：长响应会被截断显示
4. **依赖**：确保已导入Best HTTP包

## 调试提示

- 查看Console窗口获取详细日志
- 使用 `https://httpbingo.org/get` 作为测试URL
- 检查网络连接是否正常