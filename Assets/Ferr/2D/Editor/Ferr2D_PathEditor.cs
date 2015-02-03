using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(Ferr2D_Path))]
public class Ferr2D_PathEditor : Editor { 
	Texture2D texMinus           = Ferr_EditorTools.GetGizmo("dot-minus.png"         );
	Texture2D texMinusSelected   = Ferr_EditorTools.GetGizmo("dot-minus-selected.png");
	Texture2D texDot             = Ferr_EditorTools.GetGizmo("dot.png"               );
	Texture2D texDotSnap         = Ferr_EditorTools.GetGizmo("dot-snap.png"          );
	Texture2D texDotPlus         = Ferr_EditorTools.GetGizmo("dot-plus.png"          );
	Texture2D texDotSelected     = Ferr_EditorTools.GetGizmo("dot-selected.png"      );
	Texture2D texDotSelectedSnap = Ferr_EditorTools.GetGizmo("dot-selected-snap.png" );
	
	Texture2D texLeft   = Ferr_EditorTools.GetGizmo("dot-left.png"  );
	Texture2D texRight  = Ferr_EditorTools.GetGizmo("dot-right.png" );
	Texture2D texTop    = Ferr_EditorTools.GetGizmo("dot-top.png"   );
	Texture2D texBottom = Ferr_EditorTools.GetGizmo("dot-down.png"  );
	Texture2D texAuto   = Ferr_EditorTools.GetGizmo("dot-auto.png"  );
	Texture2D texReset  = Ferr_EditorTools.GetGizmo("dot-reset.png" );
	Texture2D texScale  = Ferr_EditorTools.GetGizmo("dot-scale.png" );
	
	public static Action  OnChanged = null;
	public static Vector3 offset    = new Vector3(0, 0, -0.0f);
	static int updateCount    = 0;
	bool       showVerts      = false;
	bool       prevChanged    = false;
	List<int>  selectedPoints = new List<int>();
	Vector2    dragStart;
	bool       drag           = false;
	
	private         void OnEnable      () {
		selectedPoints.Clear();
	}
	private         void OnSceneGUI    () {
		Ferr2D_Path  path      = (Ferr2D_Path)target;
		GUIStyle     iconStyle = new GUIStyle();
		iconStyle.alignment    = TextAnchor.MiddleCenter;
		
		// setup undoing things
		#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
		Undo.RecordObject(target, "Modified Path");
		#else
		Undo.SetSnapshotTarget(target, "Modified Path");
		Undo.CreateSnapshot();
		#endif
		
        // draw the path line
		if (Event.current.type == EventType.repaint)
			DoPath(path);
		
		// Check for drag-selecting multiple points
		DragSelect(path);
		
        // do adding verts in when the shift key is down!
		if (Event.current.shift && !Event.current.control) {
			DoShiftAdd(path, iconStyle);
		}
		
        // draw and interact with all the path handles
		DoHandles(path, iconStyle);
		
		// update everything that relies on this path, if the GUI changed
		if (GUI.changed) {
			#if (UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
			Undo.RegisterSnapshot();
			#endif
			UpdateDependentsSmart(path, false, false);
			EditorUtility.SetDirty (target);
			prevChanged = true;
		} else if (Event.current.type == EventType.used) {
			if (prevChanged == true) {
				UpdateDependentsSmart(path, false, true);
			}
			prevChanged = false;
		}
	}
	public override void OnInspectorGUI() {
		#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
		Undo.RecordObject(target, "Modified Path");
		#else
		Undo.SetSnapshotTarget(target, "Modified Path");
		#endif
		
		Ferr2D_Path path = (Ferr2D_Path)target;
		
		// if this was an undo, refresh stuff too
		if (Event.current.type == EventType.ValidateCommand) {
			switch (Event.current.commandName) {
			case "UndoRedoPerformed":
				
				path.UpdateDependants(true);
				if (OnChanged != null) OnChanged();
				return;
			}
		}
		
		path.closed = EditorGUILayout.Toggle ("Closed", path.closed);
		if (path)
			
        // display the path verts list info
			showVerts   = EditorGUILayout.Foldout(showVerts, "Path Vertices");
		EditorGUI.indentLevel = 2;
		if (showVerts)
		{
			int size = EditorGUILayout.IntField("Count: ", path.pathVerts.Count);
			while (path.pathVerts.Count > size) path.pathVerts.RemoveAt(path.pathVerts.Count - 1);
			while (path.pathVerts.Count < size) path.pathVerts.Add     (new Vector2(0, 0));
		}
        // draw all the verts! Long list~
		for (int i = 0; showVerts && i < path.pathVerts.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("#" + i, GUILayout.Width(60));
			path.pathVerts[i] = new Vector2(
				EditorGUILayout.FloatField(path.pathVerts[i].x),
				EditorGUILayout.FloatField(path.pathVerts[i].y));
			EditorGUILayout.EndHorizontal();
		}
		
        // button for updating the origin of the object
		if (GUILayout.Button("Center Position")) path.ReCenter();
		
		bool updateClosed = false;
		Ferr2DT_PathTerrain terrain = path.GetComponent<Ferr2DT_PathTerrain>();
		if (!path.closed && terrain != null && (terrain.fill == Ferr2DT_FillMode.Closed || terrain.fill == Ferr2DT_FillMode.InvertedClosed || terrain.fill == Ferr2DT_FillMode.FillOnlyClosed)) {
			path.closed  = true;
			updateClosed = true;
		}
		if (path.closed && (terrain.fill == Ferr2DT_FillMode.FillOnlySkirt || terrain.fill == Ferr2DT_FillMode.Skirt)) {
			path.closed  = false;
			updateClosed = true;
		}
		
        // update dependants when it changes
		if (GUI.changed || updateClosed)
		{
			path.UpdateDependants(false);
			EditorUtility.SetDirty(target);
		}
	}
	
