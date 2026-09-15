using System.Collections;
using UnityEngine;

public class CoroutineLoader : MonoBehaviour
{
    IEnumerator Start()
    {
        // 模拟连续不断地进行高强度异步加载
        while (true)
        {
            yield return StartCoroutine(TheLoadRoutine());
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator TheLoadRoutine()
    {
        // 假设我们要循环加载 Resources 文件夹里的资产
        for (int i = 0; i < 20; i++)
        {
            // 传统异步加载 API
            ResourceRequest request = Resources.LoadAsync<Texture2D>("TestTexture"); 
            yield return request; // 这里会产生临时对象和状态机分配

            // 模拟实例化或使用
            if (request.asset != null)
            {
                Texture2D tex = request.asset as Texture2D;
            }
        }
    }
}