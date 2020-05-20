using System;
using UnityEngine;

public class CollectCell : GameCell
{
	static readonly int m_RestoreStateID     = Animator.StringToHash("Restore");
	static readonly int m_CollectStateID     = Animator.StringToHash("Collect");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_CollectParameterID = Animator.StringToHash("Collect");

	Action m_RestoreFinished;
	Action m_CollectFinished;

	public override void Setup(Level _Level, Vector3Int _Position)
	{
		base.Setup(_Level, _Position);
		
		StateBehaviour.AddStateBehaviour(Animator, m_RestoreStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_RestoreStateID, InvokeRestoreFinished);
		
		StateBehaviour.AddStateBehaviour(Animator, m_CollectStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_CollectStateID, InvokeCollectFinished);
	}

	public override void Restore(Action _Finished = null)
	{
		if (!gameObject.activeInHierarchy)
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		Animator.ResetTrigger(m_CollectParameterID);
		Animator.ResetTrigger(m_RestoreParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
	}

	public override void Sample(Action _Finished = null)
	{
		GameCell colorCell = Level.GetColorCell(Position);
		
		if (colorCell == null)
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		Level.CompleteCondition(Position);
		
		if (!gameObject.activeInHierarchy)
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		m_CollectFinished = _Finished;
		
		Animator.SetTrigger(m_CollectParameterID);
	}

	void InvokeCollectFinished()
	{
		InvokeCallback(ref m_CollectFinished);
	}

	void InvokeRestoreFinished()
	{
		InvokeCallback(ref m_RestoreFinished);
	}
}