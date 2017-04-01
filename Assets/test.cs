using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {

    public GameObject parent;
	void Start () {
		
	}
	
	void Update () {
        transform.localScale = parent.transform.localScale;
	}
}
