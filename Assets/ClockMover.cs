using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockMover : MonoBehaviour {
	void Update ()
    {
        transform.Rotate(0, 3f * Time.deltaTime , 0);
	}
}
