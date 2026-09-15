using System;
using Unity.Profiling;
using UnityEditor.MemoryProfiler;


public struct MemorySnapshot
{
    public long TotalMemory;
    public long GCMemory;
    public long GCAlloc;

    public static MemorySnapshot operator -(MemorySnapshot a, MemorySnapshot b)
    {
        MemorySnapshot result = new MemorySnapshot();

        result.TotalMemory = Math.Abs(a.TotalMemory-b.TotalMemory);
        result.GCMemory = Math.Abs(a.GCMemory-b.GCMemory);
        result.GCAlloc = Math.Abs(a.GCAlloc-b.GCAlloc);

        return result;
    }
    public override string ToString()
    {
        return "Total Memory: " + TotalMemory + " bytes | " +
               "GC Used Memory: " + GCMemory + " bytes | " +
               "GC Allocated In Frame: " + GCAlloc + " bytes";
    }

    public class MemoryProfiler : IDisposable
    {
        public readonly ProfilerRecorder TotalMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory,"Total Used Memory", 1, ProfilerRecorderOptions.Default |ProfilerRecorderOptions.StartImmediately);
        public readonly ProfilerRecorder GCMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory", 1, ProfilerRecorderOptions.Default | ProfilerRecorderOptions.StartImmediately);
        public readonly ProfilerRecorder GCAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1, ProfilerRecorderOptions.Default | ProfilerRecorderOptions.StartImmediately);

        //safty control disposed == true means turning off
        bool disposed;

        //get current state
        public void GetMemorySnapshot(out MemorySnapshot snapshot)
        {
            snapshot = new MemorySnapshot
            {
                TotalMemory = TotalMemoryRecorder.CurrentValue,
                GCMemory = GCMemoryRecorder.CurrentValue,
                GCAlloc = GCAllocRecorder.CurrentValue,
            };
        }
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            TotalMemoryRecorder.Dispose();
            GCMemoryRecorder.Dispose();
            GCAllocRecorder.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
