using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointParent : MonoBehaviour {
	int currentTarget = -1;

	public Transform GetNextTarget()
	{
		currentTarget++;
		if(currentTarget >= transform.childCount)
		{
			currentTarget -= transform.childCount;
		}
		return transform.GetChild(currentTarget);
	}
}