	private void    UpdateDependentsSmart(Ferr2D_Path aPath, bool aForce, bool aFullUpdate) {
		if (aForce || Ferr_Menu.UpdateTerrainSkipFrames == 0 || updateCount % Ferr_Menu.UpdateTerrainSkipFrames == 0) {
			aPath.UpdateDependants(aFullUpdate);
			if (Application.isPlaying) aPath.UpdateColliders();
			if (OnChanged != null) OnChanged();
		}
		updateCount += 1;
	}
	private void    DragSelect           (Ferr2D_Path path) {
		
		if (Event.current.type == EventType.repaint) {
			if (drag) {
				Vector3 pt1 = HandleUtility.GUIPointToWorldRay(dragStart).GetPoint(0.2f);
				Vector3 pt2 = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).GetPoint(0.2f);
				Vector3 pt3 = HandleUtility.GUIPointToWorldRay(new Vector2(dragStart.x, Event.current.mousePosition.y)).GetPoint(0.2f);
				Vector3 pt4 = HandleUtility.GUIPointToWorldRay(new Vector2(Event.current.mousePosition.x, dragStart.y)).GetPoint(0.2f);
				Handles.DrawSolidRectangleWithOutline(new Vector3[] { pt1, pt3, pt2, pt4 }, new Color(0, 0.5f, 0.25f, 0.25f), new Color(0, 0.5f, 0.25f, 0.5f));
			}
		}
		
