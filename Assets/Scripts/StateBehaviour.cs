using System;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

public class StateBehaviour : StateMachineBehaviour
{
	[SerializeField, HideInInspector] int m_Hash;

	Action m_EnterState;
	Action m_UpdateState;
	Action m_CompleteState;
	Action m_ExitState;

	bool m_Complete;

	public static void AddStateBehaviour(Animator _Animator, int _StateID)
	{
		if (_Animator == null)
		{
			Debug.LogError("[StateBehaviour] Add state behaviour failed. Animator not found.");
			return;
		}
		
		AnimatorController controller = _Animator.runtimeAnimatorController as AnimatorController;
		
		if (controller == null)
		{
			Debug.LogError("[StateBehaviour] Add state behaviour failed. Animator controller not found.");
			return;
		}
		
		bool rebind = false;
		foreach (AnimatorControllerLayer layer in controller.layers)
		{
			if (layer.stateMachine == null)
				continue;
			
			foreach (AnimatorState state in layer.stateMachine.states.Select(_State => _State.state))
			{
				if (state.behaviours != null && state.behaviours.OfType<StateBehaviour>().Any(_Behaviour => _Behaviour.m_Hash == _StateID))
					continue;
				
				if (state.nameHash == _StateID)
				{
					rebind = true;
					StateBehaviour behaviour = state.AddStateMachineBehaviour<StateBehaviour>();
					behaviour.m_Hash = state.nameHash;
				}
			}
		}
		
		if (rebind)
			_Animator.Rebind();
	}

	public static void SetEnterStateListener(Animator _Animator, int _StateID, Action _Listener)
	{
		StateBehaviour[] behaviours = _Animator.GetBehaviours<StateBehaviour>();
		foreach (StateBehaviour behaviour in behaviours)
		{
			if (behaviour.m_Hash == _StateID)
				behaviour.m_EnterState = _Listener;
		}
	}

	public static void SetUpdateStateListener(Animator _Animator, int _StateID, Action _Listener)
	{
		StateBehaviour[] behaviours = _Animator.GetBehaviours<StateBehaviour>();
		foreach (StateBehaviour behaviour in behaviours)
		{
			if (behaviour.m_Hash == _StateID)
				behaviour.m_UpdateState = _Listener;
		}
	}

	public static void SetCompleteStateListener(Animator _Animator, int _StateID, Action _Listener)
	{
		StateBehaviour[] behaviours = _Animator.GetBehaviours<StateBehaviour>();
		foreach (StateBehaviour behaviour in behaviours)
		{
			if (behaviour.m_Hash == _StateID)
				behaviour.m_CompleteState = _Listener;
		}
	}

	public static void SetExitStateListener(Animator _Animator, int _StateID, Action _Listener)
	{
		StateBehaviour[] behaviours = _Animator.GetBehaviours<StateBehaviour>();
		foreach (StateBehaviour behaviour in behaviours)
		{
			if (behaviour.m_Hash == _StateID)
				behaviour.m_ExitState = _Listener;
		}
	}

	public override void OnStateEnter(Animator _Animator, AnimatorStateInfo _StateInfo, int _LayerIndex)
	{
		base.OnStateEnter(_Animator, _StateInfo, _LayerIndex);
		
		m_Complete = false;
		
		InvokeEnterState();
	}

	public override void OnStateUpdate(Animator _Animator, AnimatorStateInfo _StateInfo, int _LayerIndex)
	{
		base.OnStateUpdate(_Animator, _StateInfo, _LayerIndex);
		
		InvokeUpdateState();
		
		if (_StateInfo.normalizedTime >= 1)
			InvokeCompleteState();
	}

	public override void OnStateExit(Animator _Animator, AnimatorStateInfo _StateInfo, int _LayerIndex)
	{
		base.OnStateExit(_Animator, _StateInfo, _LayerIndex);
		
		InvokeCompleteState();
		
		InvokeExitState();
		
		m_Complete = false;
	}

	void InvokeEnterState()
	{
		if (m_EnterState != null)
			m_EnterState();
	}

	void InvokeUpdateState()
	{
		if (m_UpdateState != null)
			m_UpdateState();
	}

	void InvokeCompleteState()
	{
		if (m_Complete)
			return;
		
		m_Complete = true;
		
		if (m_CompleteState != null)
			m_CompleteState();
	}

	void InvokeExitState()
	{
		if (m_ExitState != null)
			m_ExitState();
	}
}