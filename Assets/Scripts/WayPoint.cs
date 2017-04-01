using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WayPoint : MonoBehaviour {
	public enum ActionOnArrival
	{
		Wait, Sit, Turn
	}
	public ActionOnArrival action;
	Animator anim;

	public void Arrive(Animator _anim)
	{
		anim = _anim;
		switch(action)
		{
			case ActionOnArrival.Wait:
			anim.SetBool("inseat",false);
			anim.SetBool("move",false);
			anim.transform.parent.DORotateQuaternion(transform.rotation, 1f);
			Invoke("Go",Random.Range(1f,6f));
			break;
			case ActionOnArrival.Sit:
			anim.SetBool("inseat",true);
			anim.SetBool("move",false);
			anim.transform.parent.DORotateQuaternion(transform.rotation, 1f);
			Invoke("Go",Random.Range(1f,6f));
			break;
			case ActionOnArrival.Turn:
			anim.transform.parent.DORotateQuaternion(transform.rotation, 1f);
			break;
		}
	}
	void Go()
	{
		anim.SetBool("move",true);
	}
}
