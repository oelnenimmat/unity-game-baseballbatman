using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class RoadTile : Tile 
{
	[SerializeField] private Sprite [] sprites;

	public override void RefreshTile (Vector3Int position, ITilemap tilemap)
	{
		base.RefreshTile (position, tilemap);

		Vector3Int otherPosition = new Vector3Int ();
		for (int y = position.y - 1; y <= position.y + 1; y++) {
			otherPosition.y = y;

			for (int x = position.x - 1; x <= position.x + 1; x++) {
				otherPosition.x = x;

				if (tilemap.GetTile (otherPosition) == this) {
					tilemap.RefreshTile (otherPosition);
				}
			}	
		}

		tilemap.RefreshTile (new Vector3Int(position.x - 1,	position.y + 1, position.z));
		tilemap.RefreshTile (new Vector3Int(position.x, 	position.y + 1, position.z));
		tilemap.RefreshTile (new Vector3Int(position.x + 1, position.y + 1, position.z));
		tilemap.RefreshTile (new Vector3Int(position.x - 1, position.y, 	position.z));
//		tilemap.RefreshTile (new Vector3Int(position.x,		position.y, 	position.z));
		tilemap.RefreshTile (new Vector3Int(position.x + 1, position.y, 	position.z));
		tilemap.RefreshTile (new Vector3Int(position.x - 1, position.y - 1, position.z));
		tilemap.RefreshTile (new Vector3Int(position.x, 	position.y - 1,	position.z));
		tilemap.RefreshTile (new Vector3Int(position.x + 1, position.y - 1, position.z));
	}

	private bool HasRoadTile (Vector3Int position, ITilemap tilemap)
	{
		return tilemap.GetTile (position) == this;
	}

	public override void GetTileData (Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		base.GetTileData (position, tilemap, ref tileData);

		int mask = 0;

		mask += HasRoadTile (new Vector3Int(position.x - 1,	position.y + 1, position.z), tilemap) ? 1 : 0;
		mask += HasRoadTile (new Vector3Int(position.x, 	position.y + 1, position.z), tilemap) ? 2 : 0;
		mask += HasRoadTile (new Vector3Int(position.x + 1, position.y + 1, position.z), tilemap) ? 4 : 0;
		mask += HasRoadTile (new Vector3Int(position.x - 1, position.y, 	position.z), tilemap) ? 8 : 0;

		mask += HasRoadTile (new Vector3Int(position.x + 1, position.y, 	position.z), tilemap) ? 16 : 0;
		mask += HasRoadTile (new Vector3Int(position.x - 1, position.y - 1, position.z), tilemap) ? 32 : 0;
		mask += HasRoadTile (new Vector3Int(position.x, 	position.y - 1,	position.z), tilemap) ? 64 : 0;
		mask += HasRoadTile (new Vector3Int(position.x + 1, position.y - 1, position.z), tilemap) ? 128 : 0;

		int spriteIndex = TilesMap.GetTileIndex(mask);
		int rotation = TilesMap.GetRotationIndex (mask);

		float zRot = 0f;
		switch (rotation) {
		case 0:
			zRot = 0f;
			break;
		case 1:
			zRot = -90f;
			break;
		case 2:
			zRot = -180f;
			break;
		case 3:
			zRot = -270f;
			break;
		}
		tileData.transform = Matrix4x4.Rotate (Quaternion.Euler (0f, 0f, zRot));

		try {
			tileData.sprite = sprites [spriteIndex];
		} catch (System.Exception e) {
			Debug.Log (spriteIndex + "    " + position);

		}
			
		tileData.flags = TileFlags.LockAll;

	}
}