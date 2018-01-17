using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class WallTileSet : ScriptableObject
{
	[SerializeField] private Tile solidTile;
	[SerializeField] private Tile transparentTile;

	public Tile GetTile (int mask)
	{
		return mask == 1 ? solidTile : transparentTile;
	}
}
