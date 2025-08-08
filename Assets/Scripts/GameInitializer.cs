using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        // 确保PurchaseHistorySender在场景中存在
        if (PurchaseHistorySender.Instance == null)
        {
            GameObject senderObject = new GameObject("PurchaseHistorySender");
            senderObject.AddComponent<PurchaseHistorySender>();
        }

        // 确保PurchaseHistoryManager的单例被初始化
        // 访问Instance属性会触发其构造函数，从而启动定时器
        PurchaseHistoryManager.Instance.ToString(); // 简单访问一下，确保构造函数被调用

        Debug.Log("游戏初始化完成：PurchaseHistoryManager和PurchaseHistorySender已准备就绪。");
    }
}