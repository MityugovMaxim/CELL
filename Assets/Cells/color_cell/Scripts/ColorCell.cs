using System;
using UnityEngine;

public class ColorCell : GameCell
{
	static readonly int m_ShowStateID        = Animator.StringToHash("Show");
	static readonly int m_HideStateID        = Animator.StringToHash("Hide");
	static readonly int m_RestoreStateID     = Animator.StringToHash("Sample");
	static readonly int m_SampleStateID      = Animator.StringToHash("Restore");
	static readonly int m_ShowParameterID    = Animator.StringToHash("Show");
	static readonly int m_RestoreParameterID = Animator.StringToHash("Restore");
	static readonly int m_SampleParameterID  = Animator.StringToHash("Sample");

	[SerializeField] ColorCell m_ColorCell = default;

	Action m_ShowFinished;
	Action m_HideFinished;
	Action m_RestoreFinished;
	Action m_SampleFinished;

	public override void Setup(Level _Level, Vector3Int _Position)
	{
		base.Setup(_Level, _Position);
		
		StateBehaviour.AddStateBehaviour(Animator, m_ShowStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_ShowStateID, InvokeShowFinished);
		
		StateBehaviour.AddStateBehaviour(Animator, m_HideStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_HideStateID, InvokeHideFinished);
		
		StateBehaviour.AddStateBehaviour(Animator, m_RestoreStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_RestoreStateID, InvokeRestoreFinished);
		
		StateBehaviour.AddStateBehaviour(Animator, m_SampleStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_SampleStateID, InvokeSampleFinished);
	}

	public override void Remove()
	{
		Hide(base.Remove);
	}

	public override void Show(Action _Finished = null)
	{
		if (!gameObject.activeInHierarchy)
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		m_ShowFinished = _Finished;
		
		Animator.SetBool(m_ShowParameterID, true);
	}

	public override void Hide(Action _Finished = null)
	{
		if (!gameObject.activeInHierarchy)
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		m_HideFinished = _Finished;
		
		Animator.SetBool(m_ShowParameterID, false);
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
		
		Animator.SetBool(m_ShowParameterID, false);
		Animator.ResetTrigger(m_SampleParameterID);
		Animator.ResetTrigger(m_RestoreParameterID);
		Animator.SetTrigger(m_RestoreParameterID);
	}

	public override void Sample(Action _Finished = null)
	{
		int count = 0;
		foreach (Vector3Int position in HexUtility.GetNeighborPositions(Position))
		{
			if (!Level.ContainsGround(position))
				continue;
			
			GameCell colorCell = Level.GetColorCell(position);
			
			if (colorCell != null)
				continue;
			
			Level.AddColorCell(position, m_ColorCell);
			Level.ExecuteCell(position);
			
			count++;
		}
		
		if (!gameObject.activeInHierarchy || count == 0)
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		m_SampleFinished = _Finished;
		
		Animator.SetTrigger(m_SampleParameterID);
	}

	void InvokeShowFinished()
	{
		InvokeCallback(ref m_ShowFinished);
	}

	void InvokeHideFinished()
	{
		InvokeCallback(ref m_HideFinished);
	}

	void InvokeRestoreFinished()
	{
		InvokeCallback(ref m_RestoreFinished);
	}

	void InvokeSampleFinished()
	{
		InvokeCallback();
	}
}
