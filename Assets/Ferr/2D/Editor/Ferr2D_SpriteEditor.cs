using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Ferr2D_Sprite))]
public class Ferr2D_SpriteEditor : Editor {

    void OnSceneGUI()
    {
        Ferr2D_Sprite sprite = (Ferr2D_Sprite)target;
        //sprite.meshScale.x = Handles.FreeMoveHandle(new Vector3(sprite.Width, 0, 0), Quaternion.identity,1,Vector3.zero, Handles.RectangleCap).x;
        if (GUI.changed)
        {
            
            sprite.Rebuild();
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUI.changed)
        {
            Ferr2D_Sprite sprite = (Ferr2D_Sprite)target;
            sprite.Rebuild();
        }
    }
}
