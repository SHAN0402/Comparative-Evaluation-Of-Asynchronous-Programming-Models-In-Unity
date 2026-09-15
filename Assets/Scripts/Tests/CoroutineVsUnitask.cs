using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Unity.Profiling.Memory;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;



public class CoroutineVsUnitask
{
    BenchmarkSettings benchmarkManager;
    EmptyBehaviour behaviour;

    [SetUp]
    public void Setup()
    {
        if (benchmarkManager == null)
        {
            benchmarkManager = Resources.Load<BenchmarkSettings>("BenchmarkSettings");
            if (benchmarkManager == null)
            {
                Debug.LogError("找不到 BenchmarkSettings 配置文件，请检查 Resources 文件夹！");
            }
        }

        if (behaviour == null)
        {
            behaviour = new GameObject("CoroutineRunner").AddComponent<EmptyBehaviour>();
        }
    }

    //help method
    //create an array for simulate data
    void CreateSimulationArray<T>(out T[] simulationsArray)
    {
        int totalLength = benchmarkManager.SimulationCount + benchmarkManager.InitialThreshold;
        simulationsArray = new T[totalLength];

    }
    //end the test and export the data
    void ConcludeTest<T>(T[] results, string testLabel)
    {
        JSONWriter.WriteToFile(results, testLabel);

    }

    [UnityTest]
    public IEnumerator SimpleCoroutineTest()
    {
        yield return behaviour.StartCoroutine(simpleCoroutine());

        IEnumerator simpleCoroutine()
        {
            CreateSimulationArray<double>(out var results);
            var watch = new Stopwatch();

            for (int i = 0; i < results.Length; i++)
            {
                watch.Restart();
                yield return null;
                watch.Stop();

                results[i] = watch.Elapsed.TotalMilliseconds;
            }

            var validResults = results.Skip(benchmarkManager.InitialThreshold).ToArray();
            double averageTime = validResults.Average();

            Debug.Log($"[SimpleCoroutine] Average elapsed time: {averageTime:0.00}ms");
            ConcludeTest(validResults, "SimpleCoroutine");


        }



    }
    [UnityTest]
    public IEnumerator SimpleAsyncTest()
    {
        var task = SimpleTaskAsync();
        while (!task.IsCompleted)
        {
            yield return null;
        }

    }
    private async Task SimpleTaskAsync(CancellationToken token = default)
    {
        CreateSimulationArray<double>(out var results);
        var watch = new Stopwatch();

        try
        {
            for (int i = 0; i < results.Length && !token.IsCancellationRequested; i++)
            {
                watch.Restart();
                await Task.Yield();
                watch.Stop();

                results[i] = watch.Elapsed.TotalMilliseconds;
            }
            //skip the warm-up
            var validResults = results.Skip(benchmarkManager.InitialThreshold).ToArray();
            //calculate the average
            double averageTime = validResults.Average();
            Debug.Log($"[SimpleAsync] Average elapsed time: {averageTime:0.00}ms");

            ConcludeTest(validResults, "SimpleAsync");
        }
        catch (Exception e) when (!(e is TaskCanceledException))
        {
            throw e;
        }
    }
    [UnityTest]
    public IEnumerator SimpleUniTaskTest()
    {
        return SimpleUniTaskAsync().ToCoroutine();
    }
    private async UniTask SimpleUniTaskAsync()
    {
        CreateSimulationArray<double>(out var results);
        var watch = new Stopwatch();

        try
        {
            for (int i = 0; i < results.Length; i++)
            {
                watch.Restart();
                await UniTask.Yield();
                watch.Stop();

                results[i] = watch.Elapsed.TotalMilliseconds;

            }
            var validResults = results.Skip(benchmarkManager.InitialThreshold).ToArray();
            double averageTime = validResults.Average();
            Debug.Log($"[SimpleUniTask] Average elapsed time: {averageTime:0.00}ms");

            ConcludeTest(validResults, "SimpleUniTask");
        }
        catch (Exception e) when (!(e is OperationCanceledException))
        {
            throw e;
        }
    }


