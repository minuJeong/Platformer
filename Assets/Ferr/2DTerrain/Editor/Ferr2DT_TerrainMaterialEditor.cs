using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(Ferr2DT_TerrainMaterial))]
public class Ferr2DT_TerrainMaterialEditor : Editor {

	public override void OnInspectorGUI() {
		#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
		Undo.RecordObject(target, "Modified Terrain Material");
		#else
        Undo.SetSnapshotTarget(target, "Modified Terrain Material");
		#endif
        
		Ferr2DT_TerrainMaterial mat = target as Ferr2DT_TerrainMaterial;

        mat.edgeMaterial = (Material)EditorGUILayout.ObjectField("Edge Material", mat.edgeMaterial, typeof(Material), true);
		Material newMat = (Material)EditorGUILayout.ObjectField("Fill Material", mat.fillMaterial, typeof(Material), true);
		if (mat.fillMaterial != newMat) {
			mat.fillMaterial = newMat;
			Ferr2DT_TerrainMaterialUtility.CheckMaterialRepeat(mat.fillMaterial);
		}
        if (mat.edgeMaterial == null) EditorGUILayout.HelpBox("Please add an edge material to enable the material editor!", MessageType.Warning);
        else {
            if (GUILayout.Button("Open Material Editor")) Ferr2DT_TerrainMaterialWindow.Show(mat);
        }
		if (GUI.changed) {
			EditorUtility.SetDirty(target);

            Ferr2DT_PathTerrain[] terrain = GameObject.FindObjectsOfType(typeof(Ferr2DT_PathTerrain)) as Ferr2DT_PathTerrain[];
            for (int i = 0; i < terrain.Length; i++)
            {
                if(terrain[i].TerrainMaterial == mat)
                    terrain[i].RecreatePath(true);
            }
		}
	}
}