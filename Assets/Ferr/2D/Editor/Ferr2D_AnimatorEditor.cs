using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Ferr2D_Animator))]
public class Ferr2D_AnimatorEditor : UnityEditor.Editor {
    int activeAnim;
    string tName;
    public override void OnInspectorGUI()
    {

		#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
		Undo.RecordObject(target, "Modified Animation");
		#else
        Undo.SetSnapshotTarget(target, "Modified Animation");
		#endif

        Ferr2D_Animator anim = (Ferr2D_Animator)target;

        anim.cellSize = EditorGUILayout.Vector2Field("Spritesheet Cell Size", anim.cellSize);
        anim.offset   = EditorGUILayout.Vector2Field("Spritesheet offset",    anim.offset  );

        EditorGUILayout.BeginHorizontal();
        tName = EditorGUILayout.TextField("New", tName);
        if (GUILayout.Button("Add"))
        {
            anim.animations.Add(new Ferr2D_Animation(tName));
            activeAnim = anim.animations.Count-1;
            tName = "";
        }
        EditorGUILayout.EndHorizontal();

        List<string> keys = GetNames(anim);
        if (keys.Count > 0 && activeAnim < anim.animations.Count)
        {
            activeAnim = EditorGUILayout.Popup(activeAnim, keys.ToArray());
        }

        if (activeAnim < anim.animations.Count)
        {
            Ferr2D_Animation curr = anim.animations[activeAnim];
            curr.loop = (Ferr2D_LoopMode)EditorGUILayout.EnumPopup("Loop type", curr.loop);
            if (curr.loop == Ferr2D_LoopMode.Next && keys.Count > 0) curr.next = keys[EditorGUILayout.Popup(anim.HasAnim(curr.next) ? keys.IndexOf(curr.next) : 0, keys.ToArray())];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Frame Index", GUILayout.Width(80f));
            EditorGUILayout.LabelField("Duration(s)", GUILayout.Width(80f));
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < curr.frames.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                curr.frames[i].index    = EditorGUILayout.IntField(curr.frames[i].index, GUILayout.Width(80f));
                curr.frames[i].duration = EditorGUILayout.FloatField(curr.frames[i].duration, GUILayout.Width(80f));
                if (GUILayout.Button("+")) curr.frames.Insert(i, new Ferr2D_Frame());
                if (GUILayout.Button("x")) { curr.frames.RemoveAt(i); i--; }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+")) curr.frames.Add(new Ferr2D_Frame());
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty (target);
        }
    }
    public List<string> GetNames(Ferr2D_Animator aAnim)
    {
        List<string> result = new List<string>( aAnim.animations.Count );
        for (int i = 0; i < aAnim.animations.Count; i++)
        {
            result.Add(aAnim.animations[i].name);
        }
        return result;
    }
}
