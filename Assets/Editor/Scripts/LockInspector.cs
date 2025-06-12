using UnityEngine;
using UnityEditor;

public class LockInspector : ScriptableObject
{
	[MenuItem("Edit/Lock2 _`")]
	public static void Lock ()
	{
		ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
		ActiveEditorTracker.sharedTracker.ForceRebuild ();
	}
	
	[MenuItem("Edit/Lock _`", true)]
	public static bool Valid ()
	{
		return ActiveEditorTracker.sharedTracker.activeEditors.Length != 0;
	}
}
