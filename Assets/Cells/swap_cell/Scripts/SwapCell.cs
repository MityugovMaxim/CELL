using System;
using UnityEngine;

public class SwapCell : GameCell
{
	static readonly int m_SampleStateID     = Animator.StringToHash("Sample");
	static readonly int m_SampleParameterID = Animator.StringToHash("Sample");

	[SerializeField] ColorCell m_ColorCell = default;

	Action m_SampleFinished;

	public override void Setup(Level _Level)
	{
		base.Setup(_Level);
		
		StateBehaviour.AddStateBehaviour(Animator, m_SampleStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_SampleStateID, InvokeSampleFinished);
	}

	public override void Sample(Action _Finished = null)
	{
		GameCell colorCell = Level.GetColorCell(Position);
		
		if (!gameObject.activeInHierarchy || colorCell == null)
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		Level.RemoveColorCell(Position);
		Level.AddColorCell(Position, m_ColorCell);
		
		m_SampleFinished = _Finished;
		
		Animator.SetTrigger(m_SampleParameterID);
	}

	void InvokeSampleFinished()
	{
		InvokeCallback(ref m_SampleFinished);
	}
}