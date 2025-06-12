using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TransformMgr : ScriptableObject
{
	private static Vector3 position;
	private static Quaternion rotation;
	private static Vector3 scale;
	private static bool has_data = false;
	
	[MenuItem("GameObject/Get Transform")]
	static void GetTransform ()
	{
		if (Selection.activeTransform != null)
		{
			position = Selection.activeTransform.position;
			rotation = Selection.activeTransform.rotation;
			scale = Selection.activeTransform.localScale;
			has_data = true;
		}
	}

	[MenuItem("GameObject/Set Transform")]
	static void SetTransform ()
	{
		if (Selection.activeTransform != null && has_data)
		{
			Selection.activeTransform.position = position;
			Selection.activeTransform.rotation = rotation;
			Selection.activeTransform.localScale = scale;
		}
	}
}
