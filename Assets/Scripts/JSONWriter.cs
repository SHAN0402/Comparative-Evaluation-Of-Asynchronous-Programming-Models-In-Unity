

using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public static class JSONWriter
{
    public static void WriteToFile<T>(T[] collection, string filename)
    {
        //transfer the array to directionary
        var content = new Dictionary<int, T>();
        for (int i = 0; i < collection.Length; i++)
        {
            content.Add(i, collection[i]);
        }
        //
        var wrapper = new Dictionary<string, Dictionary<int, T>>()
        {
            {filename, content}
        };

        //transfer Dicectionary to JSON, Formatting.Indented -> make the file result more clearly
        var json = JsonConvert.SerializeObject(wrapper,Formatting.Indented);
        var path = Path.Combine(Application.dataPath, "..","Output",$"{filename}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        File.WriteAllText(path, json);
        //refresh the file
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}