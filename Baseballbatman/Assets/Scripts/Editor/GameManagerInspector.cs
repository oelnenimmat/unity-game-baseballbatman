using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (GameManager))]
public class GameManagerInspector : Editor
{
	public override void OnInspectorGUI ()
	{
		if (GUILayout.Button ("Generate")) {
			(target as GameManager).GenerateLevel ();
		}
		base.OnInspectorGUI ();
	}
}
