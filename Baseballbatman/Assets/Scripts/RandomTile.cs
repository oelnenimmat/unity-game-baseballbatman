using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class RandomTile : Tile 
{
	[SerializeField] private Sprite[] _sprites;
	public void SetSprites (Sprite [] sprites)
	{
		_sprites = sprites;
	}

	public override void GetTileData (UnityEngine.Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		// Maybe this is needed
//		base.GetTileData (position, tilemap, ref tileData);
		tileData.sprite = _sprites.RandomItem ();
		tileData.gameObject = gameObject;
	}

	#if UNITY_EDITOR
	[MenuItem("Assets/Create/Random Tile")]
	public static void CreateTileSet () {
		string path = EditorUtility.SaveFilePanelInProject ("Save Random Tile", "New Random Tile", "Asset", "Save Random Tile", "Assets");
		if (path == "")
			return;
		AssetDatabase.CreateAsset (ScriptableObject.CreateInstance<RandomTile> (), path);
	}
	#endif
}
