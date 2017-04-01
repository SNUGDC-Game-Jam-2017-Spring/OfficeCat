using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperController : MonoBehaviour {
    
    Rigidbody rb;

	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	}

    private void OnCollisionStay(Collision collision)
    {
        SteamVR_TrackedObject trackedObj = collision.transform.GetComponentInParent<SteamVR_TrackedObject>();
        if (trackedObj != null)
        {
            var device = SteamVR_Controller.Input((int)trackedObj.index);
            rb.velocity = -(device.velocity - transform.up.normalized * Vector3.Dot(transform.up.normalized, device.velocity));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<LeftHand>() != null)
        {
            LeftHand leftHand = other.GetComponent<LeftHand>();
            leftHand.isStampReady = false;
            leftHand.catPaw[0].enabled = true;
            leftHand.catPaw[1].enabled = false;
        }
    }
}
