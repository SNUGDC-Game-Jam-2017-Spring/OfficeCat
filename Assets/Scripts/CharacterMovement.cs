using UnityEngine;
using DG.Tweening;

public class CharacterMovement : MonoBehaviour {
	public Transform head;
	public float speed = 5f;
	public float targetDistance = 0.1f;
	public float maxHeadRotation = 60f;
	public float headRotationSpeed = 90f;
	public Transform currentTarget;
	Animator anim;
	Rigidbody body;
	WayPointParent wayParent;
	int LookAroundDirection = 1;
	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		anim = GetComponentInChildren<Animator>();
		var behaviours = anim.GetBehaviours<BossAnimBehaviour>();
		foreach(var behaviour in behaviours)
		{
			behaviour.movement = this;
		}
		body = GetComponent<Rigidbody>();
		wayParent = FindObjectOfType<WayPointParent>();
		currentTarget = wayParent.GetNextTarget();
	}
	public void Forward(float speedMultiplier = 1f)
	{
		body.velocity = transform.forward * speed * speedMultiplier;
		transform.LookAt(currentTarget.position);
	}
	public void LookAround()
	{
		head.DOKill();
		var rot = head.localRotation.eulerAngles;
		var currntRotation = -Mathf.DeltaAngle(rot.y,0);
		if(currntRotation * LookAroundDirection > maxHeadRotation)
		{
			currntRotation = maxHeadRotation * LookAroundDirection;
			LookAroundDirection *= -1;
		}
		currntRotation += headRotationSpeed * Time.deltaTime * LookAroundDirection;
		rot.y = currntRotation;
		head.localRotation = Quaternion.Euler(rot);
	}
	public void TurnToNextWaypoint()
	{
		var headRotation = head.rotation;
		transform.LookAt(currentTarget.position);
		head.rotation = headRotation;
		head.DOLocalRotate(Vector3.zero,0.4f);
	}
	public void SetAngry()
	{
		anim.SetTrigger("angry");
	}
	public void CheckPlayerWorking()
	{
		if(!GameController.instance.isWorking 
		&& GameController.instance.paperStackResizable.gameObject.activeInHierarchy)
		{
			SetAngry();
			Debug.Log("PlayerIsNotWorking!");
		}
		GameController.instance.AddWork();
	}
	public bool isPositionInBossView(Vector3 target)
	{
		var deg = Vector3.Angle(head.transform.forward, target - head.transform.position);
		Debug.Log("Boss View Degree : "+deg);
		return deg < 90;
	}
	public void LookAtPlayer()
	{
		var vrCam = FindObjectOfType<SteamVR_Camera>();
		transform.LookAt((Vector2)vrCam.head.position);
		head.localRotation = Quaternion.identity;
	}
	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void FixedUpdate()
	{
		// if(!anim.GetBool("move")) return;
		if(currentTarget == null)
		{
			currentTarget = wayParent.GetNextTarget();
		}
		var distance = currentTarget.position - transform.position;
		if(distance.magnitude < targetDistance)
		{
			var way = currentTarget.GetComponent<WayPoint>();
			currentTarget = wayParent.GetNextTarget();
			if(way != null)
			{
				way.Arrive(anim);
			}
		}
	}
}
