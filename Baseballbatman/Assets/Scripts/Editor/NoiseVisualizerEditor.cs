using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoiseVisualizer))]
public class NoiseVisualizerEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		if(GUILayout.Button("Generate")) {
			(target as NoiseVisualizer).Generate ();
		}
		if (DrawDefaultInspector ()) {
			(target as NoiseVisualizer).Generate ();
		}
	}
}
