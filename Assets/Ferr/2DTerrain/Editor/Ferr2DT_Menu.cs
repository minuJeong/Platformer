using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;

public class Ferr2DT_Menu {
    [MenuItem("GameObject/Create Ferr2D Terrain/Create Physical 2D Terrain %t", false, 0)]
    static void MenuAddPhysicalTerrain() {
        Ferr2DT_MaterialSelector.Show(AddPhysicalTerrain);
    }
    static void AddPhysicalTerrain(Ferr2DT_TerrainMaterial aMaterial) {
        GameObject obj = CreateBaseTerrain(aMaterial, true);
        Selection.activeGameObject = obj;
        EditorGUIUtility.PingObject(obj);
    }


    [MenuItem("GameObject/Create Ferr2D Terrain/Create Decorative 2D Terrain %#t", false, 0)]
    static void MenuAddDecoTerrain() {
        Ferr2DT_MaterialSelector.Show(AddDecoTerrain);
    }
    static void AddDecoTerrain(Ferr2DT_TerrainMaterial aMaterial) {
        GameObject obj = CreateBaseTerrain(aMaterial, false);
        Selection.activeGameObject = obj;
        EditorGUIUtility.PingObject(obj);
    }
    static GameObject CreateBaseTerrain(Ferr2DT_TerrainMaterial aMaterial, bool aCreateColliders) {
        GameObject          obj     = new GameObject("New Terrain");
        Ferr2D_Path         path    = obj.AddComponent<Ferr2D_Path        >();
        Ferr2DT_PathTerrain terrain = obj.AddComponent<Ferr2DT_PathTerrain>();

        bool hasEdges = aMaterial.Has(Ferr2DT_TerrainDirection.Bottom) ||
                        aMaterial.Has(Ferr2DT_TerrainDirection.Left) ||
                        aMaterial.Has(Ferr2DT_TerrainDirection.Right);

        if (hasEdges) {
            path.Add(new Vector2(-6, 0));
            path.Add(new Vector2(-6, 6));
            path.Add(new Vector2( 6, 6));
            path.Add(new Vector2( 6, 0));
            path.closed = true;
        } else {
            path.Add(new Vector2(-6, 6));
            path.Add(new Vector2( 6, 6));
            terrain.splitCorners = false;
            path.closed = false;
        }
        
        if (aMaterial.fillMaterial != null) {
            if (hasEdges) {
                terrain.fill = Ferr2DT_FillMode.Closed;
            } else {
                terrain.fill = Ferr2DT_FillMode.Skirt;
                terrain.splitCorners = true;
            }
        } else {
            terrain.fill = Ferr2DT_FillMode.None;
        }
        terrain.smoothPath     = Ferr_Menu.SmoothTerrain;
        terrain.pixelsPerUnit  = Ferr_Menu.PPU;
        terrain.createCollider = aCreateColliders;
        terrain.SetMaterial (aMaterial);
        terrain.RecreatePath(true);

        obj.isStatic           = true;
        obj.transform.position = GetSpawnPos();

        return obj;
    }
	

	[MenuItem("GameObject/Create Ferr2D Terrain/Create Terrain Material", false, 11)]
    static void MenuAddTerrainMaterial() {
        AddTerrainMaterial(GetCurrentPath());
    }
    [MenuItem("Assets/Create/Ferr2D Terrain Material", false, 101)]
    static void ContextAddTerrainMaterial() {
        AddTerrainMaterial(GetCurrentPath());
    }
    static void AddTerrainMaterial(string aFolder) {
        GameObject obj = new GameObject("New Terrain Material");
        obj.AddComponent<Ferr2DT_TerrainMaterial>();
        string name = aFolder + "/NewTerrainMaterial.prefab";
        int id = 0;
        while (AssetDatabase.LoadAssetAtPath(name, typeof(GameObject)) != null) {
            id += 1;
            name = aFolder + "/NewTerrainMaterial" + id + ".prefab";
        }
        GameObject prefab = PrefabUtility.CreatePrefab(name, obj);
        GameObject.DestroyImmediate(obj);

        Selection.activeGameObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }


    [MenuItem("Assets/Prebuild Ferr2D Terrain", false, 101)]
    static void MenuPrebuildTerrain() {
        Debug.Log("Prebuilding...");
        Ferr2DT_Builder.SaveTerrains();
        Debug.Log("Prebuilding complete.");
    }

    [MenuItem("Assets/Rebuild Ferr2D Component Cache", false, 101)]
    static void MenuRebuildComponentCache() {
        Ferr_ComponentTracker.RecreateCache();
    }

    static Vector3 GetSpawnPos() {
        Plane   plane  = new Plane(new Vector3(0, 0, -1), 0);
        float   dist   = 0;
        Vector3 result = new Vector3(0, 0, 0);
        //Ray     ray    = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Ray ray = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
        if (plane.Raycast(ray, out dist)) {
            result = ray.GetPoint(dist);
        }
        return new Vector3(result.x, result.y, 0);
    }
    static string  GetCurrentPath() {
        string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (Path.GetExtension(path) != "") path = Path.GetDirectoryName(path);
        if (path                    == "") path = "Assets";
        return path;
    }
}
