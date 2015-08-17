using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class PossibilityCalculator
{
	string simpleBoardTileStates;

	public PossibilityCalculator ()
	{
		simpleBoardTileStates = "";

		Initialize ();
		BinaryStuff ();
	}


	public void Initialize()
	{
		System.Random rnd = new System.Random ();
		for (int i = 0; i < 64; i++) 
		{
			simpleBoardTileStates += rnd.Next (3);
		}
	}

	public static int NumberOfPossibilities(int n, int r)
	{
		return (int)(factorial (n) / (factorial (n - r) * factorial (r)));
	}

	public static double factorial(int numberInt)
	{
		double result = numberInt;

		for (int i = 1; i < numberInt; i++) 
		{
			result = result * i;
		}
		return result;
	}

	public void BinaryStuff()
	{
		int board = 0;
		board = 1 << 1;
		var binary = Convert.ToString (board, 2);
			Debug.Log(binary);
	}
}