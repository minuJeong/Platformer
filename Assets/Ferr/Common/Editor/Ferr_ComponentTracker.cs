using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using System.IO;

[AttributeUsage(AttributeTargets.Method)]
public class Ferr_TrackerRegistration : Attribute {
}

public partial class Ferr_ComponentTracker : AssetPostprocessor {
    static List      <Type>                                   trackTypes;
	static Dictionary<Type, Dictionary<string, List<object>>> components;

    const string cacheFile = "Assets/Ferr/Common/Data/cache.txt";

    static void           AddType<T>      () {
        trackTypes.Add(typeof(T));
	    components.Add(typeof(T), new Dictionary<string, List<object>>());
    }
    static void           RegisterTypes   () {
        Type         t       = typeof(Ferr_ComponentTracker);
        MethodInfo[] methods = t.GetMethods( BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
        for (int i = 0; i < methods.Length; i++) {
            if (methods[i].GetCustomAttributes(typeof(Ferr_TrackerRegistration), false).Length > 0) {
                if ((methods[i].Attributes & MethodAttributes.Static) <= 0) {
                    Debug.LogWarning("Ferr_ComponentTracker: [Ferr_TrackerRegistration] cannot be applied to non-static methods! - " + methods[i].Name);
                } else {
                    methods[i].Invoke(null, null);
                }
            }
        }
    }
    public static List<T> GetComponents<T>() {
        CheckInitialization();

        if (!components.ContainsKey(typeof(T))) {
            Debug.LogWarning("Ferr_ComponentTracker: type " + typeof(T).Name + " is not registered!");
            return new List<T>();
        }

        List<T> result = new List<T>();
	    foreach (var item in components[typeof(T)]) {
		    for (int i = 0; i < item.Value.Count; ++i) {
		    	result.Add((T)item.Value[i]);
		    }
        }
        return result;
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        CheckInitialization();

        bool dataDirty = false;

		// import new assets!
        for (int i = 0; i < importedAssets.Length; i++) {
            if (importedAssets[i].EndsWith(".prefab")) {
                GameObject go = AssetDatabase.LoadAssetAtPath(importedAssets[i], typeof(GameObject)) as GameObject;
                for (int t = 0; t < trackTypes.Count; t++) {
                    if (go != null && go.GetComponentsInChildren(trackTypes[t]) != null) {
                        AddComponent(trackTypes[t], importedAssets[i], go);
                        dataDirty = true;
                    }
                }
            }
        }
        
        // update renamed or moved assets
        for (int i = 0; i < movedAssets.Length; i++) {
        	if (movedAssets[i].EndsWith(".prefab")) {
        		RemoveComponent(movedFromAssetPaths[i]);
        		GameObject go = AssetDatabase.LoadAssetAtPath(movedAssets[i], typeof(GameObject)) as GameObject;
                for (int t = 0; t < trackTypes.Count; t++) {
                    if (go != null && go.GetComponentsInChildren(trackTypes[t]) != null) {
                        AddComponent(trackTypes[t], movedAssets[i], go);
                        dataDirty = true;
                    }
                }
        	} 
        }

		// delete 'em too
        for (int i = 0; i < deletedAssets.Length; i++) {
            if (deletedAssets[i].EndsWith(".prefab")) {
                RemoveComponent(deletedAssets[i]);
                dataDirty = true;
            }
        }

        if (dataDirty) SaveCache();
    }
    static void CheckInitialization   () {
        if (trackTypes == null || components == null) {
            trackTypes = new List<Type>();
	        components = new Dictionary<Type, Dictionary<string, List<object>>>();

            RegisterTypes();

            if (!HasCache()) {
                CreateCache();
            }
            LoadCache();
        }
    }

    static void AddComponent   (Type   aType, string aPath, GameObject go = null) {
        if (components[aType].ContainsKey(aPath)) return;
        if (go == null) go = AssetDatabase.LoadAssetAtPath(aPath, typeof(GameObject)) as GameObject;
        if (go == null) return;
        Component[] coms = go.GetComponentsInChildren(aType, true);
	    
	    List<object> comList = new List<object>();
        for (int i = 0; i < coms.Length; i++) {
	        comList.Add(coms[i]);
        }
	    components[aType].Add(aPath, comList);
    }
    static void RemoveComponent(string aPath) {
        foreach (var itemList in components) {
            foreach (var item in itemList.Value) {
                if (item.Key == aPath) {
                    itemList.Value.Remove(item.Key);
                    return;
                }
            }
        }
    }

    static bool HasCache   () {
        if (!File.Exists(cacheFile)) {
            return false;
        }
        return true;
    }
    static void LoadCache  () {
        StreamReader reader = new StreamReader(cacheFile);
        string[]     lines  = reader.ReadToEnd().Split('\n');

        Type currType = null;
        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i].Trim();
            if (line.StartsWith("[")) {
                line     = line.Replace("[", "").Replace("]", "");
                currType = GetTypeFromName(line);
            } else if (line != "") {
                AddComponent(currType, line);
            }
        }
        reader.Close();
    }
    static void SaveCache  () {
        if (!Directory.Exists(Path.GetDirectoryName(cacheFile))) {
            Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
        }

        string file = "";

        foreach (var itemList in components) {
            file += "\n[" + itemList.Key.Name + "]\n";
            foreach (var item in itemList.Value) {
                file += item.Key + "\n";
            }
        }

        StreamWriter writer = new StreamWriter(cacheFile);
        writer.Write(file);
        writer.Close();
    }
    static void CreateCache() {
        Debug.Log("One-time construction of the Ferr component cache, this may take a little while on large projects!");

        for (int i = 0; i < trackTypes.Count; i++) {
            List<object> coms = GetPrefabsOfTypeSlow(trackTypes[i]);
            for (int t = 0; t < coms.Count; t++) {
                Component com = coms[t] as Component;
                if (com != null) {
                    AddComponent(trackTypes[i], AssetDatabase.GetAssetPath(com.gameObject.GetInstanceID()), com.gameObject);
                }
            }
        }

        SaveCache();

        Debug.Log("Ferr component cache construction completed!");
    }

    static Type GetTypeFromName(string aName) {
        for (int i = 0; i < trackTypes.Count; i++) {
            if (trackTypes[i].Name == aName)
                return trackTypes[i];
        }
        return null;
    }

    public static List<T     > GetPrefabsOfTypeSlow<T>(          ) where T : Component {
        string[] fileNames = System.IO.Directory.GetFiles(Application.dataPath, "*.prefab", System.IO.SearchOption.AllDirectories);
        int pathLength = Application.dataPath.Length + 1;
        List<T> result = new List<T>();

        for (int i = fileNames.Length; i > 0; i--) {
            fileNames[i - 1] = "Assets\\" + fileNames[i - 1].Substring(pathLength);
            GameObject go = UnityEditor.AssetDatabase.LoadAssetAtPath(fileNames[i - 1], typeof(GameObject)) as GameObject;
            if (go != null) {
                T[] source = go.GetComponentsInChildren<T>(true);
                result.AddRange(source);
            }
        }
        return result;
    }
    public static List<object> GetPrefabsOfTypeSlow   (Type aType) {
        string[] fileNames = System.IO.Directory.GetFiles(Application.dataPath, "*.prefab", System.IO.SearchOption.AllDirectories);
        int pathLength = Application.dataPath.Length + 1;
        List<object> result = new List<object>();

        for (int i = fileNames.Length; i > 0; i--) {
            fileNames[i - 1] = "Assets\\" + fileNames[i - 1].Substring(pathLength);
            GameObject go = UnityEditor.AssetDatabase.LoadAssetAtPath(fileNames[i - 1], typeof(GameObject)) as GameObject;
            if (go != null) {
                Component[] source = go.GetComponentsInChildren(aType, true);
                if (source.Length > 0) {
                    result.AddRange(source);
                }
            }
        }
        return result;
    }
    public static void         RecreateCache          () {
        File.Delete(cacheFile);
        trackTypes = null;
        components = null;
        CheckInitialization();
    }
}