using System;
using UnityEngine;

public class BitMaskMap
{
	/*
			Supermap knows of itself and of 8 surrounding tiles
			Supermap needs these:
			 - rooms: 		bits 1 - 9
			 - roads: 		bits 11 - 19
			 - walls: 		bits 21 - 29
			 - walkable: 	bit 30, only this square

			 - checkedbit for regionfinding floodfill
			 	bit 32
			This leaves bits 10, 20,  unused
			
			First bit is always current tile, and rest eight are surroundin tiles, 
			from topleft, row by row.
		*/
	// These should be moved somewhere else, but now they are here for time being
	// these refer to surrounding tiles around current tile
	const int	topLeft 	= 1,
				topMid 		= 2,
				topRight	= 4,
				midLeft 	= 8,
				midRight	= 16,
				bottomLeft	= 32,
				bottomMid	= 64,
				bottomRight	= 128;

	// use these with surrounding tiles
	const int	groundShift = 1,
				roadShift = 11,
				wallShift = 21;

	// these refer to bit in which is stored if this tile has that tile in it
	const int 	groundBit = 1 << 0,
				roadBit = 1 << 10,
				wallBit = 1 << 20;

	int walkableBit = 1 << 29;

	// This is used in region separating to check if tile has been looked already
	int checkedBit = 1 << 31;
	public Vector2Int size { get; private set; }

	private Int32[,] map;

	public BitMaskMap (int sizeX, int sizeY) {
		map = new int[sizeX, sizeY];
		size = new Vector2Int (sizeX, sizeY);
	}

	public bool GetGround (int x, int y) {
		return (map [x, y] & groundBit) == groundBit;
	}

	public void SetGround	(int x, int y) {
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

	public void ClearGround	(int x, int y) {
		map [x, y] &= ~groundBit;

		map [x - 1, y - 1]	&= ~(topRight		<< groundShift);
		map [x, y - 1]		&= ~(topMid			<< groundShift);
		map [x + 1, y - 1]	&= ~(topLeft		<< groundShift);

		map [x - 1, y]		&= ~(midRight		<< groundShift);
		map [x + 1, y]		&= ~(midLeft		<< groundShift);

		map [x - 1, y + 1]	&= ~(bottomRight	<< groundShift);
		map [x, y + 1]		&= ~(bottomMid		<< groundShift);
		map [x + 1, y + 1]	&= ~(bottomLeft		<< groundShift);	

	}




	public bool GetRoad (int x, int y)
	{
		return (map [x, y] & roadBit) == roadBit;
	}

	public void SetRoad (int x, int y)
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

	public void ClearRoad (int x, int y)
	{
		map [x, y] &= ~roadBit;

		map [x - 1, y - 1]	&= ~(topRight		<< roadShift);
		map [x, y - 1]		&= ~(topMid			<< roadShift);
		map [x + 1, y - 1]	&= ~(topLeft		<< roadShift);

		map [x - 1, y]		&= ~(midRight		<< roadShift);
		map [x + 1, y]		&= ~(midLeft		<< roadShift);

		map [x - 1, y + 1]	&= ~(bottomRight	<< roadShift);
		map [x, y + 1]		&= ~(bottomMid		<< roadShift);
		map [x + 1, y + 1]	&= ~(bottomLeft		<< roadShift);	
	}




	public bool GetWall (int x, int y)
	{
		return (map [x, y] & wallBit) == wallBit;
	}

	public void SetWall (int x, int y)
	{
		map [x, y] |= wallBit;

		map [x - 1, y - 1]	|= (topRight	<< wallShift);
		map [x, y - 1]		|= (topMid		<< wallShift);
		map [x + 1, y - 1]	|= (topLeft		<< wallShift);

		map [x - 1, y]		|= (midRight	<< wallShift);
		map [x + 1, y]		|= (midLeft		<< wallShift);

		map [x - 1, y + 1]	|= (bottomRight	<< wallShift);
		map [x, y + 1]		|= (bottomMid	<< wallShift);
		map [x + 1, y + 1]	|= (bottomLeft	<< wallShift);
	}

	public void ClearWall (int x, int y)
	{
		map [x, y] &= ~wallBit;

		map [x - 1, y - 1]	&= ~(topRight		<< wallShift);
		map [x, y - 1]		&= ~(topMid			<< wallShift);
		map [x + 1, y - 1]	&= ~(topLeft		<< wallShift);

		map [x - 1, y]		&= ~(midRight		<< wallShift);
		map [x + 1, y]		&= ~(midLeft		<< wallShift);

		map [x - 1, y + 1]	&= ~(bottomRight	<< wallShift);
		map [x, y + 1]		&= ~(bottomMid		<< wallShift);
		map [x + 1, y + 1]	&= ~(bottomLeft		<< wallShift);	
	}

	public bool GetWalkable (int x, int y)
	{
		return (map [x, y] & walkableBit) == walkableBit;
	}

	public void SetWalkable (int x, int y)
	{
		map [x, y] |= walkableBit;
	}

}
