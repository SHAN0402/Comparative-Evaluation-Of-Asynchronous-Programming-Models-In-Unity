using UnityEngine;
[CreateAssetMenu(fileName ="BenchmarkSettings",menuName ="StreamingBenchmark/Settings")]
public class BenchmarkSettings : ScriptableObject
{
    public int InitialThreshold => initialThreshold;
    public int SimulationCount => simulationCount;

    [SerializeField, Range(100,10000)]
    //the total number of simulation iterations to run for accurate benchmarking
    int simulationCount= 1000;
    [SerializeField, Range(0,20)]
    //the number of initial simulation cycles to discard as warm-up data to avoid performance noise
    int initialThreshold=10;
}