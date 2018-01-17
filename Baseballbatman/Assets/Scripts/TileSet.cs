using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using UnityEditor;


public class TileSet : ScriptableObject
{
	[SerializeField] private Sprite [] _groundSprites;
	[SerializeField, HideInInspector] private RandomTile _groundTile;
	public RandomTile Ground 
	{ 
		get {
			if (_groundTile == null) {
				_groundTile = ScriptableObject.CreateInstance<RandomTile> ();
				_groundTile.SetSprites (_groundSprites);
			
			}
			_groundTile.gameObject = null;
			return _groundTile; 
		} 
	}

	[SerializeField] private NPCContainer containerPrefab;
	[SerializeField] private GameObject enemyPrefab;
	[SerializeField, HideInInspector] private RandomTile _enemyTile;
	public RandomTile EnemyTile
	{
		get {
			if (_enemyTile == null) {
				_enemyTile = ScriptableObject.CreateInstance<RandomTile> ();
				_enemyTile.SetSprites (_groundSprites);
				containerPrefab.containedEnemyPrefab = enemyPrefab;
				_enemyTile.gameObject = containerPrefab.gameObject;
			}
			return _enemyTile;
		}
	}
	public GameObject ContainedEnemy {
		get {
			containerPrefab.containedEnemyPrefab = enemyPrefab;
			return containerPrefab.gameObject;
		}
	}


	[SerializeField] private Sprite hazardSprite;
	[SerializeField, HideInInspector] private Tile hazardTile;
	[SerializeField] private GameObject hazardPrefab;
	public Tile Hazard
	{
		get {
			if (hazardTile == null) {
				hazardTile = ScriptableObject.CreateInstance<Tile> ();
				hazardTile.sprite = hazardSprite;
				hazardTile.gameObject = hazardPrefab;
			}
			return hazardTile;
		}
	}
	[Range(0f, 1f)] public float hazardPercent = 0.1f;
	public NoiseBool hazardNoiseMethod;



	public void Refresh ()
	{
		Ground.SetSprites (_groundSprites);
		Hazard.sprite = hazardSprite;
		EnemyTile.SetSprites (_groundSprites);
		EnemyTile.gameObject = enemyPrefab;
	}


	#if UNITY_EDITOR
	[MenuItem("Assets/Create/Tile Set")]
	public static void CreateTileSet () {
		string path = EditorUtility.SaveFilePanelInProject ("Save Tile Set", "New Tile Set", "Asset", "Save Tile Set", "Assets");
		if (path == "")
			return;
		AssetDatabase.CreateAsset (ScriptableObject.CreateInstance<TileSet> (), path);
	}
	#endif
}