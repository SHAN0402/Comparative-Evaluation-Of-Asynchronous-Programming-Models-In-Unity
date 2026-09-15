using UnityEngine;
using Cysharp.Threading.Tasks; // 引入 UniTask 命名空间

public class UniTaskLoader : MonoBehaviour
{
    async void Start()
    {
        // 持续循环进行高强度异步加载
        while (true)
        {
            await LoadRoutineAsync();
            await UniTask.Delay(500); // 相当于 0.5 秒延迟，但零分配
        }
    }

    async UniTask LoadRoutineAsync()
    {
        for (int i = 0; i < 20; i++)
        {
            // 使用 UniTask 包装的零分配异步加载
           var asset = await Resources.LoadAsync<GameObject>("YourModelName").ToUniTask(); }
    }
}