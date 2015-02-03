using UnityEngine;
using UnityEditor;
using System.Collections;

public enum Ferr2DT_SnapMode {
    SnapGlobal,   // Snap to global coordinates
    SnapLocal,    // Snap to coordinates relative to transform
    SnapRelative  // Default, Unity-like snapping
}

public static class Ferr_Menu {
    static bool             prefsLoaded = false;
    static bool             hideMeshes  = true;
    static float            pathScale   = 1;
	static Ferr2DT_SnapMode snapMode    = Ferr2DT_SnapMode.SnapRelative;
	static float            smartSnapDist = 0.4f;
    static int              updateTerrainSkipFrames = 0;
    static int              ppu           = 64;
	static bool             smoothTerrain = false;

    public static bool HideMeshes {
        get { LoadPrefs(); return hideMeshes; }
        set { hideMeshes = value; SavePrefs(); }
    }
    public static float PathScale {
        get { LoadPrefs(); return pathScale;  }
    }
    public static Ferr2DT_SnapMode SnapMode {
        get { LoadPrefs(); return snapMode;   }
        set { snapMode = value; SavePrefs(); }
    }
	public static float SmartSnapDist {
		get { LoadPrefs(); return smartSnapDist;   }
		set { smartSnapDist = value; SavePrefs(); }
	}
    public static int UpdateTerrainSkipFrames {
        get { LoadPrefs(); return updateTerrainSkipFrames; }
    }
    public static int PPU {
        get { LoadPrefs(); return ppu; }
    }
    public static bool SmoothTerrain {
        get { LoadPrefs(); return smoothTerrain; }
    }

    [PreferenceItem("Ferr")]
    static void Ferr2DT_PreferencesGUI() 
    {
        LoadPrefs();

        pathScale  = EditorGUILayout.FloatField   ("Path vertex scale",        pathScale );
	    updateTerrainSkipFrames = EditorGUILayout.IntField("Update Terrain Every X Frames", updateTerrainSkipFrames);
	    smartSnapDist = EditorGUILayout.FloatField("Smart Snap Distance",      smartSnapDist);
        ppu           = EditorGUILayout.IntField  ("Default PPU",              ppu);
	    smoothTerrain = EditorGUILayout.Toggle    ("Default smoothed terrain", smoothTerrain);

        if (GUI.changed) {
            SavePrefs();
        }
    }

    static void LoadPrefs() {
        if (prefsLoaded) return;
        prefsLoaded   = true;
        hideMeshes    = EditorPrefs.GetBool ("Ferr_hideMeshes", true);
        pathScale     = EditorPrefs.GetFloat("Ferr_pathScale",  1   );
        updateTerrainSkipFrames = EditorPrefs.GetInt("Ferr_updateTerrainAlways", 0);
        snapMode      = (Ferr2DT_SnapMode)EditorPrefs.GetInt("Ferr_snapMode", (int)Ferr2DT_SnapMode.SnapRelative);
        ppu           = EditorPrefs.GetInt  ("Ferr_ppu", 64);
	    smoothTerrain = EditorPrefs.GetBool ("Ferr_smoothTerrain", false);
	    smartSnapDist = EditorPrefs.GetFloat("Ferr_smartSnapDist", 0.4f);
    }
    static void SavePrefs() {
        if (!prefsLoaded) return;
        EditorPrefs.SetBool ("Ferr_hideMeshes", hideMeshes);
        EditorPrefs.SetFloat("Ferr_pathScale",  pathScale );
        EditorPrefs.SetInt  ("Ferr_updateTerrainAlways", updateTerrainSkipFrames);
        EditorPrefs.SetInt  ("Ferr_snapMode",   (int)snapMode);
        EditorPrefs.SetInt  ("Ferr_ppu",        ppu       );
	    EditorPrefs.SetBool ("Ferr_smoothTerrain", smoothTerrain);
	    EditorPrefs.SetFloat("Ferr_smartSnapDist", smartSnapDist);
    }
}
