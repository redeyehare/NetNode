using System;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Linq;

public class SimpleDecryptor : MonoBehaviour
{
    public string encryptedDataFilePath = "encrypted_test_data.json";

    [Serializable]
    public class EncryptedTestData
    {
        public string key;
        public string iv;
        public string ciphertext;
        public string tag;
    }

    void Start()
    {
        DecryptTestData();
    }
    


    private byte[] SimpleDecrypt(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag)
    {
        try
        {
            // 验证输入参数
            if (key == null || key.Length == 0)
                throw new ArgumentException("密钥不能为空");
            if (ciphertext == null || ciphertext.Length == 0)
                throw new ArgumentException("密文不能为空");
            if (iv == null || iv.Length == 0)
                throw new ArgumentException("IV不能为空");
            if (tag == null || tag.Length == 0)
                throw new ArgumentException("认证标签不能为空");

            // 确保密钥长度为32字节（256位）
            byte[] finalKey = new byte[32];
            if (key.Length >= 32)
            {
                Array.Copy(key, 0, finalKey, 0, 32);
            }
            else
            {
                // 如果密钥不足32字节，用0填充
                Array.Copy(key, 0, finalKey, 0, key.Length);
                for (int i = key.Length; i < 32; i++)
                {
                    finalKey[i] = 0;
                }
            }

            try
            {
                // 尝试使用System.Security.Cryptography.AesGcm（如果平台支持）
                if (System.Environment.OSVersion.Platform != PlatformID.Unix || 
                    !UnityEngine.Application.isEditor)
                {
                    // 使用AES-GCM模式解密（匹配Python代码）
                    using (var aesGcm = new System.Security.Cryptography.AesGcm(finalKey))
                    {
                        byte[] plaintext = new byte[ciphertext.Length];
                        aesGcm.Decrypt(iv, ciphertext, tag, plaintext);
                        return plaintext;
                    }
                }
            }
            catch (PlatformNotSupportedException)
            {
                Debug.Log("平台不支持AesGcm，使用兼容实现");
            }

            // 如果System.Security.Cryptography.AesGcm不可用，则抛出异常
            throw new PlatformNotSupportedException("当前平台不支持System.Security.Cryptography.AesGcm。请考虑引入第三方库，例如BouncyCastle的C#版本，以支持AES-GCM解密。");
        }
        catch (PlatformNotSupportedException ex)
        {
            Debug.LogError($"平台不支持错误: {ex.Message}");
            throw;
        }
        catch (CryptographicException ex)
        {
            Debug.LogError($"加密错误: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Debug.LogError($"解密错误: {ex.Message}");
            throw;
        }
    }



    public void DecryptTestData()
    {
        string fullPath = Path.Combine(Application.dataPath, "test", encryptedDataFilePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"加密文件不存在: {fullPath}");
            return;
        }

        try
        {
            string jsonString = File.ReadAllText(fullPath);
            EncryptedTestData encryptedData = JsonUtility.FromJson<EncryptedTestData>(jsonString);

            byte[] key = Convert.FromBase64String(encryptedData.key);
            byte[] iv = Convert.FromBase64String(encryptedData.iv);
            byte[] ciphertext = Convert.FromBase64String(encryptedData.ciphertext);
            byte[] tag = Convert.FromBase64String(encryptedData.tag);

            Debug.Log($"--- C# 解密端 ---");
            Debug.Log($"Key (Base64): {Convert.ToBase64String(key)} (Length: {key.Length})");
            Debug.Log($"IV (Base64): {Convert.ToBase64String(iv)} (Length: {iv.Length})");
            Debug.Log($"Ciphertext (Base64): {Convert.ToBase64String(ciphertext)} (Length: {ciphertext.Length})");
            Debug.Log($"Tag (Base64): {Convert.ToBase64String(tag)} (Length: {tag.Length})");

            try
            {
                // 使用简单的AES解密方法
                byte[] decryptedBytes = SimpleDecrypt(ciphertext, key, iv, tag);
                
                string decryptedText = System.Text.Encoding.UTF8.GetString(decryptedBytes);
                Debug.Log($"解密成功！明文: {decryptedText}");
            }
            catch (CryptographicException e)
            {
                Debug.LogError($"解密失败: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"解密过程中发生错误: {e.Message}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"文件读取或JSON解析错误: {e.Message}");
        }
    }
}