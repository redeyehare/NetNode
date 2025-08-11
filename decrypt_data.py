import os
import json
import base64
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes

def decrypt_file_aes_gcm(
    encrypted_data: dict,
    password: str
) -> bytes:
    """
    使用 AES-GCM 解密数据。

    Args:
        encrypted_data (dict): 包含加密数据（盐、IV、密文、认证标签）的字典。
        password (str): 用于派生密钥的密码（或预共享密钥）。

    Returns:
        bytes: 解密后的原始数据。
    """
    try:
        salt = base64.b64decode(encrypted_data["salt"])
        iv = base64.b64decode(encrypted_data["iv"])
        ciphertext = base64.b64decode(encrypted_data["ciphertext"])
        tag = base64.b64decode(encrypted_data["tag"])
        iterations = encrypted_data.get("iterations", 100000) # 默认值与加密时保持一致
    except KeyError as e:
        print(f"错误：加密数据缺少必要字段 - {e}")
        return b''
    except base64.binascii.Error:
        print("错误：Base64 解码失败，数据可能已损坏。")
        return b''

    # KDF 密钥派生 (PBKDF2-HMAC-SHA256)
    kdf = PBKDF2HMAC(
        algorithm=hashes.SHA256(),
        length=32, # AES-256 密钥长度
        salt=salt,
        iterations=iterations,
        backend=default_backend()
    )
    key = kdf.derive(password.encode('utf-8'))

    # AES-GCM 解密
    cipher = Cipher(algorithms.AES(key), modes.GCM(iv, tag), backend=default_backend())
    decryptor = cipher.decryptor()

    try:
        plaintext = decryptor.update(ciphertext) + decryptor.finalize()
        return plaintext
    except Exception as e:
        print(f"解密失败：认证标签不匹配或数据已损坏 - {e}")
        return b''

if __name__ == "__main__":
    encrypted_file = "/Users/mahe/Project/unity/NetNode/Assets/test/encrypted_data.json"
    output_decrypted_file = "/Users/mahe/Project/unity/NetNode/Assets/test/decrypted_data.json"
    # 这里的 'your_secret_password' 必须与加密时使用的密码一致
    print(f"正在读取加密文件：{encrypted_file}")
    try:
        with open(encrypted_file, 'r') as f:
            encrypted_json_data = json.load(f)
        print("加密文件读取成功，开始解析数据...")

        current_date = encrypted_json_data.get('date_string')
        random_num = encrypted_json_data.get('random_num')
        if not current_date or not random_num:
            print("错误：加密数据中缺少日期或随机数信息，无法重构密码。")
            exit()
        secret_password = f"{current_date}{random_num}"

        print(f"KDF 迭代次数：{encrypted_json_data.get('iterations', '未知')}")
        print(f"盐长度：{encrypted_json_data.get('salt_length', '未知')} 字节")
    except FileNotFoundError:
        print(f"错误：加密文件未找到 - {encrypted_file}")
        exit()
    except json.JSONDecodeError:
        print(f"错误：加密文件 {encrypted_file} 不是有效的 JSON 格式。")
        exit()

    print("正在解密数据...")
    decrypted_bytes = decrypt_file_aes_gcm(encrypted_json_data, secret_password)

    if decrypted_bytes:
        print("解密过程完成，正在验证和保存结果...")
        try:
            # 尝试将解密后的字节数据解码为UTF-8字符串，并重新格式化为JSON
            decrypted_content = json.loads(decrypted_bytes.decode('utf-8'))
            print("解密内容验证成功，是有效的JSON格式")
            with open(output_decrypted_file, 'w') as f:
                json.dump(decrypted_content, f, indent=4)
            print(f"解密成功！原始数据已保存到：{output_decrypted_file}")
            print("解密后的内容预览：")
            print(json.dumps(decrypted_content, indent=4)[:200] + "...")
        except UnicodeDecodeError:
            print("解密成功，但内容不是有效的UTF-8字符串。可能不是JSON数据或编码错误。")
            with open(output_decrypted_file + ".bin", 'wb') as f:
                f.write(decrypted_bytes)
            print(f"原始字节数据已保存到：{output_decrypted_file}.bin")
            print("原始字节数据预览：")
            print(decrypted_bytes[:50])
            print("...")
        except json.JSONDecodeError:
            print("解密成功，但内容不是有效的JSON格式。原始字节数据已保存到文件。")
            with open(output_decrypted_file + ".bin", 'wb') as f:
                f.write(decrypted_bytes)
            print(f"原始字节数据已保存到：{output_decrypted_file}.bin")
            print("原始字节数据预览：")
            print(decrypted_bytes[:50])
            print("...")
    else:
        print("解密失败。请检查以下可能的问题：")
        print("1. 密码是否正确")
        print("2. 加密数据是否损坏")
        print("3. 加密参数是否匹配（盐、迭代次数等）")