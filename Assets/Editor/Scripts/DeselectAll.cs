using UnityEngine;
using UnityEditor;     
public class EditorShortCutKeys : ScriptableObject
{    
	[MenuItem("GameObject/Deselect all #Q")]
	static void DeselectAll()
	{
		Selection.objects = new Object[0];
	}
}