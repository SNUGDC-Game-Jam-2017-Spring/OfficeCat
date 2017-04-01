using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHand : MonoBehaviour {

    public Transform paperPsition;

    Transform rightHandPosition;
    public bool isInStampPosition;
    float distance;

    private void Awake()
    {
        rightHandPosition = GetComponent<Transform>();
    }

    void Start () {
		
	}
	

	void Update ()
    {
        distance = Vector3.Distance(paperPsition.position, rightHandPosition.position);
		if(distance <= 0.05 && isInStampPosition == false)
        {
            isInStampPosition = true;
        }
        else
        {
            isInStampPosition = false;
        }
	}
}
