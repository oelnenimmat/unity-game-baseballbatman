using System.Diagnostics;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class GeneratedLevel
{
	public bool takeTime = true;
	private Stopwatch stopwatch = new Stopwatch();
	private Stopwatch otherStopwatch = new Stopwatch ();

	public Grid grid;
	[SerializeField] private Tilemap groundTilemap;
	[SerializeField] private RoadTile roadTile;
	[SerializeField] private Tilemap roadTilemap;

	[SerializeField] private Tilemap wallTilemap;
	[SerializeField] private RoadTile wallTile;

	[SerializeField] private TileSet[] tileSets;

	[SerializeField] private WallTileSet wallTileSet;

	[SerializeField] GameObject colliderObject;

	public Tilemap debugTilemap;
	public Tile debugTile;
	public Tile debugTile2;

	[SerializeField, Range(0f, 1f)] private float _enemyPercent = 0.01f;
	[SerializeField] private Vector2Int mapSize = new Vector2Int (50, 50);
	[SerializeField] private int 
		roomsNumber 	= 20, 
		minRoomSize 	=  5, 
		maxRoomSize 	= 20,
		minRoadLength 	=  5,
		maxRoadLength 	= 20;

	private Direction RandomMajorDirection { get { return (Direction)(RandomUtility.Range (0, Directions.Count / 2) * 2); } }
	private Vector3 cellCenterOffset { get { return (Vector3)grid.cellSize / 2f; } }

	#region Bitmaskmap constants and helpers
	// Bitmask map info
	/*
			Bitmaskmap knows of itself and of 8 surrounding tiles
			First is itself, 8 next are surrounding tiles
			 - ground: 		bits 1 - 9
			 - roads: 		bits 10 - 18
			 - walls: 		bits 19 - 27

			 - checkedbit for regionfinding floodfill
			 	bit 32
			This leaves bits 28 to 32 unused, maybe store region info there
		*/
	// these refer to surrounding tiles around current tile
	private const Int32	topLeft 	= 1,
						topMid 		= 2,
						topRight	= 4,
						midLeft 	= 8,
						midRight	= 16,
						bottomLeft	= 32,
						bottomMid	= 64,
						bottomRight	= 128,

	// use these with surrounding tiles
						groundShift = 1,
						roadShift 	= 10,
						wallShift 	= 19,

	// these refer to bit in which is stored if this tile has that tile in it
						groundBit 	= 1 << 0,
						roadBit 	= 1 << 9,
						wallBit 	= 1 << 18;

	// This is used in region separating to check if tile has been looked already
//						checkedBit = 1 << 31;

	private void SetGround (Int32 [,] map, int x, int y)
	{
		map [x, y] |= groundBit;

		map [x - 1, y - 1]	|= (topRight	<< groundShift);
		map [x, y - 1]		|= (topMid		<< groundShift);
		map [x + 1, y - 1]	|= (topLeft		<< groundShift);

		map [x - 1, y]		|= (midRight	<< groundShift);
		map [x + 1, y]		|= (midLeft		<< groundShift);

		map [x - 1, y + 1]	|= (bottomRight	<< groundShift);
		map [x, y + 1]		|= (bottomMid	<< groundShift);
		map [x + 1, y + 1]	|= (bottomLeft	<< groundShift);	
	}

	private int GetGroundMask (Int32 [,] map, int x, int y) {
		return (map [x, y] >> groundShift) & 255;
	}

	private void SetRoad (Int32 [,] map, int x, int y)
	{
		map [x, y] |= roadBit;

		map [x - 1, y - 1]	|= (topRight	<< roadShift);
		map [x, y - 1]		|= (topMid		<< roadShift);
		map [x + 1, y - 1]	|= (topLeft		<< roadShift);

		map [x - 1, y]		|= (midRight	<< roadShift);
		map [x + 1, y]		|= (midLeft		<< roadShift);

		map [x - 1, y + 1]	|= (bottomRight	<< roadShift);
		map [x, y + 1]		|= (bottomMid	<< roadShift);
		map [x + 1, y + 1]	|= (bottomLeft	<< roadShift);
	}

	private int GetRoadMask (Int32 [,] map, int x, int y) {
		return (map [x, y] >> roadShift) & 255;
	}

	private void SetWall (Int32 [,] map, int x, int y)
	{
		map [x, y] |= wallBit;

		map [x - 1, y - 1]	|= (topRight	<< wallShift);
		map [x, 	y - 1]	|= (topMid		<< wallShift);
		map [x + 1, y - 1]	|= (topLeft		<< wallShift);

		map [x - 1, y]		|= (midRight	<< wallShift);
		map [x + 1, y]		|= (midLeft		<< wallShift);

		map [x - 1, y + 1]	|= (bottomRight	<< wallShift);
		map [x, 	y + 1]	|= (bottomMid	<< wallShift);
		map [x + 1, y + 1]	|= (bottomLeft	<< wallShift);
	}

	private int GetWallMask (Int32 [,] map, int x, int y) {
		return (map [x, y] >> wallShift) & 255;
	}
	#endregion

	/// <summary>
	/// Generate map and set tiles to tilemaps.
	/// </summary>
	public StartCoordinates Generate ()
	{
		/*
			OVERVIEW:
			Start by generating rooms, that are  connected by roads.
			Then, as roads now are only outside and between rooms, connect them inside rooms.
			Next, flatten rooms to a int [,] map, and find different regions there, as those are what we are interested in.
			Also, add walls wherever region or road tile meets empty.
			Only horizontal walls are shown in game, so only those tiles are added to wallilemap, but add colliders to every wall position.
		*/

		// Clear colliderobject
		Collider2D[] oldColliders = colliderObject.GetComponents <Collider2D> ();
		for (int i = 0; i < oldColliders.Length; i++) {
			GameObject.DestroyImmediate (oldColliders [i]);
		}

		// Clear all tilemaps
		groundTilemap.ClearAllTiles ();
		if (Application.isPlaying) {
			for (int i = groundTilemap.transform.childCount - 1; i >= 0; i--) {
				GameObject.Destroy (groundTilemap.transform.GetChild (i).gameObject);
			}
		}
		roadTilemap.ClearAllTiles ();
		wallTilemap.ClearAllTiles ();
		debugTilemap.ClearAllTiles ();

		// Minimum and maximum coordinates INSIDE room
		Vector2Int[] roomMins = new Vector2Int[roomsNumber];
		Vector2Int[] roomMaxs = new Vector2Int[roomsNumber];

		Direction[] directions = new Direction	[roomsNumber];

		Vector2Int[] roadStarts	= new Vector2Int[roomsNumber - 1];
		Vector2Int[] roadEnds = new Vector2Int[roomsNumber - 1];

		List<Vector2Int> diagonalRoadCoordinates = new List<Vector2Int> ();

		Vector2Int boundsMin = mapSize;
		Vector2Int boundsMax = Vector2Int.zero;


		stopwatch.Reset ();
		stopwatch.Start ();



		// Generate First Room, start from roughly center
		Vector2Int startCoord = new Vector2Int (mapSize.x / 2, mapSize.y / 2);
		GenerateRoom (startCoord, RandomMajorDirection, out roomMins [0], out roomMaxs [0], out startCoord, out directions [0]);
		boundsMin = Vector2Int.Min (boundsMin, roomMins [0]);
		boundsMax = Vector2Int.Max (boundsMax, roomMaxs [0]);

		// Generate roads and other rooms
		for (int i = 1; i < roomsNumber; i++) {
			GenerateRoad (startCoord, directions [i - 1], out roadStarts [i - 1], out roadEnds [i - 1], out startCoord);
			GenerateRoom (startCoord, directions [i - 1], out roomMins [i], out roomMaxs [i], out startCoord, out directions [i]);

			boundsMin = Vector2Int.Min (boundsMin, roomMins [i]);
			boundsMax = Vector2Int.Max (boundsMax, roomMaxs [i]);
		}

		// What is going on??? why these need to be expanded so much
		boundsMax += Vector2Int.one * 3;
		boundsMin -= Vector2Int.one * 3;


		// This tries to cover all possible tiles at same time.
		Vector2Int bitmaskMapSize = new Vector2Int (boundsMax.x - boundsMin.x, boundsMax.y - boundsMin.y);
		Int32[,] bitmaskMap = new Int32[bitmaskMapSize.x, bitmaskMapSize.y];

		// walkablemap is passed to pathfinder to create pathfinding grid
		int[,] walkableMap = new int[bitmaskMapSize.x, bitmaskMapSize.y];

		// Add rooms to bitmaskmap
		for (int i = 0; i < roomsNumber; i++) {
			for (int h = roomMins [i].y; h <= roomMaxs [i].y; h++) {
				for (int w = roomMins [i].x; w <= roomMaxs [i].x; w++) {
					int x = w - boundsMin.x;
					int y = h - boundsMin.y;

					if ((bitmaskMap [x, y] & groundBit) != groundBit) {
						SetGround (bitmaskMap, x, y);
						walkableMap [x, y] = 1;		
					}
				}
			}
		} 

		// Add roads to bitmaskmap
		for (int i = 0; i < roomsNumber - 1; i++) {
			if (i < roomsNumber - 2) {
				ConnectRoads (
					ref roadStarts [i], ref roadEnds [i], directions [i],
					ref roadStarts [i + 1], ref roadEnds [i + 1], directions [i + 1],
					bitmaskMap, boundsMin
				);
			}

			for (int h = roadStarts [i].y - 1; h <= roadEnds [i].y + 1; h++) {
				for (int w = roadStarts [i].x - 1; w <= roadEnds [i].x + 1; w++) {
					int x = w - boundsMin.x;
					int y = h - boundsMin.y;

					// If this actually is road
					if (h >= roadStarts [i].y && h <= roadEnds [i].y && w >= roadStarts [i].x && w <= roadEnds [i].x) {

						if ((bitmaskMap [x, y] & roadBit) != roadBit) {

							// Add also to walkable map
							walkableMap [w - boundsMin.x, h - boundsMin.y] = 1;
							SetRoad (bitmaskMap, x, y);
						}

					} else if ((bitmaskMap [x, y] & (groundBit | roadBit)) == 0) {
						bitmaskMap [x, y] |= wallBit;
					}
				}
			}
		}

		// Separate Regions
		List<List<Vector3Int>> regions = new List<List<Vector3Int>> ();
		Queue<Vector3Int> coordQueue = new Queue<Vector3Int> ();
		int [,] checkedCoords = new int[bitmaskMapSize.x, bitmaskMapSize.y];

		for (int y = 0; y < bitmaskMapSize.y; y++) {
			for (int x = 0; x < bitmaskMapSize.x; x++) {

				if ((bitmaskMap [x, y] & groundBit) == groundBit && checkedCoords [x,y] == 0) {

					coordQueue.Enqueue (new Vector3Int (x, y, 0));

					List<Vector3Int> region = new List<Vector3Int> ();
					regions.Add (region);

					// Floodfill with horizontal scanlines
					while (coordQueue.Count > 0) {
						Vector3Int coord = coordQueue.Dequeue ();
							
						// Expand to east
						int e = coord.x; 
						while (
							e < bitmaskMapSize.x && 
							(bitmaskMap [e, coord.y] & groundBit) == groundBit && 
							checkedCoords[e, coord.y] == 0
						) {
							e++;
						}

						// Expand to west
						int w = coord.x;
						while (
							w > 0 &&
							(bitmaskMap [w, coord.y] & groundBit) == groundBit &&
							checkedCoords[w, coord.y] == 0
						) {
							w--;
						}

						int n = coord.y + 1;
						int s = coord.y - 1;

						// East wall, include corners
						if (e >= bitmaskMapSize.x || (bitmaskMap [e, coord.y] & (groundBit | roadBit)) == 0) {
							SetWall (bitmaskMap, e, coord.y);
						} 

						if (e >= bitmaskMapSize.x || n >= bitmaskMapSize.y || (bitmaskMap [e, n] & (groundBit | roadBit)) == 0) {
							SetWall (bitmaskMap, e, n);
						}
							
						if (e >= bitmaskMapSize.x || s < 0 || (bitmaskMap [e, s] & (groundBit | roadBit)) == 0) {
							SetWall (bitmaskMap, e, s);
						}

						// West wall, include corners
						if (w < 0 || (bitmaskMap [w, coord.y] & (groundBit | roadBit)) == 0) {
							SetWall (bitmaskMap, w, coord.y);
						}

						if (w < 0 || n >= bitmaskMapSize.y || (bitmaskMap [w,n] & (groundBit | roadBit)) == 0) {
							SetWall (bitmaskMap, w, n);
						}

						if (w < 0 || s < 0 || (bitmaskMap [w, s] & (groundBit | roadBit)) == 0) {
							SetWall (bitmaskMap, w,s);
						}

						for (coord.x = w + 1; coord.x < e; coord.x++) {

							region.Add (coord);
							checkedCoords[coord.x, coord.y] = 1;

							// Here, add north and south coords to queue if room continues, else add wall (exluding corners)
							if (n < bitmaskMapSize.y && (bitmaskMap [coord.x, n] & groundBit) == groundBit) {
								coordQueue.Enqueue (coord + Vector3Int.up);

							} else if (n >= bitmaskMapSize.y || (bitmaskMap [coord.x, n] & (groundBit | roadBit)) == 0) {
								SetWall (bitmaskMap, coord.x, n);
							}

							if (s >= 0 && (bitmaskMap [coord.x, s] & groundBit) == groundBit) {
								coordQueue.Enqueue (coord + Vector3Int.down);

							} else if (s < 0 || (bitmaskMap [coord.x, s] & (groundBit | roadBit)) == 0) {
								SetWall (bitmaskMap, coord.x, s);
							}
						}
					}
				} 
			}
		}




		List <Vector3> enemyCoordinates = new List<Vector3> ();

		// Set regions Tiles
		Vector3Int [][] regionsArray = new Vector3Int[regions.Count][];
		Tile[][] regionsTiles = new Tile[regions.Count][];
		for (int i = 0; i < regions.Count; i++) {
			regionsArray [i] = regions [i].ToArray ();
			regionsTiles [i] = new Tile[regionsArray[i].Length];

			TileSet tileSet = tileSets.RandomItem();
			for (int j = 0; j < regionsTiles [i].Length; j++) {

				// Add boundsMax as offset, to get varying noise, its not linked to coordinates here any other way and can be replaced
				bool hazard = Noise.Bool (tileSet.hazardNoiseMethod, regionsArray [i] [j].x + boundsMax.x, regionsArray [i] [j].y + boundsMax.y, tileSet.hazardPercent);

				// remove all hazards under roads
				if ((bitmaskMap [regionsArray[i][j].x, regionsArray [i][j].y] & roadBit) == roadBit) {
					hazard = false;
				}

				regionsTiles [i][j] = hazard ? tileSet.Hazard : tileSet.Ground;

				if (!hazard) {
					walkableMap [regionsArray [i] [j].x, regionsArray [i] [j].y] = 1;
				}

				if (!hazard && RandomUtility.Percent < _enemyPercent) {
					enemyCoordinates.Add (
						new Vector3 (
							regionsArray[i][j].x * grid.cellSize.x + cellCenterOffset.x,
							regionsArray[i][j].y * grid.cellSize.y + cellCenterOffset.y,
							0
						)
					);
				}

			}
		}

		stopwatch.Stop ();
		if (takeTime)
			UnityEngine.Debug.Log (stopwatch.Elapsed);


		// Set rooms
		for (int i = 0; i < regionsArray.Length; i++) {
			groundTilemap.SetTiles (regionsArray [i], regionsTiles [i]);
		}


		// Set roads and walls
		List<Vector3Int> wallCoordinates = new List<Vector3Int> ();
		for (int y = 0; y < bitmaskMapSize.y; y++) {
			for (int x = 0; x < bitmaskMapSize.x; x++) {

				// Add road
				if ((bitmaskMap [x, y] & roadBit) == roadBit) {
					roadTilemap.SetTile (new Vector3Int (x, y, 0), roadTile);

				// Add walls
				} else if ((bitmaskMap [x, y] & wallBit) == wallBit) {
					
					// solid
					Int32 mapValue = bitmaskMap [x, y] >> groundShift | bitmaskMap [x, y] >> roadShift;
					if ((mapValue & bottomMid)  == bottomMid) {
						wallTilemap.SetTile (new Vector3Int (x, y, 0), wallTileSet.GetTile(1));
					}

					// transparent
					if ((mapValue & topMid) == topMid) {
						wallTilemap.SetTile (new Vector3Int (x, y + 1, 0), wallTileSet.GetTile (0));
					}

					// Add colliders
					BoxCollider2D collider = colliderObject.AddComponent <BoxCollider2D> ();
					collider.offset = (new Vector2 (x, y)) * 2 + Vector2.one;
					collider.size = Vector2.one * 2;

				}
			}
		}



		Vector2Int firstRoadCoord = directions [0] == Direction.North || directions[0] == Direction.East ? roadStarts [0] : roadEnds [0];
		firstRoadCoord -= boundsMin;

		int lastRoadIndex = roomsNumber - 2;
		Vector2Int lastRoadCoord = directions [lastRoadIndex] == Direction.North || directions[lastRoadIndex] == Direction.East ? roadEnds [lastRoadIndex] : roadStarts [lastRoadIndex];
		lastRoadCoord -= boundsMin;

		Vector3 playerPosition = new Vector3 (
			firstRoadCoord.x * grid.cellSize.x,
			firstRoadCoord.y * grid.cellSize.y,
			0
		) + cellCenterOffset;

		Vector3 rescueePosition = new Vector3 (
			lastRoadCoord.x * grid.cellSize.x,
			lastRoadCoord.y * grid.cellSize.y,
			0
		) + cellCenterOffset;

		Vector3[] enemyPositions = enemyCoordinates.ToArray ();

		return new StartCoordinates (playerPosition, rescueePosition, enemyPositions, walkableMap);
	}


	private void GenerateRoom (Vector2Int enterCoord, Direction enterDirection, out Vector2Int min, out Vector2Int max, out Vector2Int exitCoord, out Direction exitDirection)
	{
		Vector2Int roomSize = new Vector2Int (
			RandomUtility.Range (minRoomSize, maxRoomSize),
			RandomUtility.Range (minRoomSize, maxRoomSize)
		);

		// Generate room corners (min and max) depending on enterDirection
		min = new Vector2Int();
		if (enterDirection.IsVertical ()) {
			min.x = 	RandomUtility.Range (-roomSize.x, 0) + enterCoord.x + 1;
			min.y = 	enterDirection == Direction.North ? 
				enterCoord.y : 
				enterCoord.y - roomSize.y + 1;

		} else if (enterDirection.IsHorizontal ()) {
			min.x = 	enterDirection == Direction.East ? 
				enterCoord.x : 
				enterCoord.x - roomSize.x + 1;
			min.y = 	RandomUtility.Range (-roomSize.y, 0) + enterCoord.y + 1;

		} else {
			UnityEngine.Debug.LogError ("Invalid Room enter direction!");
		}
		max = min + roomSize - Vector2Int.one;


		// Get next direction
		if (min.y < 0) {
			exitDirection = Direction.North;
		} else if (max.y > mapSize.y) {
			exitDirection = Direction.South;
		} else if (min.x < 0) {
			exitDirection = Direction.East;
		} else if (max.x > mapSize.x) {
			exitDirection = Direction.West;
		} else {
			exitDirection = RandomMajorDirection;
		}

		exitDirection = exitDirection == enterDirection.Opposite() ? exitDirection.Clockwise() : exitDirection;



		// Generate exit coordinate depending on exitDirection and room size
		// Make the range smaller, so that road won't start in corner
		exitCoord = new Vector2Int ();
		if (exitDirection.IsVertical ()) {
			exitCoord.x = 	RandomUtility.Range (min.x + 1, max.x - 1);
			exitCoord.y = 	exitDirection == Direction.North ? 
				max.y : 
				min.y - 1;

		} else if (exitDirection.IsHorizontal ()) {
			exitCoord.x = 	exitDirection == Direction.East ? 
				max.x : 
				min.x - 1;
			exitCoord.y = 	RandomUtility.Range (min.y + 1, max.y - 1);

		} else {
			UnityEngine.Debug.LogError ("Invalid Room exit direction!");
		}
	}

	private void GenerateRoad (Vector2Int startCoord, Direction enterDirection, out Vector2Int roadStart, out Vector2Int roadEnd, out Vector2Int endCoord)
	{
		// Length of road
		int roadLength = RandomUtility.Range (minRoadLength, maxRoadLength);

		// Move road initial position one space backwards, so it starts from within the room
		roadStart = startCoord - enterDirection.ToVector2Int ();

		// Set road end and next room's startcoord (endcoord) to roadlength amount to enterdirection
		endCoord = roadEnd = startCoord + enterDirection.ToVector2Int() * roadLength;

		// Road start needs to be smaller, so swap if necessary
		if (roadEnd.x <= roadStart.x && roadEnd.y <= roadStart.y) {
			roadEnd = roadStart;
			roadStart = endCoord;
		}
	}

	private void ConnectRoads (
		ref Vector2Int previousStart, ref Vector2Int previousEnd, Direction previousDirection,
		ref Vector2Int nextStart, ref Vector2Int nextEnd, Direction nextDirection,
		Int32 [,] map, Vector2Int boundsMin
	)
	{
		Vector2Int connectionStart 	= previousDirection == Direction.North || previousDirection == Direction.East ? previousEnd : previousStart;
		Vector2Int connectionEnd 	= nextDirection == Direction.North || nextDirection == Direction.East ? nextStart : nextEnd;

		Vector2Int delta 	= connectionEnd - connectionStart;
		Vector2Int absDelta = new Vector2Int (Mathf.Abs (delta.x), Mathf.Abs (delta.y));

		int diagonalLength = 0;
		Vector2Int diagonalAddition = Vector2Int.zero;

		// Road changes direction
		if (previousDirection != nextDirection) {
			int extrudeLength = Mathf.Abs (absDelta.x - absDelta.y);


			// Extrude Horizontally
			if (absDelta.x > absDelta.y) {

				if (previousDirection == Direction.East) {
					previousEnd.x += extrudeLength;
				} else if (previousDirection == Direction.West) {
					previousStart.x -= extrudeLength;
				} else if (nextDirection == Direction.East) {
					nextStart.x -= extrudeLength;
				} else if (nextDirection == Direction.West) {
					nextEnd.x += extrudeLength;
				}

				// Extrude Vertically
			} else if (absDelta.x < absDelta.y) {

				if (previousDirection == Direction.North) {
					previousEnd.y += extrudeLength;
				} else if (previousDirection == Direction.South) {
					previousStart.y -= extrudeLength;
				} else if (nextDirection == Direction.North) {
					nextStart.y -= extrudeLength;
				} else if (nextDirection == Direction.South) {
					nextEnd.y += extrudeLength;
				}

			} 

			diagonalLength = Mathf.Min (absDelta.x, absDelta.y) - 1;
			diagonalAddition = previousDirection.ToVector2Int () + nextDirection.ToVector2Int ();

			// Road continues forward
		} else {

			// Horizontal gap is bigger
			if (absDelta.x > absDelta.y) {

				int extrudeAmount = Mathf.Abs (Mathf.Abs (previousDirection.ToVector2Int ().x * absDelta.x) - absDelta.y);
				int extrudePrevious = extrudeAmount / 2;
				int extrudeNext = extrudeAmount - extrudePrevious;

				if (previousDirection == Direction.East) {
					previousEnd.x += extrudePrevious;
					nextStart.x -= extrudeNext;

				} else if (previousDirection == Direction.West) {
					previousStart.x -= extrudePrevious;
					nextEnd.x += extrudeNext;

				} else if (previousDirection == Direction.North) {
					previousEnd.y += extrudePrevious;
					nextStart.y -= extrudeNext;

				} else if (previousDirection == Direction.South) {
					previousStart.y -= extrudePrevious;
					nextEnd.y += extrudeNext;
				}


				// Extrude both roads
				if (previousDirection.IsHorizontal ()) {
					diagonalLength = absDelta.y - 1;
					diagonalAddition = previousDirection.ToVector2Int () + (connectionStart.y < connectionEnd.y ? Vector2Int.up : Vector2Int.down);

					// Make sharp turn
				} else if (previousDirection.IsVertical ()) {
					diagonalLength = absDelta.x - 1;
					diagonalAddition = connectionStart.x < connectionEnd.x ? Vector2Int.right : Vector2Int.left;

				} 

				// Vertical gap is bigger
			} else if (absDelta.x < absDelta.y) {

				int extrudeAmount = Mathf.Abs (Mathf.Abs (previousDirection.ToVector2Int ().y * absDelta.y) - absDelta.x);
				int extrudePrevious = extrudeAmount / 2;
				int extrudeNext = extrudeAmount - extrudePrevious;

				if (previousDirection == Direction.North) {
					previousEnd.y += extrudePrevious;
					nextStart.y -= extrudeNext;

				} else if (previousDirection == Direction.South) {
					previousStart.y -= extrudePrevious;
					nextEnd.y += extrudeNext;

				} else if (previousDirection == Direction.East) {
					previousEnd.x += extrudePrevious;
					nextStart.x -= extrudeNext;

				} else if (previousDirection == Direction.West) {
					previousStart.x -= extrudePrevious;
					nextEnd.x += extrudeNext;

				}

				// Extrude both roads
				if (previousDirection.IsVertical ()) {
					diagonalLength = absDelta.x - 1;
					diagonalAddition = previousDirection.ToVector2Int () + (connectionStart.x < connectionEnd.x ? Vector2Int.right : Vector2Int.left);

					// Make sharp turn
				} else if (previousDirection.IsHorizontal ()) {
					diagonalLength = absDelta.y - 1;
					diagonalAddition = connectionStart.y < connectionEnd.y ? Vector2Int.up : Vector2Int.down;

				}

				// Gap is even, no extrusion needed
			} else {

				if (previousDirection.IsVertical ()) {
					diagonalLength = absDelta.x - 1;
					diagonalAddition = previousDirection.ToVector2Int () + (connectionStart.x < connectionEnd.x ? Vector2Int.right : Vector2Int.left);

				} else if (previousDirection.IsHorizontal ()) {
					diagonalLength = absDelta.y - 1;
					diagonalAddition = previousDirection.ToVector2Int () + (connectionStart.y < connectionEnd.y ? Vector2Int.up : Vector2Int.down);

				}
			}
		}

		connectionStart = previousDirection == Direction.North || previousDirection == Direction.East ? previousEnd : previousStart;
//		connectionEnd = nextDirection == Direction.North || nextDirection == Direction.East ? nextStart : nextEnd;
		connectionStart -= boundsMin;

		for (int d = 0; d < diagonalLength; d++) {
			connectionStart += diagonalAddition;
			SetRoad (map, connectionStart.x, connectionStart.y);
		}
	}
}

public class StartCoordinates
{
	public Vector3 playerPosition;
	public Vector3 rescueePosition;
	public Vector3 [] enemyPositions;
	public int[,] walkableMap;

	public StartCoordinates (Vector3 player, Vector3 rescuee, Vector3 [] enemies, int [,] walkable)
	{
		playerPosition 	= player;
		rescueePosition = rescuee;
		enemyPositions 	= enemies;
		walkableMap 	= walkable;
	}
}