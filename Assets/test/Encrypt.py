import os
import json
import base64
from cryptography.hazmat.primitives.asymmetric import ec
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives import serialization
from cryptography.hazmat.primitives.kdf.hkdf import HKDF
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import padding

# --- 配置路径 ---
JSON_FILE_PATH = '/Users/mahe/Project/unity/NetNode/Assets/test/load.json'
ENCRYPTED_FILE_PATH = '/Users/mahe/Project/unity/NetNode/Assets/test/date.json'
PRIVATE_KEY_PATH = 'ecc_private_key.pem'
PUBLIC_KEY_PATH = 'ecc_public_key.pem'

# --- 1. 生成 ECC 密钥对 ---
def generate_ecc_keys():
    private_key = ec.generate_private_key(
        ec.SECP256R1(), # 使用 NIST P-256 曲线
        default_backend()
    )
    public_key = private_key.public_key()

    # 保存私钥
    with open(PRIVATE_KEY_PATH, "wb") as f:
        f.write(private_key.private_bytes(
            encoding=serialization.Encoding.PEM,
            format=serialization.PrivateFormat.PKCS8,
            encryption_algorithm=serialization.NoEncryption() # 实际应用中应加密私钥
        ))
    print(f"私钥已保存到 {PRIVATE_KEY_PATH}")

    # 保存公钥
    with open(PUBLIC_KEY_PATH, "wb") as f:
        f.write(public_key.public_bytes(
            encoding=serialization.Encoding.PEM,
            format=serialization.PublicFormat.SubjectPublicKeyInfo
        ))
    print(f"公钥已保存到 {PUBLIC_KEY_PATH}")

    return private_key, public_key

# --- 2. 加密数据 (使用 ECIES 思想，但简化为直接加密对称密钥) ---
def encrypt_data(public_key, data_to_encrypt):
    # ECC 非对称加密通常用于加密对称密钥，而不是直接加密大量数据。
    # 这里为了演示，我们模拟一个简单的 ECIES 流程：
    # 1. 生成一个临时的 ECC 密钥对 (ephemeral_private_key)
    # 2. 使用 ephemeral_private_key 和接收方的 public_key 派生共享秘密 (shared_secret)
    # 3. 使用 shared_secret 派生对称加密密钥 (key) 和 IV
    # 4. 使用对称加密密钥和 IV 加密实际数据

    ephemeral_private_key = ec.generate_private_key(
        ec.SECP256R1(),
        default_backend()
    )
    shared_secret = ephemeral_private_key.exchange(ec.ECDH(), public_key)

    # 使用 HKDF 从共享秘密派生对称密钥和 IV
    derived_key = HKDF(algorithm=hashes.SHA256(),
                       length=32, # AES-256 密钥长度
                       salt=None, # 确保加密和解密时 salt 一致
                       info=b'aes-key',
                       backend=default_backend()).derive(shared_secret)
    derived_iv = HKDF(algorithm=hashes.SHA256(),
                      length=16, # AES IV 长度
                      salt=None, # 确保加密和解密时 salt 一致
                      info=b'aes-iv',
                      backend=default_backend()).derive(shared_secret)

    # 对称加密数据
    cipher = Cipher(algorithms.AES(derived_key), modes.CBC(derived_iv), backend=default_backend())
    encryptor = cipher.encryptor()

    # PKCS7 填充
    padder = padding.PKCS7(algorithms.AES.block_size).padder()
    padded_data = padder.update(data_to_encrypt) + padder.finalize()

    ciphertext = encryptor.update(padded_data) + encryptor.finalize()

    # 将所有加密信息封装到 JSON 对象中
    ephemeral_public_key_pem = ephemeral_private_key.public_key().public_bytes(
        encoding=serialization.Encoding.PEM,
        format=serialization.PublicFormat.SubjectPublicKeyInfo
    ).decode('utf-8') # 解码为字符串以便放入 JSON

    encrypted_data_json = {
        "ephemeral_public_key": ephemeral_public_key_pem,

        "ciphertext": base64.b64encode(ciphertext).decode('utf-8'),
        "algorithm": "ECC_AES256_CBC_PKCS7",
        "original_data_length": len(data_to_encrypt)
    }
    return json.dumps(encrypted_data_json, indent=2).encode('utf-8') # 返回 JSON 字符串的字节形式

