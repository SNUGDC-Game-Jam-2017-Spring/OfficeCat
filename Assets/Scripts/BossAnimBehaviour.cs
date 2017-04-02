using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum BossActionType
{
	Forward, LookAround, TurnToNextPosition, LookForward, CheckPlayerWorking, AddAngryPoint, SetNotAngry
}
[System.SerializableAttribute]
public struct BossAction
{
	public BossActionType actionType;
}
public class BossAnimBehaviour : StateMachineBehaviour {
	public BossAction[] enter;
	public BossAction[] update;
	public BossAction[] exit;
	public CharacterMovement movement;

	void PlayAction(BossAction action, Animator anim)
	{
		switch(action.actionType)
		{
			case BossActionType.Forward:
			movement.Forward();
			break;
			case BossActionType.LookAround:
			movement.LookAround();
			break;
			case BossActionType.TurnToNextPosition:
			movement.TurnToNextWaypoint();
			break;
			case BossActionType.LookForward:
			movement.head.DOLocalRotate(Vector3.zero,1f);
			break;
			case BossActionType.CheckPlayerWorking:
			movement.CheckPlayerWorking();
			break;
			case BossActionType.AddAngryPoint:
			Level.WarningCount++;
			break;
			case BossActionType.SetNotAngry:
			anim.SetBool("angry",false);
			break;
		}
	}

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		foreach(var action in enter)
		{
			PlayAction(action, animator);
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		foreach(var action in update)
		{
			PlayAction(action, animator);
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		foreach(var action in exit)
		{
			PlayAction(action, animator);
		}
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
