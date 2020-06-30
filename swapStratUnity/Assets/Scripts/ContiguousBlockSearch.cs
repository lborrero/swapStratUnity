using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ContiguousBlockSearch : MonoBehaviour {

	public static bool verifyDouble(List<int> v, int valueToErase){
		for (int i=v.Count-1; i>=0; i--) {
			if(v[i] == valueToErase){
				return true;
			}
		}
		return false;
	}
	
	public static void recursiveRight(List<int> idCollector, List<int> arrs, int counter, int width, int height){
		int checkerInt = (counter+1)%(width);
		//cout << "recursiveRight: " << checkerInt << endl;
		if (arrs[counter] == 0 || checkerInt == 0 || counter > (width*height)-1) {
			return;
		}else{
			//cout << &idCollector << " ";
			if(!verifyDouble(idCollector, counter)){
				idCollector.Add(counter);
			}
			//cout << counter << ": " << arrs[counter] << endl;
			recursiveRight(idCollector, arrs, ++counter, width, height);
			return;
		}
	}
	
	static void recursiveLeft(List<int> idCollector, List<int> arrs, int counter, int width, int height){
		int checkerInt = (counter+1)%(width);
		//cout << "recursiveLeft: " << checkerInt << endl;
		if (arrs[counter] == 0 || checkerInt == 0) {
			return;
		}else{
			//cout << &idCollector << " ";
			if(!verifyDouble(idCollector, counter)){
				idCollector.Add(counter);
			}
			//cout << counter << ": " << arrs[counter] << endl;
			recursiveLeft(idCollector, arrs, --counter, width, height);
			return;
		}
	}
	
	static void recursiveTop(List<int> idCollector, List<int> arrs, int counter, int width, int height){
		//cout << "recursiveTop: " << counter << endl;
		if (arrs[counter] == 0 || counter < 0) {
			return;
		}else{
			//cout << &idCollector << " ";
			if(!verifyDouble(idCollector, counter)){
				idCollector.Add(counter);
			}
			//cout << counter << ": " << arrs[counter] << endl;
			counter = counter - width;
			recursiveTop(idCollector, arrs, counter, width, height);
			return;
		}
	}
	
	static void recursiveBottom(List<int> idCollector, List<int> arrs, int counter, int width, int height){
		//cout << "recursiveBottom: " << counter << endl;
		if (arrs[counter] == 0 || counter > (width*height)-1) {
			return;
		}else{
			//cout << &idCollector << " ";
			if(!verifyDouble(idCollector, counter)){
				idCollector.Add(counter);
			}
			//cout << counter << ": " << arrs[counter] << endl;
			counter = counter + width;
			recursiveBottom(idCollector, arrs, counter, width, height);
			return;
		}
	}
	
	public static void verifyTopBottomLeftAndRight(List<int> idCollector, List<int> intArray, int placement, int width, int height){
		recursiveRight(idCollector, intArray, placement, width, height);
		recursiveLeft(idCollector, intArray, placement, width, height);
		recursiveTop(idCollector, intArray, placement, width, height);
		recursiveBottom(idCollector, intArray, placement, width, height);
	}
	
	public static int coordToIndex(int xPos, int yPos, int width){
		return yPos*width+xPos;
	}
	
	public static int indexToCoordX(int coordIndex, int width){
		return coordIndex%width;
	}
	
	public static int indexToCoordY(int coordIndex, int width){
		return (int)Math.Floor((double)coordIndex/width);
	}
//	
//	static vector<int> giveMeRowMatchesForCoord(vector<int> &ids, int xPos, int yPos, int width, int height){
//		vector<int> returningIds;
//		
//		int goodId = coordToIndex(xPos, yPos, width);
//		bool goodIdDetected = false;
//		bool breakEverything = false;
//		
//		int initialCountPoint = yPos*width;
//		int finalCountPoint = (yPos+1)*width - 1;
//		
//		//cout << initialCountPoint << " to " << finalCountPoint << endl;
//		
//		for (int i=initialCountPoint; i<finalCountPoint; i++) {
//			bool inIds = false;
//			for (int j=0; j<ids.size(); j++) {
//				if (i == ids.at(j)) {
//					inIds = true;
//					//cout << "inIds: " << inIds << endl;
//				}
//			}
//			if (inIds) {
//				if (goodId == i) {
//					goodIdDetected = true;
//					//cout << "goodIdDetected: " << i << endl;
//				}
//				if(!breakEverything){
//					returningIds.push_back(i);
//				}
//				//cout << "push_back: " << i << endl;
//			}else{
//				if (goodIdDetected) {
//					//cout << "goodIdDetected check" << endl;
//					breakEverything = true;
//				}else{
//					//cout << "clearing: " << returningIds.size() << endl;
//					returningIds.clear();
//				}
//			}
//		}
//		
//		return returningIds;
//	}
//	
//	static vector<int> giveMeColomnMatchesForCoord(vector<int> &ids, int xPos, int yPos, int width, int height){
//		vector<int> returningIds;
//		
//		int goodId = coordToIndex(xPos, yPos, width);
//		bool goodIdDetected = false;
//		bool breakEverything = false;
//		
//		int initialCountPoint = xPos;
//		int finalCountPoint = xPos+((height-1)*width);
//		
//		//cout << initialCountPoint << " to " << finalCountPoint << endl;
//		
//		for (int i=initialCountPoint; i<finalCountPoint; i+=width) {
//			bool inIds = false;
//			for (int j=0; j<ids.size(); j++) {
//				if (i == ids.at(j)) {
//					inIds = true;
//					//cout << "inIds: " << i << endl;
//				}
//			}
//			if (inIds) {
//				if (goodId == i) {
//					goodIdDetected = true;
//					//cout << "goodIdDetected: " << i << endl;
//				}
//				if(!breakEverything){
//					returningIds.push_back(i);
//				}
//				//cout << "push_back: " << i << endl;
//			}else{
//				if (goodIdDetected) {
//					//cout << "goodIdDetected check" << endl;
//					breakEverything = true;
//				}else{
//					//cout << "clearing: " << returningIds.size() << endl;
//					returningIds.clear();
//				}
//			}
//		}
//		
//		return returningIds;
//	}
//	
	public static List<int> returnContiguousFromTile(List<int> intArray, int width2, int height2, int initialCoord_x2, int initialCoord_y2){
		int width = width2;
		int height = height2;
		
		int initialCoord_x = initialCoord_x2;
		int initialCoord_y = initialCoord_y2;

		List<int> giveMeIds = new List<int>();
		int previousGiveMeIdsSize = giveMeIds.Count;
		int placement = coordToIndex(initialCoord_x, initialCoord_y, width);
		
		verifyTopBottomLeftAndRight(giveMeIds, intArray, placement, width, height);
		
		while(giveMeIds.Count != previousGiveMeIdsSize){
			previousGiveMeIdsSize = giveMeIds.Count;
			for (int i=giveMeIds.Count-1; i>=0; i--) {
				verifyTopBottomLeftAndRight(giveMeIds, intArray, giveMeIds[i], width, height);
			}
		}
		return giveMeIds;
	}

	public static List<int> returnCardianlTilesFromTile(List<int> intArray, int width2, int height2, int initialCoord_x2, int initialCoord_y2){
		int width = width2;
		int height = height2;

		int initialCoord_x = initialCoord_x2;
		int initialCoord_y = initialCoord_y2;

		List<int> giveMeIds = new List<int>();
		int previousGiveMeIdsSize = giveMeIds.Count;
		int placement = coordToIndex(initialCoord_x, initialCoord_y, width);

		verifyCardinals(giveMeIds, intArray, placement, width, height);
		return giveMeIds;
	}

	public static void verifyCardinals(List<int> idCollector, List<int> intArray, int placement, int width, int height)
	{
			//right
			int right = placement+1;
			int rightChecker = (right+1)%(width);
			if (intArray [right] == 0 || rightChecker == 0 || right > (width * height) - 1) {
				
			} else {
				idCollector.Add (right);
			}

			//left
			int left = placement-1;
			int leftChecker = (left+1)%(width);
			if (intArray [left] == 0 || leftChecker == 0) {
				
			} 
			else 
			{
				idCollector.Add (left);
			}

			//top
			int top = placement - width;
			if (intArray[top] == 0 || top < width) {
				
			}
			else 
			{
				idCollector.Add (top);
			}

			//bottom
			int bottom = placement + width;
			if (intArray[bottom] == 0 || bottom > (width * height) - 1) {
			}
			else 
			{
				idCollector.Add (bottom);
			}
	}
}
