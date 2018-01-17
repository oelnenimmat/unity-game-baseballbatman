using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileSet))]
public class TileSetEditor : Editor 
{
	public override void OnInspectorGUI ()
	{
		if (DrawDefaultInspector ()) {
			(target as TileSet).Refresh ();
		}
	}
}
