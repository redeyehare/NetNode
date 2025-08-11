import base64
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.backends import default_backend
import os

def simple_encrypt_aes_gcm(plaintext: bytes, key: bytes, iv: bytes) -> tuple:
    """
    使用 AES-GCM 加密数据。
    Args:
        plaintext (bytes): 要加密的明文数据。
        key (bytes): 32字节的加密密钥。
        iv (bytes): 12字节的初始化向量。
    Returns:
        tuple: (ciphertext, tag) 密文和认证标签。
    """
    cipher = Cipher(algorithms.AES(key), modes.GCM(iv), backend=default_backend())
    encryptor = cipher.encryptor()
    ciphertext = encryptor.update(plaintext) + encryptor.finalize()
    tag = encryptor.tag
    return ciphertext, tag

if __name__ == "__main__":
    # 固定密钥和IV，用于测试
    fixed_key = b'\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f' # 32 bytes
    fixed_iv = b'\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b' # 12 bytes
    plaintext_data = b"Hello, this is a test message for AES-GCM encryption."

    print("--- Python 加密端 ---")
    print(f"明文: {plaintext_data.decode('utf-8')}")
    print(f"固定密钥 (Base64): {base64.b64encode(fixed_key).decode('utf-8')}")
    print(f"固定IV (Base64): {base64.b64encode(fixed_iv).decode('utf-8')}")

    ciphertext, tag = simple_encrypt_aes_gcm(plaintext_data, fixed_key, fixed_iv)

    print(f"密文 (Base64): {base64.b64encode(ciphertext).decode('utf-8')}")
    print(f"标签 (Base64): {base64.b64encode(tag).decode('utf-8')}")

    # 将结果保存到文件，以便C#读取
    output_path = os.path.join("Assets", "test", "encrypted_test_data.json")
    with open(output_path, "w") as f:
        import json
        json.dump({
            "key": base64.b64encode(fixed_key).decode('utf-8'),
            "iv": base64.b64encode(fixed_iv).decode('utf-8'),
            "ciphertext": base64.b64encode(ciphertext).decode('utf-8'),
            "tag": base64.b64encode(tag).decode('utf-8')
        }, f, indent=4)
    print("加密数据已保存到 encrypted_test_data.json")