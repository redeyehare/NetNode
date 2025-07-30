import os
import json
import base64
from datetime import datetime
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.kdf.pbkdf2 import PBKDF2HMAC
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes

def encrypt_file_aes_gcm(
    file_path: str,
    password: str,
    iterations: int = 100000, # 手机端常用迭代次数，可根据性能和安全需求调整
    salt_length: int = 16 # 盐的长度，推荐16字节
) -> dict:
    """
    使用 AES-GCM 加密文件内容，并包含 KDF、盐和时间戳。

    Args:
        file_path (str): 要加密的文件路径。
        password (str): 用于派生密钥的密码（或预共享密钥）。
        iterations (int): KDF 的迭代次数。
        salt_length (int): 盐的长度（字节）。

    Returns:
        dict: 包含加密数据（盐、IV、密文、认证标签、时间戳）的字典。
    """
    try:
        with open(file_path, 'rb') as f:
            plaintext = f.read()
    except FileNotFoundError:
        print(f"错误：文件未找到 - {file_path}")
        return {}

    # 1. 生成盐
    salt = os.urandom(salt_length)

    # 2. KDF 密钥派生 (PBKDF2-HMAC-SHA256)
    # 注意：AES-256 需要 32 字节的密钥
    kdf = PBKDF2HMAC(
        algorithm=hashes.SHA256(),
        length=32, # AES-256 密钥长度
        salt=salt,
        iterations=iterations,
        backend=default_backend()
    )
    key = kdf.derive(password.encode('utf-8'))

    # 3. 生成 IV (Initialization Vector)
    # AES-GCM 推荐使用 12 字节的 IV
    iv = os.urandom(12)

    # 4. AES-GCM 加密
    cipher = Cipher(algorithms.AES(key), modes.GCM(iv), backend=default_backend())
    encryptor = cipher.encryptor()
    ciphertext = encryptor.update(plaintext) + encryptor.finalize()
    tag = encryptor.tag

    # 5. 获取当前时间戳
    timestamp = datetime.now().isoformat()

    # 编码为 Base64 字符串以便于 JSON 存储
    encrypted_data = {
        "salt": base64.b64encode(salt).decode('utf-8'),
        "iv": base64.b64encode(iv).decode('utf-8'),
        "ciphertext": base64.b64encode(ciphertext).decode('utf-8'),
        "tag": base64.b64encode(tag).decode('utf-8'),
        "timestamp": timestamp,
        "iterations": iterations,
        "salt_length": salt_length,
        "date_string": date_string,
        "random_number_string": random_number_string
    }

    return encrypted_data

if __name__ == "__main__":
    input_file = "/Users/mahe/Project/unity/NetNode/Assets/test/load.json"
    output_file = "/Users/mahe/Project/unity/NetNode/Assets/test/encrypted_data.json"
    # 使用日期+随机数+迭代次数作为密码
    import random
    current_date = datetime.now().strftime("%Y%m%d")
    random_num = str(random.randint(1000, 9999))
    secret_password = f"{current_date}{random_num}{100000}"

    print(f"正在加密文件：{input_file}")
    print(f"生成的密码：{secret_password}")
    encrypted_result = encrypt_file_aes_gcm(input_file, secret_password)

    if encrypted_result:
        with open(output_file, 'w') as f:
            json.dump(encrypted_result, f, indent=4)
        print(f"加密成功！加密数据已保存到：{output_file}")
        print("请注意：此脚本仅为演示目的。在生产环境中，请确保密码管理和密钥分发的安全性。")
        print("此外，请确保安装了 'cryptography' 库：pip install cryptography")
    else:
        print("加密失败。")