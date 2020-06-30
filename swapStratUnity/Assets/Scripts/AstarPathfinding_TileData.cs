using UnityEngine;
using System.Collections;

public class AstarPathfinding_TileData : MonoBehaviour {
	public int id;
	public int x_pos;
	public int y_pos;
	public int parent_id = -1;
	public int h_heuristicValue;
	public int g_movementCost;
	public int f_totalCost;
	public bool isTarget;
	public bool isObstacle;
	
	public int north;
	public int south;
	public int west;
	public int east;
}
