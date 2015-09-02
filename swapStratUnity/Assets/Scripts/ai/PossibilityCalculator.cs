using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Facet.Combinatorics;

public class PossibilityCalculator
{
	string simpleBoardTileStates;

	public PossibilityCalculator ()
	{
		simpleBoardTileStates = "";

		Initialize ();
		BinaryStuff ();
	}

	public static void NumberOfPermutationsWithOutRepetition(int n /*number of tiles for this possibility bubble*/, int r/*number of tokens in this possibility bubble*/)
	{
		char[] inputSet = new char[n];
		for (int i = 0; i < r; i++)
		{
			inputSet[i] = 't';
		}
		for (int i = r; i < n; i++)
		{
			inputSet[i] = 'e';
		}
		
		Permutations<char> P1 = new Permutations<char>(inputSet, 
		                                               GenerateOption.WithoutRepetition);
		string format1 = "Permutations of {{A A C}} without repetition; size = {0}";
		Debug.Log(String.Format(format1, P1.Count));
//		foreach(IList<char> p in P1) {
//			Debug.Log(String.Format("{{{0} {1} {2}}}", p[0], p[1], p[2]));
//		}
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