using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointParent : MonoBehaviour {
	int currentTarget = -1;
	int currentSet = -1;

	public Transform GetNextTarget()
	{
		if(currentSet == -1) ResetWaySet();
		currentTarget++;
		if(currentTarget >= transform.GetChild(currentSet).childCount)
		{
			currentTarget = 0;
			ResetWaySet();
		}
		return transform.GetChild(currentSet).GetChild(currentTarget);
	}
	void ResetWaySet()
	{
		currentSet = Random.Range(0,transform.childCount);
	}
}