    //stress test
    [UnityTest]
    public IEnumerator StressCoroutineTest()
    {
        yield return behaviour.StartCoroutine(StressTestRoutine());
    }
    private static IEnumerator EndNextFrameCoroutine()
    {
        yield return null;
    }
    private IEnumerator StressTestRoutine()
    {
        CreateSimulationArray<MemorySnapshot>(out var snapshotsResults);
        //initialize a 100 length array, in order to start 100 coroutines in the same time
        var coroutines = new Coroutine[10];
        int simulationTimes = 0;
        int length = snapshotsResults.Length;

        using (var profiler = new MemorySnapshot.MemoryProfiler())
        {
            while (simulationTimes < length)
            {
                yield return null;
                profiler.GetMemorySnapshot(out var first);

                //start 100 coroutines , simulate stress scenes
                for (int i = 0; i < 10; i++)
                {
                    coroutines[i] = behaviour.StartCoroutine(EndNextFrameCoroutine());

                }
                profiler.GetMemorySnapshot(out var final);
                yield return null;
                //stop 100 coroutines one by one
                for (int i = 0; i < 10; i++)
                {
                    behaviour.StopCoroutine(coroutines[i]);
                }
                snapshotsResults[simulationTimes] = final - first;
                simulationTimes++;
            }

        }
        snapshotsResults = snapshotsResults.Skip(benchmarkManager.InitialThreshold).ToArray();
        var averageSnapshot = new MemorySnapshot
        {
            TotalMemory = (long)snapshotsResults.Select(s => s.TotalMemory).Average(),
            GCMemory = (long)snapshotsResults.Select(s => s.GCMemory).Average(),
            GCAlloc = (long)snapshotsResults.Select(s => s.GCAlloc).Average()
        };
        Debug.Log($"[StressCoroutine] Average Memory Snapshot: {averageSnapshot}");
        ConcludeTest(snapshotsResults, "StressCoroutine");

    }

    [UnityTest]
    public IEnumerator StressAsyncTest()
    {
        yield return behaviour.StartCoroutine(StressAsyncTestRoutine());
    }

    private static async Task EndNextFrameAsync()
    {
        await Task.Yield();
    }

    private IEnumerator StressAsyncTestRoutine()
    {
        CreateSimulationArray<MemorySnapshot>(out var snapshotResults);
        var tasks = new Task[10];
        int simulationsRan = 0;
        int length = snapshotResults.Length;

        // 使用 MemorySnapshot. 前缀实例化嵌套的 Profiler
        using (var profiler = new MemorySnapshot.MemoryProfiler())
        {
            while (simulationsRan < length)
            {
                yield return null;
                profiler.GetMemorySnapshot(out var first);

                for (int i = 0; i < 10; i++)
                {
                    tasks[i] = EndNextFrameAsync();
                }

                profiler.GetMemorySnapshot(out var final);
                yield return null;

                Task.WaitAll(tasks);

                snapshotResults[simulationsRan] = final - first;
                simulationsRan++;
            }
        }

        snapshotResults = snapshotResults.Skip(benchmarkManager.InitialThreshold).ToArray();
        var averageSnapshot = new MemorySnapshot
        {
            TotalMemory = (long)snapshotResults.Select(s => s.TotalMemory).Average(),
            GCMemory = (long)snapshotResults.Select(s => s.GCMemory).Average(),
            GCAlloc = (long)snapshotResults.Select(s => s.GCAlloc).Average()
        };

        Debug.Log($"[StressAsync] Average Memory Snapshot: {averageSnapshot}");
        ConcludeTest(snapshotResults, "StressAsync");
    }
    [UnityTest]
    public IEnumerator StressUniTaskTest()
    {
        yield return StressUniTaskTestRoutine().ToCoroutine();
    }

    private static async UniTask EndNextFrameUniTask()
    {
        await UniTask.Yield();
    }

    private async UniTask StressUniTaskTestRoutine()
    {
        CreateSimulationArray<MemorySnapshot>(out var snapshotResults);
        var tasks = new UniTask[10];
        int simulationsRan = 0;
        int length = snapshotResults.Length;

        using (var profiler = new MemorySnapshot.MemoryProfiler())
        {
            while (simulationsRan < length)
            {
                // 等待一帧，与 Coroutine / Async 保持一致的测试节奏
                await UniTask.Yield();
                profiler.GetMemorySnapshot(out var first);

                // 同时启动 100 个 UniTask 模拟压力场景
                for (int i = 0; i < 10; i++)
                {
                    tasks[i] = EndNextFrameUniTask();
                }

                profiler.GetMemorySnapshot(out var final);
                await UniTask.Yield();

                // 等待所有 UniTask 完成
                await UniTask.WhenAll(tasks);

                snapshotResults[simulationsRan] = final - first;
                simulationsRan++;
            }
        }

        // 跳过预热数据并计算平均值
        snapshotResults = snapshotResults.Skip(benchmarkManager.InitialThreshold).ToArray();
        var averageSnapshot = new MemorySnapshot
        {
            TotalMemory = (long)snapshotResults.Select(s => s.TotalMemory).Average(),
            GCMemory = (long)snapshotResults.Select(s => s.GCMemory).Average(),
            GCAlloc = (long)snapshotResults.Select(s => s.GCAlloc).Average()
        };

        Debug.Log($"[StressUniTask] Average Memory Snapshot: {averageSnapshot}");
        ConcludeTest(snapshotResults, "StressUniTask");
    }



}










