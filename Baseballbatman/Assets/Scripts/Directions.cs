using UnityEngine;

public enum Direction
{
	North,
	NorthEast,
	East,
	SouthEast,
	South,
	SouthWest,
	West,
	NorthWest
}

public static class Directions
{
	public const int Count = 8;

	public static Direction Opposite (this Direction original)
	{
		return (Direction)(((int)original + (Count / 2)) % Count);
	}

	public static Direction Clockwise (this Direction original, bool majorOnly = true)
	{
		if (majorOnly) {
			// Return next major direction
			return (Direction)((((int)original + 2) / 2 * 2) % Count);
		} else {
			// Return next whatever direction
			return (Direction)(((int)original + 1) % Count);
		}
	}

	public static bool IsHorizontal (this Direction direction)
	{
		return direction == Direction.East || direction == Direction.West;
	}

	public static bool IsVertical (this Direction direction)
	{
		return direction == Direction.North || direction == Direction.South;
	}

	private static readonly Vector2Int[] vector2IntArray = {
		Vector2Int.up,
		Vector2Int.up + Vector2Int.right,
		Vector2Int.right,
		Vector2Int.right + Vector2Int.down,
		Vector2Int.down,
		Vector2Int.down + Vector2Int.left,
		Vector2Int.left,
		Vector2Int.left + Vector2Int.up
	};

	public static Vector2Int ToVector2Int (this Direction direction)
	{
		return vector2IntArray [(int)direction];
	}


	// Does this make even sense
	public static int Sign (this Direction direction)
	{
		return (int)direction < Count / 2 ? 1 : -1;
	}
}