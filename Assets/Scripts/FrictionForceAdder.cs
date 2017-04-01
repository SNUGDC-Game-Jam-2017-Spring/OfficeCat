using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrictionForceAdder : MonoBehaviour {
    
    Vector3 frictionForce;

    Vector3 lastPostion;

    SteamVR_TrackedObject trackedObj;
    FixedJoint joint;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }
    void Start ()
    {
        
        lastPostion = transform.position;
	}
	

	void Update ()
    {

	}

    
}
