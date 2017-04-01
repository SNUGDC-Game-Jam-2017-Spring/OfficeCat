using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHand : MonoBehaviour {
    
    public Transform paper;
    public GameObject RightHand; 
    
    bool isInPaperPosition;
    public bool isStampReady;
    

    void Start ()
    {
		
	}
	

	void Update ()
    {
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "paperStack")
        {
            if(isStampReady)
            {
                Transform newPaper = Instantiate(paper) as Transform;
                Debug.Log("paperInstatiated");
                isStampReady = false;
            }
        }
    }
}
