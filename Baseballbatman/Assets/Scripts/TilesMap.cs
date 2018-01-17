using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesMap
{
	// full with all edges
	static int[] _0 = {
		0
	};
	// U-shape
	static int[] _1 = {
		1, 2, 3, 4, 6, 7, 8, 9, 16, 20, 32, 40, 41,
		64, 96, 128, 144, 148, 160, 192, 224, 
	};
	// Full with two sides
	static int[] _2 = { 
		5, 12, 13, 17, 21, 24, 25, 28, 29, 34,
		35, 39, 44, 45, 56, 57, 60, 61, 66, 67, 70, 
		71, 98, 99, 102, 103, 130, 131, 134, 135, 145, 149, 152, 
		153, 156, 157, 162, 163, 166, 167, 184, 185, 188, 
		189, 194, 195, 198, 199, 226, 227, 230, 231, 
	};
	// Corner with small corner
	static int[] _3 = {
		10, 14, 18, 19, 42, 72, 73, 80, 84, 100, 112, 146,
		168, 176, 193, 196, 200, 240, 
	};
	// Corner without other side
	static int[] _4 = { 
		11, 15, 22, 43, 104, 105, 150, 208, 212,
		232,
	};
	// Full with no edges
	static int[] _5 = {
		255
	};
	// Diagonal without other side
	static int[] _6 = {
		47, 151, 233, 244
	};
	// Diagonal with other side
	static int[] _7 = {
		33, 36, 46, 48, 52, 65, 68, 97, 116, 129,
		132, 136, 137, 147, 161, 164, 169, 180, 201, 225, 228, 
	};
	// Full with one edge
	static int[] _8 = {
		23, 31, 63, 107, 111, 159, 191, 214,
		215, 235, 239, 246, 247, 248, 249, 252, 253
	};
	// Full with two opposite corners
	static int [] _9 = {
		126, 219
	};
	// Full full with all corners
	static int[] _10 = {
		90
	};
	// Full with three corners
	static int[] _11 = {
		91, 94, 122, 174, 206, 218
	};
	// Edge with southeast corner
	static int[] _12 = {
		30, 62, 75, 79, 108, 109, 120, 121, 125,
		139, 143, 158, 171, 172, 173,  190, 203, 210,
		211, 236, 237, 242, 245,
	};
	// Edge with two corners
	static int[] _13 = { 
		26, 37, 49, 50, 53, 58, 69, 74, 76, 77, 
		78, 81, 82, 83, 85, 88, 89, 92, 93,  
		101, 113, 114, 115, 117, 133, 138, 140, 141,
		142, 154, 165, 170, 177, 178, 179, 186, 197, 202,
		204, 205, 229
	};
	// Edge with northeast corner
	static int[] _14 = { 
		27, 38, 51, 54, 55, 59, 86, 87, 106, 110, 118,
		119, 124, 155, 181, 182, 183, 187, 209, 213, 216, 217, 220, 221,
		234, 238, 241, 243, 
	};
	// Full with one corners
	static int[] _15 = {
		127, 223, 251, 254
	};
	// Full with two corners 
	static int[] _16 = {
		95, 123, 175, 207, 222,  250
	};


	static int[][] TileIndexes = {
		_0, _1, _2, _3, _4, _5, 
		_6, _7, _8, _9, _10, _11,
		_12, _13, _14, _15, _16
	};

	public static int GetTileIndex (int surroundingTiles)
	{
		for (int i = 0; i < TileIndexes.Length; i++) {
			if (System.Array.Exists (TileIndexes [i], (x) => x == surroundingTiles)) {
				return i;
			}
		}
		return -1;
	}

	static int [] rot0 = {
		0, 32, 33, 34, 35, 39, 64, 65, 66, 67, 70, 71, 72, 73, 74, 75, 78, 79, 90, 96, 96, 97, 98, 99,
		102, 103, 104, 105, 106, 107, 110, 111, 122, 126, 128, 129, 130, 131, 136, 137, 138, 139, 142,
		143, 160, 161, 162, 163, 166, 167, 168, 169, 170, 171, 174, 175, 192, 193, 194, 195, 198, 199, 
		200, 201, 202, 203, 207, 207, 222, 223, 224, 225, 226, 227, 230, 231, 232, 233, 234, 235, 238,
		239, 255
	};

	static int [] rot1 = {
		1, 5, 8, 9, 10, 11, 12, 13, 14, 15, 17, 21, 24, 25, 26, 27, 28, 29, 30, 31, 40, 41, 42, 43, 46, 47,
		56, 57, 58, 59, 60, 61, 62, 63, 91, 151, 152, 153, 154, 155, 156, 157, 158, 159, 184, 185, 186, 187,
		188, 189, 190, 191, 219, 250, 254, 
	};

	static int[] rot2 = {
		2, 3, 6, 7, 18, 19, 22, 23, 38, 50, 51, 54, 55, 82, 83, 86, 87, 114, 115, 118, 119, 123, 134,
		135, 146, 147, 150, 151, 178, 179, 182, 183, 210, 211, 214, 215, 242, 243, 246, 247, 251
	};

	static int[] rot3 = {
		4, 16, 20, 36, 37, 44, 45, 48, 49, 52, 53, 68, 69, 76, 77, 80, 81, 84, 85, 88, 89, 92, 93, 95,
		100, 101, 108, 109, 112, 113, 116,117, 120, 121, 124, 125, 127, 132, 133, 140, 141, 144, 145,
		148, 149, 164, 165, 172, 173, 176, 177, 180, 181, 196, 197, 204, 205, 208, 209, 212, 213, 216,
		217, 218, 220, 221, 228, 229, 236, 237, 240, 241, 244, 245, 248, 249, 252, 253, 
	};

	static int[][] RotationIndexes = {
		rot0, rot1, rot2, rot3
	};

	public static int GetRotationIndex (int surroundingTiles)
	{
//		return 0;
		for (int i = 0; i < RotationIndexes.Length; i++) {
			if (System.Array.Exists (RotationIndexes [i], (x) => x == surroundingTiles)) {
				return i;
			}
		}
		return 0;
	}

//	static int[] yesAddTileToUnder = {
//		32,33, 34,35, 36, 37, 38, 39,
//		48, 49, 50, 51, 52, 53, 54, 55,
//		128, 129, 130, 131, 132, 133, 134, 135,
//		160, 161, 162, 163, 164, 165, 166, 167,
//		168, 169, 170, 171, 172, 173, 174, 175,
//		180, 181, 182, 183, 
//	};
//
//	public static bool AddTileToUnder (int surroundingTilesMask)
//	{
//		return (System.Array.Exists (yesAddTileToUnder, (x) => x == surroundingTilesMask));
//	}
}