# --- 3. 解密数据 ---
def decrypt_data(private_key, encrypted_json_data_bytes):
    # 解析 JSON 格式的加密数据
    encrypted_json_data = json.loads(encrypted_json_data_bytes.decode('utf-8'))

    ephemeral_public_key_pem = encrypted_json_data["ephemeral_public_key"]
    ciphertext_b64 = encrypted_json_data["ciphertext"]

    # Base64 解码
    ephemeral_public_key_bytes = ephemeral_public_key_pem.encode('utf-8')
    ciphertext = base64.b64decode(ciphertext_b64)

    # 加载临时公钥
    ephemeral_public_key = serialization.load_pem_public_key(
        ephemeral_public_key_bytes,
        backend=default_backend()
    )

    # 使用接收方的 private_key 和临时公钥派生共享秘密
    shared_secret = private_key.exchange(ec.ECDH(), ephemeral_public_key)

    # 使用 HKDF 从共享秘密派生对称密钥和 IV (与加密时相同)
    derived_key = HKDF(algorithm=hashes.SHA256(),
                       length=32,
                       salt=None,
                       info=b'aes-key',
                       backend=default_backend()).derive(shared_secret)
    derived_iv = HKDF(algorithm=hashes.SHA256(),
                      length=16,
                      salt=None,
                      info=b'aes-iv',
                      backend=default_backend()).derive(shared_secret)

    # 对称解密数据
    cipher = Cipher(algorithms.AES(derived_key), modes.CBC(derived_iv), backend=default_backend())
    decryptor = cipher.decryptor()
    padded_plaintext = decryptor.update(ciphertext) + decryptor.finalize()

    # PKCS7 去填充
    unpadder = padding.PKCS7(algorithms.AES.block_size).unpadder()
    plaintext = unpadder.update(padded_plaintext) + unpadder.finalize()

    return plaintext

# --- 主流程 ---
if __name__ == "__main__":
    # 检查并生成密钥对
    if not os.path.exists(PRIVATE_KEY_PATH) or not os.path.exists(PUBLIC_KEY_PATH):
        print("生成新的 ECC 密钥对...")
        private_key, public_key = generate_ecc_keys()
    else:
        print("加载现有 ECC 密钥对...")
        with open(PRIVATE_KEY_PATH, "rb") as f:
            private_key = serialization.load_pem_private_key(
                f.read(),
                password=None, # 如果私钥加密了，这里需要提供密码
                backend=default_backend()
            )
        with open(PUBLIC_KEY_PATH, "rb") as f:
            public_key = serialization.load_pem_public_key(
                f.read(),
                backend=default_backend()
            )

    # 读取 JSON 文件内容
    try:
        with open(JSON_FILE_PATH, 'rb') as f:
            json_data = f.read()
        print(f"成功读取 {JSON_FILE_PATH}，大小：{len(json_data)} 字节")
    except FileNotFoundError:
        print(f"错误：文件 {JSON_FILE_PATH} 未找到。请确保文件存在。")
        exit()

    # 加密数据
    print("开始加密数据...")
    encrypted_json_output = encrypt_data(public_key, json_data)
    print("数据加密完成。")

    # 将加密后的 JSON 数据保存到文件
    with open(ENCRYPTED_FILE_PATH, 'wb') as f:
        f.write(encrypted_json_output)
    print(f"加密数据已保存到 {ENCRYPTED_FILE_PATH}，大小：{len(encrypted_json_output)} 字节")

    # --- 解密演示 ---
    print("\n--- 解密演示 ---")
    try:
        with open(ENCRYPTED_FILE_PATH, 'rb') as f:
            loaded_encrypted_json_output = f.read()

        decrypted_data = decrypt_data(private_key, loaded_encrypted_json_output)

        print("数据解密完成。")
        print("解密后的数据（前100字节）：")
        print(decrypted_data[:100].decode('utf-8', errors='ignore'))

        if decrypted_data == json_data:
            print("解密数据与原始数据一致！")
        else:
            print("解密数据与原始数据不一致！")

    except Exception as e:
        print(f"解密过程中发生错误：{e}")


# 运行前请确保已安装 cryptography 库：pip install cryptography
# 注意：此示例中的私钥未加密保存，实际生产环境中应加密私钥并妥善保管。
# 此外，ECC 直接加密的数据长度有限，对于大文件，应采用混合加密方案：
# 1. 生成一个随机的对称密钥（如 AES 密钥）。
# 2. 使用该对称密钥加密大文件内容。
# 3. 使用 ECC 公钥加密该对称密钥。
# 4. 将加密后的对称密钥和加密后的文件内容一起传输或保存。