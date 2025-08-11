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

        public int original_data_length;

        public int salt_length;
        public string date_string;
        public string random_num;
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
        NativeLibsodium.sodium_init();
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
            string secret_password = encryptedData.date_string + encryptedData.random_num;


            // KDF 密钥派生
            byte[] key;
            // 迭代次数固定为 100000
            using (var pbkdf2 = new Rfc2898DeriveBytes(secret_password, salt, 100000, HashAlgorithmName.SHA256))
            {
                key = pbkdf2.GetBytes(32);
            }

            // 使用libsodium进行AES-GCM解密
            byte[] decryptedBytes = new byte[ciphertext.Length];

            Debug.Log($"解密参数：");
            Debug.Log($"  Key (Base64): {Convert.ToBase64String(key)} (Length: {key.Length})");
            Debug.Log($"  IV (Base64): {Convert.ToBase64String(iv)} (Length: {iv.Length})");
            Debug.Log($"  Ciphertext (Base64): {Convert.ToBase64String(ciphertext)} (Length: {ciphertext.Length})");
            Debug.Log($"  Tag (Base64): {Convert.ToBase64String(tag)} (Length: {tag.Length})");

                // libsodium需要ciphertext和tag分开处理
                // 创建临时缓冲区存放ciphertext
                // GCM模式下，tag是单独的参数，需要使用 detached 解密函数
                ulong decryptedLength = 0;
                
                fixed (byte* decryptedPtr = decryptedBytes)
                {
                    int result = unity.libsodium.NativeLibsodium.crypto_aead_aes256gcm_decrypt_detached(
                        decryptedPtr,
                        (byte*)null, // nsec
                        ciphertext, // ciphertext
                        (ulong)ciphertext.Length, // clen
                        tag, // mac (authentication tag)
                        new byte[0], // ad (associated data), 这里没有关联数据，所以是空数组
                        (ulong)new byte[0].Length, // adlen
                        iv, // npub (nonce)
                        key // k
                    );

                    if (result != 0)
                    {
                        throw new CryptographicException("libsodium解密失败，可能密码错误或数据损坏");
                    }
                }

                // crypto_aead_aes256gcm_decrypt_detached 不会返回解密后的长度，需要手动设置
                // 这里的 decryptedLength 应该从加密时获取，或者通过其他方式确定
                // 暂时假设 decryptedBytes 的长度就是解密后的长度，这可能需要调整
                // 实际上，mlen_p 参数在 detached 版本中是用来返回明文长度的，但这里没有使用
                // 重新检查 NativeLibsodium.cs 中的 crypto_aead_aes256gcm_decrypt_detached 签名
                // 签名是：byte* m, byte* nsec, byte[] c, ulong clen, byte[] mac, byte[] ad, ulong adlen, byte[] npub, byte[] k
                // 缺少 mlen_p 参数，这可能是一个问题
                // 重新查看 NativeLibsodium.cs
                // 发现 crypto_aead_aes256gcm_decrypt_detached 确实没有 mlen_p 参数
                // 这意味着解密后的长度需要通过其他方式获取，或者在加密时就已知
                // 暂时先不调整 Array.Resize，因为 decryptedBytes 的大小是根据 ciphertext.Length 初始化的
                // 如果解密成功，decryptedBytes 应该包含正确的数据
                // 如果需要精确的长度，可能需要使用 crypto_aead_aes256gcm_decrypt_detached_afternm
                // 或者在加密时将原始数据长度包含在关联数据中
                // 考虑到 Encrypt.py 中有 original_data_length，可以利用这个信息
                Array.Resize(ref decryptedBytes, encryptedData.original_data_length);

            string decryptedJson = Encoding.UTF8.GetString(decryptedBytes);

            // 保存解密数据
            File.WriteAllText(decryptedFilePath, decryptedJson);
            Debug.Log($"解密成功，数据已保存到: {decryptedFilePath}");
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