using System;
using unity.libsodium;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Decryptor : MonoBehaviour
{
    [Serializable]
    public class EncryptedData
    {
        public string salt;
        public string iv;
        public string ciphertext;
        public string tag;
        public string timestamp;
        public int iterations;
        public int salt_length;
        public string date_string;
        public string random_number_string;
    }

    [Serializable]
    public class OriginalData
    {
        public V2RayConfig v2ray;
        public ClashConfig clash;
        public SingboxConfig singbox;
    }

    [Serializable]
    public class V2RayConfig
    {
        public string ps;
        public string add;
        public int port;
        public string id;
        public int aid;
        public string scy;
        public string net;
        public string type;
        public string host;
        public string path;
        public string tls;
        public string sni;
        public string alpn;
    }

    [Serializable]
    public class ClashConfig
    {
        public string name;
        public string type;
        public string server;
        public int port;
        public string uuid;
        public string cipher;
        public bool udp;
        public string tls;
        public string skip_cert_verify;
        public string network;
        public string ws_path;
        public string ws_headers_Host;
    }

    [Serializable]
    public class SingboxConfig
    {
        public string type;
        public string tag;
        public string server;
        public int server_port;
        public string uuid;
        public string security;
        public string alterId;
        public string network;
        public string tls;
        public string udp_over_tcp;
        public string ws_path;
        public string ws_headers_Host;
    }

    public string encryptedFilePath = "Assets/test/encrypted_data.json";
    public string decryptedFilePath = "Assets/test/decrypted_data.json";

    void Start()
    {
        DecryptFile();
    }

    public unsafe void DecryptFile()
    {
        if (!File.Exists(encryptedFilePath))
        {
            Debug.LogError($"加密文件不存在: {encryptedFilePath}");
            return;
        }

        try
        {
            string jsonString = File.ReadAllText(encryptedFilePath);
            EncryptedData encryptedData = JsonUtility.FromJson<EncryptedData>(jsonString);

            byte[] salt = Convert.FromBase64String(encryptedData.salt);
            byte[] iv = Convert.FromBase64String(encryptedData.iv);
            byte[] ciphertext = Convert.FromBase64String(encryptedData.ciphertext);
            byte[] tag = Convert.FromBase64String(encryptedData.tag);

            // 组合密码
            string secret_password = encryptedData.date_string + encryptedData.random_number_string + encryptedData.iterations.ToString();
            Debug.Log($"C#端组合的密码: {secret_password}");

            // KDF 密钥派生
            byte[] key;
            using (var pbkdf2 = new Rfc2898DeriveBytes(secret_password, salt, encryptedData.iterations, HashAlgorithmName.SHA256))
            {
                key = pbkdf2.GetBytes(32);
            }

            // 使用libsodium进行AES-GCM解密
            byte[] decryptedBytes = new byte[ciphertext.Length];
            
            fixed (byte* decryptedPtr = decryptedBytes)
            fixed (byte* ciphertextPtr = ciphertext)
            fixed (byte* tagPtr = tag)
            fixed (byte* ivPtr = iv)
            fixed (byte* keyPtr = key)
            {
                // libsodium需要ciphertext和tag分开处理
                // 创建临时缓冲区存放ciphertext+tag
                byte[] combinedData = new byte[ciphertext.Length + tag.Length];
                Buffer.BlockCopy(ciphertext, 0, combinedData, 0, ciphertext.Length);
                Buffer.BlockCopy(tag, 0, combinedData, ciphertext.Length, tag.Length);

                ulong decryptedLength = 0;
                
                int result = unity.libsodium.NativeLibsodium.crypto_aead_aes256gcm_decrypt(
                    decryptedPtr,
                    &decryptedLength,
                    null,
                    combinedData,
                    (ulong)combinedData.Length,
                    null,
                    0,
                    iv,
                    key
                );

                if (result != 0)
                {
                    throw new CryptographicException("libsodium解密失败，可能密码错误或数据损坏");
                }

                Array.Resize(ref decryptedBytes, (int)decryptedLength);
            }

            string decryptedJson = Encoding.UTF8.GetString(decryptedBytes);

            // 保存解密数据
            File.WriteAllText(decryptedFilePath, decryptedJson);
            Debug.Log($"解密成功，数据已保存到: {decryptedFilePath}");

            // 验证数据完整性
            try
            {
                OriginalData originalData = JsonUtility.FromJson<OriginalData>(decryptedJson);
                Debug.Log("数据验证通过");
            }
            catch (Exception)
            {
                Debug.LogWarning("解密成功但数据格式验证失败");
            }
        }
        catch (CryptographicException ex)
        {
            Debug.LogError($"解密失败: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"处理错误: {ex.Message}");
        }
    }
}