using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockMover : MonoBehaviour {
	void Update ()
    {
        Debug.Log(Level.currentTime);
        transform.Rotate(0, 3f * Time.deltaTime , 0);
	}
}