		if (Event.current.shift && Event.current.control) {
			switch(Event.current.type) {
			case EventType.mouseDrag:
				SceneView.RepaintAll();
				break;
			case EventType.mouseMove:
				SceneView.RepaintAll();
				break;
			case EventType.mouseDown:
				dragStart = Event.current.mousePosition;
				drag = true;
				
				break;
			case EventType.mouseUp:
				Vector2 dragEnd = Event.current.mousePosition;
				selectedPoints.Clear();
				for	(int i=0;i<path.pathVerts.Count;i+=1) {
					float left   = Mathf.Min(dragStart.x, dragStart.x + (dragEnd.x - dragStart.x));
					float right  = Mathf.Max(dragStart.x, dragStart.x + (dragEnd.x - dragStart.x));
					float top    = Mathf.Min(dragStart.y, dragStart.y + (dragEnd.y - dragStart.y));
					float bottom = Mathf.Max(dragStart.y, dragStart.y + (dragEnd.y - dragStart.y));
					
					Rect r = new Rect(left, top, right-left, bottom-top);
					if (r.Contains(HandleUtility.WorldToGUIPoint(path.transform.TransformPoint( path.pathVerts[i]) ) )) {
						selectedPoints.Add(i);
					}
				}
				
				HandleUtility.AddDefaultControl(0);
				drag = false;
				Repaint();
				break;
			case EventType.layout :
				HandleUtility.AddDefaultControl(GetHashCode());
				break;
			}
		} else if (drag == true) {
			drag = false;
			Repaint();
		}
	}
	private void    DoHandles            (Ferr2D_Path path, GUIStyle iconStyle)
	{
		Ferr2DT_PathTerrain terrain = path.gameObject.GetComponent<Ferr2DT_PathTerrain>();
		if (terrain) terrain.MatchOverrides();
		Quaternion inv = Quaternion.Inverse(path.transform.rotation);
		
		Handles.color = new Color(1, 1, 1, 0);
		for (int i = 0; i < path.pathVerts.Count; i++)
		{
			Vector3 pos        = path.transform.position + path.transform.rotation * Vector3.Scale(new Vector3(path.pathVerts[i].x, path.pathVerts[i].y, 0), path.transform.localScale);
			Vector3 posOff     = pos + offset;
			bool    isSelected = selectedPoints.Contains(i);
			
            // check if we want to remove points
			if (Event.current.alt)
			{
				float handleScale = HandleScale(posOff);
				if (IsVisible(posOff)) {
					SetScale(posOff, texMinus, ref iconStyle);
					Handles.Label(posOff, new GUIContent((isSelected || selectedPoints.Count <= 0) ? texMinusSelected : texMinus), iconStyle);
				}
				
				if (Handles.Button(posOff, SceneView.lastActiveSceneView.camera.transform.rotation, handleScale, handleScale, Handles.CircleCap))
				{
					if (!isSelected) {
						selectedPoints.Clear();
						selectedPoints.Add(i);
					}
					for (int s = 0; s < selectedPoints.Count; s++) {
						if (terrain) terrain.RemovePoint(selectedPoints[s]);
						else  path.pathVerts.RemoveAt   (selectedPoints[s]);
						if (selectedPoints[s] <= i) i--;
						
						for (int u = 0; u < selectedPoints.Count; u++) {
							if (selectedPoints[u] > selectedPoints[s]) selectedPoints[u] -= 1;
						}
					}
					selectedPoints.Clear();
					GUI.changed = true;
				} else if (Ferr2DT_SceneOverlay.editMode != Ferr2DT_EditMode.None) {
					
					if (terrain && i+1 < path.pathVerts.Count) {
						float   scale     = handleScale * 0.5f;
						Vector3 dirOff    = GetTickerOffset(path, pos, i);
						Vector3 posDirOff = posOff + dirOff;
						
						if (IsVisible(posDirOff)) {
							Texture2D icon = null;
							if      (Ferr2DT_SceneOverlay.editMode == Ferr2DT_EditMode.Override) icon = GetDirIcon(terrain.directionOverrides[i]);
							else if (Ferr2DT_SceneOverlay.editMode == Ferr2DT_EditMode.Scale   ) icon = texScale;
							if      (Event.current.alt)                                          icon = texReset;
							
							SetScale(posDirOff, icon, ref iconStyle, 0.5f);
							Handles.Label(posDirOff, new GUIContent(icon), iconStyle);
							
							if (Handles.Button(posDirOff, SceneView.lastActiveSceneView.camera.transform.rotation, scale, scale, Handles.CircleCap)) {
								if (selectedPoints.Count < 2 || isSelected == false) {
									selectedPoints.Clear();
									selectedPoints.Add(i);
									isSelected = true;
								}
								
								for (int s = 0; s < selectedPoints.Count; s++) {
									if (Ferr2DT_SceneOverlay.editMode == Ferr2DT_EditMode.Override)
										terrain.directionOverrides[selectedPoints[s]] = Ferr2DT_TerrainDirection.None;
									else if (Ferr2DT_SceneOverlay.editMode == Ferr2DT_EditMode.Scale)
										terrain.vertScales        [selectedPoints[s]] = 1;
								}
								GUI.changed = true;
							}
						}
					}
				}
			} else {
                // check for moving the point
				Texture2D tex = null;
				if (Event.current.control) tex = isSelected ? texDotSelectedSnap : texDotSnap;
				else                       tex = isSelected ? texDotSelected     : texDot;
				
				if (IsVisible(posOff)) {
					SetScale(posOff, texMinus, ref iconStyle);
					Handles.Label(posOff, new GUIContent(tex), iconStyle);
				}
				Vector3 snap   = Event.current.control && Ferr_Menu.SnapMode == Ferr2DT_SnapMode.SnapRelative ? new Vector3(EditorPrefs.GetFloat("MoveSnapX"), EditorPrefs.GetFloat("MoveSnapY"), EditorPrefs.GetFloat("MoveSnapZ")) : Vector3.zero;
				Vector3 result = Handles.FreeMoveHandle(
					posOff,
					SceneView.lastActiveSceneView.camera.transform.rotation,
					HandleScale(pos+offset),
					snap, 
					Handles.CircleCap);
				
				if (result != posOff) {
					
					if (selectedPoints.Count < 2 || isSelected == false) {
						selectedPoints.Clear();
						selectedPoints.Add(i);
						isSelected = true;
					}
					
					if (!(Event.current.control && Ferr_Menu.SnapMode == Ferr2DT_SnapMode.SnapRelative))
						result = GetRealPoint(result, path.transform.position.z);
					Vector3 global = (result - offset);
					if (Event.current.control && Ferr_Menu.SnapMode == Ferr2DT_SnapMode.SnapGlobal) global = SnapVector(global);
					Vector3 local  = inv * (global - path.transform.position);
					if (Event.current.control && Ferr_Menu.SnapMode == Ferr2DT_SnapMode.SnapLocal ) local  = SnapVector(local);
					if (!Event.current.control && Ferr2DT_SceneOverlay.smartSnap) {
						local = SmartSnap(local, path.pathVerts, selectedPoints, Ferr_Menu.SmartSnapDist);
					}
					
					Vector2 relative = new Vector2(
						local.x / path.transform.localScale.x,
						local.y / path.transform.localScale.y) - path.pathVerts[i];
					
					for (int s = 0; s < selectedPoints.Count; s++) {
						path.pathVerts[selectedPoints[s]] += relative;
					}
				}
				
                // if using terrain, check to see for any edge overrides
				if (Ferr2DT_SceneOverlay.showIndices) {
					Vector3 labelPos = posOff + (Vector3)Ferr2D_Path.GetNormal(path.pathVerts, i, path.closed);
					Handles.color    = Color.white;
					Handles.Label(labelPos, "" + i);
					Handles.color    = new Color(1, 1, 1, 0);
				}
				
				if (terrain) {// && i+1 < path.pathVerts.Count) {
					float   scale     = HandleScale    (pos+offset) * 0.5f;
					Vector3 dirOff    = GetTickerOffset(path, pos, i);
					Vector3 posDirOff = posOff + dirOff;
					
					if (Ferr2DT_SceneOverlay.editMode == Ferr2DT_EditMode.Override && i+1 < path.pathVerts.Count) {
						if (IsVisible(posDirOff)) {
							SetScale(posDirOff, texMinus, ref iconStyle, 0.5f);
							Handles.Label(posDirOff, new GUIContent(Event.current.alt ? texReset : GetDirIcon(terrain.directionOverrides[i])), iconStyle);
							
							if (Handles.Button(posDirOff, SceneView.lastActiveSceneView.camera.transform.rotation, scale, scale, Handles.CircleCap)) {
								if (selectedPoints.Count < 2 || isSelected == false) {
									selectedPoints.Clear();
									selectedPoints.Add(i);
									isSelected = true;
								}
								
								Ferr2DT_TerrainDirection dir = NextDir(terrain.directionOverrides[i]);
								for (int s = 0; s < selectedPoints.Count; s++) {
									terrain.directionOverrides[selectedPoints[s]] = dir;
								}
								GUI.changed = true;
							}
						}
						
					} else if (Ferr2DT_SceneOverlay.editMode == Ferr2DT_EditMode.Scale) {
						if (IsVisible(posDirOff)) {
							SetScale(posDirOff, texMinus, ref iconStyle, 0.5f);
							Handles.Label(posDirOff, new GUIContent(Event.current.alt ? texReset : texScale), iconStyle);
							
							Vector3 scaleMove = Handles.FreeMoveHandle(posDirOff, SceneView.lastActiveSceneView.camera.transform.rotation, scale, Vector3.zero, Handles.CircleCap);
							float   scaleAmt  = scaleMove.y - posDirOff.y;
							if (Mathf.Abs(scaleAmt) > 0.01f ) {
								if (selectedPoints.Count < 2 || isSelected == false) {
									selectedPoints.Clear();
									selectedPoints.Add(i);
									isSelected = true;
								}
								
								float vertScale = terrain.vertScales[i] - Event.current.delta.y / 100f;
								vertScale = Mathf.Clamp(vertScale, 0.2f, 3f);
								for (int s = 0; s < selectedPoints.Count; s++) {
									terrain.vertScales[selectedPoints[s]] = vertScale;
								}
								GUI.changed = true;
							}
						}
					}
				}
				
                // make sure we can add new point at the midpoints!
				if (i + 1 < path.pathVerts.Count || path.closed == true) {
					int     index       = path.closed && i + 1 == path.pathVerts.Count ? 0 : i + 1;
					Vector3 pos2        = path.transform.position + path.transform.rotation * Vector3.Scale(new Vector3(path.pathVerts[index].x, path.pathVerts[index].y, 0), path.transform.localScale);
					Vector3 mid         = (pos + pos2) / 2;
					float   handleScale = HandleScale(mid + offset);
					
					if (Handles.Button(mid + offset, SceneView.lastActiveSceneView.camera.transform.rotation, handleScale, handleScale, Handles.CircleCap)) {
						Vector2 pt = inv * new Vector2((mid.x - path.transform.position.x) / path.transform.localScale.x, (mid.y - path.transform.position.y) / path.transform.localScale.y);
						if (terrain)
							terrain.AddPoint(pt, index);
						else
							path.pathVerts.Insert(index, pt);
					}
					if (IsVisible(mid + offset)) {
						SetScale(mid + offset, texDotPlus, ref iconStyle);
						Handles.Label(mid + offset, new GUIContent(texDotPlus), iconStyle);
					}
				}
			}
		}
		
		if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Delete && selectedPoints.Count > 0) {
			for (int s = 0; s < selectedPoints.Count; s++) {
				if (terrain) terrain.RemovePoint(selectedPoints[s]);
				else  path.pathVerts.RemoveAt   (selectedPoints[s]);
				
				for (int u = 0; u < selectedPoints.Count; u++) {
					if (selectedPoints[u] > selectedPoints[s]) selectedPoints[u] -= 1;
				}
			}
			selectedPoints.Clear();
			GUI.changed = true;
			Event.current.Use();
		}
	}
	private Vector3 GetTickerOffset      (Ferr2D_Path path, Vector3  aRootPos, int aIndex) {
		float   scale  = HandleScale(aRootPos+offset) * 0.5f;
		Vector3 result = Vector3.zero;
		
		int     index  = (aIndex + 1) % path.pathVerts.Count;
		Vector3 delta  = Vector3.Normalize(path.pathVerts[index] - path.pathVerts[aIndex]);
		Vector3 norm   = new Vector3(-delta.y, delta.x, 0);
		result = delta * scale * 3 + new Vector3(norm.x, norm.y, 0) * scale * 2;

		return result;
	}
	private void    DoShiftAdd           (Ferr2D_Path path, GUIStyle iconStyle)
	{
		Ferr2DT_PathTerrain terrain  = path.gameObject.GetComponent<Ferr2DT_PathTerrain>();
		Quaternion          inv      = Quaternion.Inverse(path.transform.rotation);
		Vector2             pos      = GetMousePos(Event.current.mousePosition, path.transform.position.z) - new Vector2(path.transform.position.x, path.transform.position.y);
		bool                hasDummy = path.pathVerts.Count <= 0;
		
		if (hasDummy) path.pathVerts.Add(Vector2.zero);
		
		int   closestID  = path.GetClosestSeg(inv * new Vector2(pos.x / path.transform.localScale.x, pos.y / path.transform.localScale.y));
		int   secondID   = closestID + 1 >= path.Count ? 0 : closestID + 1;
		
		float firstDist  = Vector2.Distance(pos, path.pathVerts[closestID]);
		float secondDist = Vector2.Distance(pos, path.pathVerts[secondID]);
		
		Vector3 local  = pos;
		if (Event.current.control && Ferr_Menu.SnapMode == Ferr2DT_SnapMode.SnapLocal ) local  = SnapVector(local );
		Vector3 global = path.transform.position + local;
		if (Event.current.control && Ferr_Menu.SnapMode == Ferr2DT_SnapMode.SnapGlobal) global = SnapVector(global);
		
		Handles.color = Color.white;
		if (!(secondID == 0 && !path.closed && firstDist > secondDist))
		{
			Handles.DrawLine(
				global,
				path.transform.position + path.transform.rotation * Vector3.Scale(new Vector3(path.pathVerts[closestID].x, path.pathVerts[closestID].y, 0), path.transform.localScale));
		}
		if (!(secondID == 0 && !path.closed && firstDist < secondDist))
		{
			Handles.DrawLine(
				global,
				path.transform.position + path.transform.rotation * Vector3.Scale(new Vector3(path.pathVerts[secondID].x, path.pathVerts[secondID].y, 0), path.transform.localScale));
		}
		Handles.color = new Color(1, 1, 1, 0);
		
		Vector3 handlePos = new Vector3(pos.x, pos.y, 0) + path.transform.position + offset;
		if (IsVisible(handlePos)) {
			if (Handles.Button(handlePos, SceneView.lastActiveSceneView.camera.transform.rotation, HandleScale(handlePos), HandleScale(handlePos), Handles.CircleCap))
			{
				Vector3    finalPos = inv * (new Vector3(global.x / path.transform.localScale.x, global.y / path.transform.localScale.y, 0) - path.transform.position);
				if (secondID == 0) {
					if (firstDist < secondDist) {
						if (terrain)
							terrain.AddPoint(finalPos);
						else
							path.pathVerts.Add(finalPos);
					} else {
						if (terrain)
							terrain.AddPoint(finalPos, 0);
						else
							path.pathVerts.Insert(0, finalPos);
					}
				} else {
					if (terrain)
						terrain.AddPoint(finalPos, Mathf.Max(closestID, secondID));
					else
						path.pathVerts.Insert(Mathf.Max(closestID, secondID), finalPos);
				}
				selectedPoints.Clear();
				GUI.changed = true;
			}
			
			SetScale(handlePos, texDotPlus, ref iconStyle);
			Handles.Label(handlePos, new GUIContent(texDotPlus), iconStyle);
		}
		
		if (hasDummy) path.pathVerts.RemoveAt(0);
	}
	private void    DoPath               (Ferr2D_Path path)
	{
		Handles.color = Color.white;
		List<Vector2> verts = path.GetVertsRaw();
		for (int i = 0; i < verts.Count - 1; i++)
		{
			Vector3 pos  = path.transform.position + path.transform.rotation * Vector3.Scale(new Vector3(verts[i    ].x, verts[i    ].y, 0), path.transform.localScale);
			Vector3 pos2 = path.transform.position + path.transform.rotation * Vector3.Scale(new Vector3(verts[i + 1].x, verts[i + 1].y, 0), path.transform.localScale);
			Handles.DrawLine(pos + offset, pos2 + offset);
		}
		if (path.closed)
		{
			Vector3 pos  = path.transform.position + path.transform.rotation * Vector3.Scale(new Vector3(verts[0              ].x, verts[0              ].y, 0), path.transform.localScale);
			Vector3 pos2 = path.transform.position + path.transform.rotation * Vector3.Scale(new Vector3(verts[verts.Count - 1].x, verts[verts.Count - 1].y, 0), path.transform.localScale);
			Handles.DrawLine(pos + offset, pos2 + offset);
		}
	}
	
	private Texture2D                GetDirIcon(Ferr2DT_TerrainDirection aDir) {
		if      (aDir == Ferr2DT_TerrainDirection.Top   ) return texTop;
		else if (aDir == Ferr2DT_TerrainDirection.Right ) return texRight;
		else if (aDir == Ferr2DT_TerrainDirection.Left  ) return texLeft;
		else if (aDir == Ferr2DT_TerrainDirection.Bottom) return texBottom;
		return texAuto;
	}
	private Ferr2DT_TerrainDirection NextDir   (Ferr2DT_TerrainDirection aDir) {
		if      (aDir == Ferr2DT_TerrainDirection.Top   ) return Ferr2DT_TerrainDirection.Right;
		else if (aDir == Ferr2DT_TerrainDirection.Right ) return Ferr2DT_TerrainDirection.Bottom;
		else if (aDir == Ferr2DT_TerrainDirection.Left  ) return Ferr2DT_TerrainDirection.Top;
		else if (aDir == Ferr2DT_TerrainDirection.Bottom) return Ferr2DT_TerrainDirection.None;
		return Ferr2DT_TerrainDirection.Left;
	}
	
	public static Vector2 GetMousePos  (Vector2 aMousePos, float aZOffset) {
		
		//aMousePos.y = Screen.height - (aMousePos.y + 25);
		Ray   ray   = SceneView.lastActiveSceneView.camera.ScreenPointToRay(new Vector3(aMousePos.x, aMousePos.y, 0));
		Plane plane = new Plane(new Vector3(0,0,-1), aZOffset);
		float dist  = 0;
		Vector3 result = new Vector3(0,0,0);
		
		ray = HandleUtility.GUIPointToWorldRay(aMousePos);
		if (plane.Raycast(ray, out dist)) {
			result = ray.GetPoint(dist);
		}
		return new Vector2(result.x, result.y);
	}
	public static float   GetCameraDist(Vector3 aPt) {
		return Vector3.Distance(SceneView.lastActiveSceneView.camera.transform.position, aPt);
	}
	public static bool    IsVisible    (Vector3 aPos) {
		Transform t = SceneView.lastActiveSceneView.camera.transform;
		if (Vector3.Dot(t.forward, aPos - t.position) > 0)
			return true;
		return false;
	}
	public static void    SetScale     (Vector3 aPos, Texture aIcon, ref GUIStyle aStyle, float aScaleOverride = 1) {
		float max      = (Screen.width + Screen.height) / 2;
		float dist     = SceneView.lastActiveSceneView.camera.orthographic ? SceneView.lastActiveSceneView.camera.orthographicSize / 0.5f : GetCameraDist(aPos);
		
		float div = (dist / (max / 160));
		float mul = Ferr_Menu.PathScale * aScaleOverride;
		
		aStyle.fixedWidth  = (aIcon.width  / div) * mul;
		aStyle.fixedHeight = (aIcon.height / div) * mul;
	}
	public static float   HandleScale  (Vector3 aPos) {
		float dist = SceneView.lastActiveSceneView.camera.orthographic ? SceneView.lastActiveSceneView.camera.orthographicSize / 0.45f : GetCameraDist(aPos);
		return Mathf.Min(0.4f * Ferr_Menu.PathScale, (dist / 5.0f) * 0.4f * Ferr_Menu.PathScale);
	}
	
	private static Vector3 SnapVector  (Vector3 aVector) {
		Vector3 snap = new Vector3(EditorPrefs.GetFloat("MoveSnapX"), EditorPrefs.GetFloat("MoveSnapY"), EditorPrefs.GetFloat("MoveSnapZ"));
		return new Vector3(
			((int)(aVector.x / snap.x + (aVector.x > 0 ? 0.5f : -0.5f))) * snap.x,
			((int)(aVector.y / snap.y + (aVector.y > 0 ? 0.5f : -0.5f))) * snap.y,
			((int)(aVector.z / snap.z + (aVector.z > 0 ? 0.5f : -0.5f))) * snap.z);
	}
	private static Vector2 SnapVector  (Vector2 aVector) {
		Vector2 snap = new Vector2(EditorPrefs.GetFloat("MoveSnapX"), EditorPrefs.GetFloat("MoveSnapY"));
		return new Vector2(
			((int)(aVector.x / snap.x + (aVector.x > 0 ? 0.5f : -0.5f))) * snap.x,
			((int)(aVector.y / snap.y + (aVector.y > 0 ? 0.5f : -0.5f))) * snap.y);
	}
	private static Vector3 GetRealPoint(Vector3 aPoint, float aHeight) {
		Plane p = new Plane(new Vector3(0, 0, -1), new Vector3(0, 0, aHeight));
		Ray   r = new Ray  (SceneView.lastActiveSceneView.camera.transform.position, aPoint - SceneView.lastActiveSceneView.camera.transform.position);
		float d = 0;
		
		if (p.Raycast(r, out d)) {
			Vector3 result = r.GetPoint(d);
			result.z = aHeight;
			return result;
		}
		return aPoint;
	}
	private Vector3 SmartSnap(Vector3 aPoint, List<Vector2> aPath, List<int> aIgnore, float aSnapDist) {
		float   minXDist = aSnapDist;
		float   minYDist = aSnapDist;
		Vector3 result   = aPoint;
		
		for (int i = 0; i < aPath.Count; ++i) {
			if (aIgnore.Contains(i)) continue;
			
			float xDist = Mathf.Abs(aPoint.x - aPath[i].x);
			float yDist = Mathf.Abs(aPoint.y - aPath[i].y);
			
			if (xDist < minXDist) {
				minXDist = xDist;
				result.x = aPath[i].x;
			}
			
			if (yDist < minYDist) {
				minYDist = yDist;
				result.y = aPath[i].y;
			}
		}
		return result;
	}
}