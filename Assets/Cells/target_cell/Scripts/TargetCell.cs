using System;
using System.Linq;
using UnityEngine;

public class TargetCell : GameCell
{
	static readonly int m_RestoreStateID      = Animator.StringToHash("Restore");
	static readonly int m_CompleteStateID     = Animator.StringToHash("Complete");
	static readonly int m_FailStateID         = Animator.StringToHash("Fail");
	static readonly int m_RestoreParameterID  = Animator.StringToHash("Restore");
	static readonly int m_CompleteParameterID = Animator.StringToHash("Complete");
	static readonly int m_FailParameterID     = Animator.StringToHash("Fail");

	[SerializeField] GameCell[] m_Cells = default;

	Action m_RestoreFinished;
	Action m_CompleteFinished;
	Action m_FailFinished;

	public override void Setup(GameStage _Stage, GameLayerType _LayerType, Vector3Int _Position)
	{
		base.Setup(_Stage, _LayerType, _Position);
		
		StateBehaviour.AddStateBehaviour(Animator, m_RestoreStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_RestoreStateID, InvokeRestoreFinished);
		
		StateBehaviour.AddStateBehaviour(Animator, m_CompleteStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_CompleteStateID, InvokeCompleteFinished);
		
		StateBehaviour.AddStateBehaviour(Animator, m_FailStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_FailStateID, InvokeFailFinished);
	}

	public override void Restore(Action _Finished = null)
	{
		if (!gameObject.activeInHierarchy)
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		m_RestoreFinished = _Finished;
		
		Animator.ResetTrigger(m_CompleteParameterID);
		Animator.ResetTrigger(m_FailParameterID);
		Animator.ResetTrigger(m_RestoreParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
	}

	public override void Sample(Action _Finished = null)
	{
		if (!Stage.ContainsCell(Position, GameLayerType.Color))
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		GameCell cell = Stage.GetCell(Position, GameLayerType.Color);
		
		bool complete = m_Cells.Contains(cell);
		
		if (complete)
			Stage.CompleteTask(Position, LayerType);
		else
			Stage.FailTask(Position, LayerType);
		
		if (!gameObject.activeInHierarchy)
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		if (complete)
		{
			m_CompleteFinished = _Finished;
			
			Animator.SetTrigger(m_CompleteParameterID);
		}
		else
		{
			m_FailFinished = _Finished;
			
			Animator.SetTrigger(m_FailParameterID);
		}
	}

	void InvokeRestoreFinished()
	{
		InvokeCallback(ref m_RestoreFinished);
	}

	void InvokeCompleteFinished()
	{
		InvokeCallback(ref m_CompleteFinished);
	}

	void InvokeFailFinished()
	{
		InvokeCallback(ref m_FailFinished);
	}
}