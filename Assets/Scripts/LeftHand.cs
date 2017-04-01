using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHand : MonoBehaviour {
    
    public Transform paper;
    public Transform paperStack;
    public Transform paperStackAir;
    public MeshRenderer[] catPaw = new MeshRenderer[2];
    
    bool isInPaperPosition;
    public bool isStampReady;
    public bool isPassPaperStackAir;
    

    void Start ()
    {
	}
	

	void Update ()
    {
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "paperStackAir" && isStampReady == true)
        {
            isPassPaperStackAir = true;
        }
        if(other.tag == "paperStack")
        {
            if(isStampReady && isPassPaperStackAir)
            {
                Transform newPaper = Instantiate(paper) as Transform;
                Debug.Log("paperInstatiated");
                newPaper.position = new Vector3(other.transform.position.x, other.transform.position.y + paperStack.localScale.y* 0.01f + 0.01f, other.transform.position.z);
                paperStack.localScale = new Vector3(1, paperStack.localScale.y - 1, 1);
                if(paperStack.localScale.y <1)
                {
                    Destroy(paperStack.gameObject);
                }
                isStampReady = false;
                isPassPaperStackAir = false;
                paperStackAir.localPosition = new Vector3(0, paperStackAir.localPosition.y - 0.01f, 0);
                catPaw[0].enabled = true;
                catPaw[1].enabled = false;
            }
        }
        else if(other.tag == "stamp")
        {
            isStampReady = true;
            catPaw[0].enabled = false;
            catPaw[1].enabled = true;
        }
    }
}
