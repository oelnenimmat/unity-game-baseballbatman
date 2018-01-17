using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
	This class provides pathfinding. It takes care of grid itself, path computing and requests.
	There can be only one pathfinder at a time, which is suitable for this game.
	Use static request path to get path.
	It uses a* algorithm.
*/
public class Pathfinder
{
	private static Pathfinder instance;

	private Node [,] nodeGrid;

	private Vector2 cellSize;
	private int width;
	private int height;
//	public int width { get; private set; }
//	public int height { get; private set; }

	private const int straightCost = 10;
	private const int diagonalCost = 14;

	// Call this once per frame
	public static void Update ()
	{
		if (instance != null) {
			instance.DoProcess ();
		}
	}


	// Use this to create new pathfinder object. Overrides previous instances
	// and creates new grid.
	public static void Create (int [,] walkableMap, Vector2 gridSize)
	{
		instance = new Pathfinder (walkableMap, gridSize);
	}

	private Pathfinder (int [,] walkableMap, Vector2 gridSize)
	{
		int xCells = (int)gridSize.x;
		int yCells = (int)gridSize.y;

		this.cellSize = new Vector2(gridSize.x / xCells, gridSize.y / yCells);

		width 	= walkableMap.GetLength (0) * xCells;
		height 	= walkableMap.GetLength (1) * yCells;

		nodeGrid = new Node[width, height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				bool walkable = walkableMap [x / xCells, y / yCells] == 1;
				nodeGrid [x, y] = new Node (x, y, walkable);
			}
		}
	}

	#region Requesting
	// Simple enough, request path from wherever, just give start and end positions and callback
	// Start position is not included on path. Also, request is added to queue, and will be processed
	// when it is its turn, so its not necessarily instant
	public static void RequestPath (Vector2 start, Vector2 end, Action<Vector2[], EventArgs> callback, EventArgs e)
	{
		instance.pathRequestQueue.Enqueue (new PathRequest (start, end, callback, e));
	}

	private struct PathRequest
	{
		public readonly Vector2 start;
		public readonly Vector2 end;
		public readonly Action<Vector2[], EventArgs> callback;
		public readonly EventArgs e;

		public PathRequest (Vector2 start, Vector2 end, Action<Vector2[], EventArgs> callback, EventArgs e)
		{
			this.start 		= start;
			this.end 		= end;
			this.callback 	= callback;
			this.e 			= e;
		}
	}

	private Queue<PathRequest> pathRequestQueue = new Queue<PathRequest> ();

	// Only ever assign FindPath to this. How to restrict this
	private IEnumerator currentProcess = null;
	private int processCount = 1;

	private void DoProcess ()
	{

		for (int i = 0; i < processCount; i++) {

			// If there is nothing to process, leave
			if (currentProcess == null && pathRequestQueue.Count == 0) {
				return;
			}

			if (currentProcess == null) {
				if (pathRequestQueue.Count > 0) {
					currentProcess = FindPath (pathRequestQueue.Dequeue ());
					currentProcess.MoveNext ();
				}

			} else {
				// There are only two steps in process, one of which is already used
				// So free up the slot for next process
				currentProcess.MoveNext ();
				currentProcess = null;
			}
		}
	}
	#endregion

	#region Pathfinding
	private class Node : IHeapItem<Node>
	{
		// cost to start
		public int gCost;

		// cost to end
		public int hCost;

		// total cost
		public int fCost { get { return gCost + hCost; } }

		public int x, y;
		public bool walkable;
		public Node parent;

		public Node (int x, int y, bool walkable)
		{
			this.x = x;
			this.y = y;
			this.walkable = walkable;
		}

		public int HeapIndex { get; set; }

		public int CompareTo (Node other)
		{
			int compare = fCost.CompareTo (other.fCost);
			if (compare == 0) {
				compare = hCost.CompareTo (other.hCost);
			}
			return -compare;
		}
	}

	private Vector2Int NodeCoordinate (Vector2 position)
	{
		return new Vector2Int (
			(int)(position.x / cellSize.x),
			(int)(position.y / cellSize.y)
		);
	}

	private Vector2 NodePosition (int x, int y)
	{
		return new Vector2 (
			(x + 0.5f) * cellSize.x,
			(y + 0.5f) * cellSize.y
		);
	}

	private int GetMoveCost (Node a, Node b)
	{
		int deltaX = Mathf.Abs (a.x - b.x);
		int deltaY = Mathf.Abs (a.y - b.y);

		if (deltaX > deltaY) {
			return diagonalCost * deltaY + straightCost * (deltaX - deltaY);
		} else {
			return diagonalCost * deltaX + straightCost * (deltaY - deltaX);
		}
	}

	private IEnumerator FindPath (PathRequest request)
	{
		Vector2Int startCoord 	= NodeCoordinate (request.start);
		Vector2Int endCoord 	= NodeCoordinate (request.end);

		Node startNode 	= nodeGrid [startCoord.x, startCoord.y];
		Node endNode 	= nodeGrid [endCoord.x, endCoord.y];

		Heap<Node> openSet = new Heap<Node> (nodeGrid.GetLength(0) * nodeGrid.GetLength (1));
		HashSet <Node> closedSet = new HashSet<Node> ();

		openSet.Add (startNode);

		bool pathfound = false;
		while (openSet.Count > 0) {
			
			Node currentNode = openSet.RemoveFirst ();
			closedSet.Add (currentNode);

			if (currentNode == endNode) {
				pathfound = true;
				break;
			}

			// Look through all neighbour nodes
			for (int y = currentNode.y - 1; y <= currentNode.y + 1; y++) {
				for (int x = currentNode.x - 1; x <= currentNode.x + 1; x++) {

					// Skip if current node or outside map
					if (
						(x == currentNode.x && y == currentNode.y) ||
						x < 0 || x >= nodeGrid.GetLength (0) ||
						y < 0 || y >= nodeGrid.GetLength (1)
					){
						continue;
					}
						
					Node neighbourNode = nodeGrid [x, y];

					// Also skip if node is not walkable or it is already closed
					if (!neighbourNode.walkable || closedSet.Contains (neighbourNode)) {
						continue;
					}


					// Calculate move cost
					int newMoveCostToNeighbour = currentNode.gCost + GetMoveCost (currentNode, neighbourNode);

					// If neighbour has not been checked yet, or if we now found faster path to it
					if (!openSet.Contains (neighbourNode) || 
						newMoveCostToNeighbour < neighbourNode.gCost
					) {
						neighbourNode.gCost = newMoveCostToNeighbour;
						neighbourNode.hCost = GetMoveCost (neighbourNode, endNode);
						neighbourNode.parent = currentNode;

						if (!openSet.Contains (neighbourNode)) {
							openSet.Add (neighbourNode);
						} else {
							openSet.UpdateItem (neighbourNode);
						}
					}
				}
			}
		}

		yield return null;
		Vector2 [] path = null;
		if (pathfound) {
			
			// Retrace path 
			List<Vector2> pathList = new List<Vector2> ();
			Node currentNode = endNode;

			while (currentNode != startNode) {
				pathList.Add (NodePosition (currentNode.x, currentNode.y));
				currentNode = currentNode.parent;
			}


			if (pathList.Count > 0) {
				List<Vector2> simplifiedReversedPathList = new List<Vector2> ();
				Vector2 oldDirection = Vector2Int.zero;

				for (int i = pathList.Count - 1; i >= 1; i--) {
				
					Vector2 newDirection = pathList [i - 1] - pathList [i];
					if (newDirection != oldDirection) {
						simplifiedReversedPathList.Add (pathList [i]);
						oldDirection = newDirection;
					}
				
				}
				simplifiedReversedPathList.Add (pathList [0]);
				path = simplifiedReversedPathList.ToArray ();
			}

		}
		request.callback (path, request.e);
	}
	#endregion
}