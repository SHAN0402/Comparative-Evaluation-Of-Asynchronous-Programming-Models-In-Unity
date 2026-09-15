using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum TestMode
{
    Coroutine,
    UniTask
}

public class StreamingBenchmark : MonoBehaviour
{
    [Header("Test Configuration")]
    public TestMode currentMode = TestMode.Coroutine;
    public string assetPath = "TestCube"; // asset under folder Resource
    public int loopCount = 100; // number of concurrent loads per cycle 
    public float intervalSeconds = 0.5f; // time interval between load test triggers

    void Start()
    {
        StartCoroutine(RunBenchmarkLoop());
    }

    private IEnumerator RunBenchmarkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalSeconds);
            
            Debug.Log($"[Benchmark] 开始新一轮触发，当前模式: {currentMode}，并发数: {loopCount}");

            for (int i = 0; i < loopCount; i++) // 修正：使用 < loopCount 避免多执行一次
            {
                if (currentMode == TestMode.Coroutine)
                {
                    StartCoroutine(LoadAssetCoroutine(assetPath));
                }
                else
                {
                    LoadAssetUniTaskAsync(assetPath).Forget();
                }
            }
            
            // 修正：移到循环外面，或者正确打印当前进度
            Debug.Log($"[Benchmark] 本轮 {loopCount} 个资产已全部触发完毕。");
        }
    }

    // Group A: traditional coroutine
    private IEnumerator LoadAssetCoroutine(string path)
    {
        ResourceRequest request = Resources.LoadAsync<GameObject>(path);
        yield return request;

        if (request.asset != null)
        {
            GameObject obj = Instantiate(request.asset) as GameObject;
            Destroy(obj, 0.1f);
        }
    }

    // Group B: UniTask

    private async UniTaskVoid LoadAssetUniTaskAsync(string path)
    {
        // 使用 await 直接等待 Resources.LoadAsync，并将其 as 为 GameObject
        GameObject asset = await Resources.LoadAsync<GameObject>(path) as GameObject;

        if (asset != null)
        {
            GameObject obj = Instantiate(asset);
            Object.Destroy(obj, 0.1f);
        }
    }
}