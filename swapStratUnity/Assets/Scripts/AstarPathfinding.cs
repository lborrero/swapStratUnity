using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AstarPathfinding : MonoBehaviour {

	List<AstarPathfinding_TileData> board_map = new List<AstarPathfinding_TileData>();

	int boardWidth;

	List<int> open_list = new List<int>();
	List<int> closed_list = new List<int>();

	int checkingNode_id;
	int source_id;
	int target_id;

	bool foundtarget = false;
	public int baseMovementCost = 1;

	public void GenerateBoard(List<int> intArray, int width2, int _source_id, int _target_id)
	{
		board_map.Clear ();

		checkingNode_id = _source_id;
		source_id = _source_id;
		target_id = _target_id;

		boardWidth = width2;

		for(int i=0; i<intArray.Count; i++)
		{
			AstarPathfinding_TileData td = new AstarPathfinding_TileData();
			td.id = i;
			td.x_pos = ContiguousBlockSearch.indexToCoordX(i, width2);
			td.y_pos = ContiguousBlockSearch.indexToCoordY(i, width2);
			td.isTarget = (_target_id==i);
			td.isObstacle = (intArray[i]==1);

			td.parent_id = -1;

			int totalHeuristicCost = 0;
			totalHeuristicCost = totalHeuristicCost + Math.Abs(td.x_pos - ContiguousBlockSearch.indexToCoordX(target_id, width2));
			totalHeuristicCost = totalHeuristicCost + Math.Abs(td.y_pos - ContiguousBlockSearch.indexToCoordY(target_id, width2));
			td.h_heuristicValue = totalHeuristicCost;

			//right;
			int counter = i;
			++counter;
			int checkerInt = (counter)%(width2);
			if (checkerInt == 0 || counter > intArray.Count-1)
				td.east = -1;
			else
				td.east = counter;

			//left
			counter = i;
			--counter;
			checkerInt = (counter+1)%(width2);
			if (checkerInt == 0)
				td.west = -1;
			else
				td.west = counter;

			//top
			counter = i;
			counter = counter - width2;
			if (counter < 0)
				td.north = -1;
			else
				td.north = counter;

			//bottom
			counter = i;
			counter = counter + width2;
			if (counter > intArray.Count-1)
				td.south = -1;
			else
				td.south = counter;


			board_map.Add(td);
		}
		printStuff ();
	}

	void printStuff()
	{
		string toPrint = "";
		for (int i =0; i<board_map.Count; i++)
		{
			if(i%boardWidth == 0)
				toPrint += "\n";
			toPrint += " " + board_map[i].parent_id.ToString() + ",";
		}
//		Debug.Log (toPrint);
	}

	public void ComputePathSequence()
	{
		int whileCounter = 0;
		while(foundtarget == false && whileCounter < 50)
		{
			FindPath();
			printStuff ();
			whileCounter++;
		}

//		Debug.Log ("whileCounter: " + whileCounter);

		if(foundtarget == true)
			printList(TraceBackPath());
	}

	void FindPath()
	{
//		Debug.Log ("FindPath: A: " + checkingNode_id);
		if(foundtarget == false)
		{
//			Debug.Log ("FindPath: B");
			if(board_map[checkingNode_id].north != -1)
				DetermineNodeValue(checkingNode_id, board_map[checkingNode_id].north);

			if(board_map[checkingNode_id].east != -1)
				DetermineNodeValue(checkingNode_id, board_map[checkingNode_id].east);

			if(board_map[checkingNode_id].south != -1)
				DetermineNodeValue(checkingNode_id, board_map[checkingNode_id].south);

			if(board_map[checkingNode_id].west != -1)
				DetermineNodeValue(checkingNode_id, board_map[checkingNode_id].west);

			if(foundtarget == false)
			{
//				Debug.Log ("FindPath: C");

				AddToClosedList(checkingNode_id);
				RemoveFromOpenList(checkingNode_id);

				if(open_list.Count > 0)
					checkingNode_id = GetSmallestFValue();
			}
		}
	}

	int GetSmallestFValue()
	{
		int smallestFValue = 100000;
		int smalleFValue_id = -1;
//		Debug.Log ("GetSmallestFValue: ");
//		printList(open_list);
		for(int i = 0; i<open_list.Count; i++)
		{
			if(!board_map[open_list[i]].isObstacle && board_map[open_list[i]].f_totalCost < smallestFValue)
			{
				smallestFValue = board_map[open_list[i]].f_totalCost;
				smalleFValue_id = open_list[i];
			}
		}
		return smalleFValue_id;
	}

	void GetSmallestFValue(int node)
	{
		int lowestFValue = 0;
	}

	void AddToClosedList(int node)
	{
			closed_list.Add (node);
	}

	void RemoveFromOpenList(int node)
	{
			open_list.Remove (node);
	}

	void DetermineNodeValue(int currentNode, int testing)
	{
//		Debug.Log ("DetermineNodeValue: A: " + checkingNode_id + " " + testing);
//		printList (open_list);
//		printList (closed_list);
		//don't work on null tiles
		if (testing == -1)
		{
//			Debug.Log ("DetermineNodeValue: B");
			return;
		}

		//check to see if the node is the target
		if (testing == target_id)
		{
//			Debug.Log ("DetermineNodeValue: C");
			board_map [target_id].parent_id = currentNode;
			foundtarget = true;
			return;
		}

		//ignore obstacles
		if (board_map [testing].isObstacle == true)
		{
//			Debug.Log ("DetermineNodeValue: D");
			return;
		}

		if(closed_list.Contains(testing) == false)
		{
//			Debug.Log ("DetermineNodeValue: E");
			if(open_list.Contains(testing) == true)
			{
				int newCost = board_map[currentNode].g_movementCost + baseMovementCost;
//				Debug.Log ("DetermineNodeValue: F: " + newCost + "<" + board_map[testing].g_movementCost);

				if(newCost < board_map[testing].g_movementCost)
				{
//					Debug.Log ("DetermineNodeValue: G");
					board_map[testing].parent_id = currentNode; //setting parent
					board_map[testing].g_movementCost = newCost; //setting g value
					board_map[testing].f_totalCost = board_map[testing].h_heuristicValue + newCost;//calculate f cost
				}
			}
			else
			{
//				Debug.Log ("DetermineNodeValue: H");
				int newCost = board_map[currentNode].g_movementCost + baseMovementCost;

				board_map[testing].parent_id = currentNode; //setting parent
				board_map[testing].g_movementCost = newCost; //setting g value
				board_map[testing].f_totalCost = board_map[testing].h_heuristicValue + newCost;//calculate f cost

				AddToOpenList(testing);
			}
		}
	}

	void AddToOpenList(int node)
	{
		if(!open_list.Contains(node))
			open_list.Add (node);
	}

	public List<int> TraceBackPath()
	{
		List<int> path = new List<int> ();
		int node = target_id;
		do
		{
			path.Add(node);
			node = board_map[node].parent_id;
		}while(node != -1);

		path.Reverse ();
		return path;
	}

	void printList(List<int> listToPrint)
	{
		string toPrint = "";
		for (int i =0; i<listToPrint.Count; i++)
		{
			toPrint += " " + listToPrint[i].ToString() + ",";
		}
		Debug.Log (toPrint);
	}
}
