using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreeEyedGames;

public class LeftHand : MonoBehaviour {
    
    public Transform paper;
    public Transform paperStack;
    public Transform paperStackAir;
    public MeshRenderer[] catPaw = new MeshRenderer[2];
    public GameObject pawMark;
    public Transform markPosition;
    
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
                GameObject newPawMark = Instantiate(pawMark);
                newPawMark.transform.parent = newPaper;
                newPawMark.transform.position = markPosition.position;
                newPawMark.transform.localPosition = new Vector3(newPawMark.transform.localPosition.x, 0, newPawMark.transform.localPosition.z);
                newPawMark.transform.rotation = markPosition.rotation;
                newPawMark.GetComponent<Decal>().LimitTo = newPaper.gameObject;
                Debug.Log("paperInstatiated");
                newPaper.position = new Vector3(other.transform.position.x, other.transform.position.y + paperStack.localScale.y* 0.01f + 0.01f, other.transform.position.z);
                paperStack.localScale = new Vector3(1, paperStack.localScale.y - 1, 1);
                Level.currentWork++;
                if(paperStack.localScale.y <1)
                {
                    paperStack.gameObject.SetActive(false);
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
            catPaw[0].enabled = false;
            catPaw[1].enabled = true;
        }
        else if(isStampReady == true && other.tag != "hand" && other.transform.root.gameObject.tag != "playerDesk" && other.transform.gameObject.tag != "paperStackAir")
        {
            GameObject newPawMark = Instantiate(pawMark);
            newPawMark.transform.parent = other.transform;
            newPawMark.transform.position = markPosition.position;
            newPawMark.transform.rotation = markPosition.rotation;
            newPawMark.GetComponent<Decal>().LimitTo = other.gameObject;
            isStampReady = false;
            catPaw[0].enabled = true;
            catPaw[1].enabled = false;
            Debug.Log("marked!");
        }
        else if(isStampReady == true && other.tag != "hand" && other.transform.root.gameObject.tag == "playerDesk")
        {
            GameObject newPawMark = Instantiate(pawMark);
            newPawMark.transform.parent = other.transform;
            newPawMark.transform.position = markPosition.position;
            newPawMark.transform.rotation = markPosition.rotation;
            isStampReady = false;
            catPaw[0].enabled = true;
            catPaw[1].enabled = false;
            Debug.Log("marked!");
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "stamp")
        {
            isStampReady = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "stamp")
        {
            isStampReady = true;
            Debug.Log("stampReady!");
        }
    }
}
