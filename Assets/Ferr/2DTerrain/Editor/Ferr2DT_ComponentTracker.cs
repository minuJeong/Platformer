using UnityEngine;
using UnityEditor;

public partial class Ferr_ComponentTracker  {
    [Ferr_TrackerRegistration]
    static void RegisterFerr2DT() {
        AddType<Ferr2DT_TerrainMaterial>();
        AddType<Ferr2DT_PathTerrain>();
    }
}