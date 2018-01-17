using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NoiseVisualizer : MonoBehaviour
{
	public MeshRenderer visualizer;
	public Vector2Int textureSize = new Vector2Int (100, 100);
	Texture2D texture;

	[Range(0f, 1f)] public float treshold;
	public bool useTreshold;

	void OnEnable ()
	{
		visualizer = GetComponent<MeshRenderer> ();
		visualizer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
	}

	public void Generate ()
	{
		textureSize.x = textureSize.x < 1 ? 1 : textureSize.x;
		textureSize.y = textureSize.y < 1 ? 1 : textureSize.y;


		Color[] pixels = new Color[textureSize.x * textureSize.y];

		for (int y = 0; y < textureSize.y; y++) {
			for (int x = 0; x < textureSize.x; x++) {


				float value = Noise.Value2D (x, y);
				if (useTreshold) {
					value = value < treshold ? 0 : 1;
				}

				pixels [x + textureSize.x * y] = new Color (value, value, value);
					

			}
		}

		texture = new Texture2D (textureSize.x, textureSize.y);
		texture.filterMode = FilterMode.Point;
		visualizer.sharedMaterial.mainTexture = texture;
		texture.SetPixels (pixels);
		texture.Apply ();
	}
}
