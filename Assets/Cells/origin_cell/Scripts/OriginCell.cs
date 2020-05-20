using System;
using UnityEngine;

public class OriginCell : GameCell
{
	static readonly int m_SampleStateID     = Animator.StringToHash("Sample");
	static readonly int m_SampleParameterID = Animator.StringToHash("Sample");

	[SerializeField] ColorCell m_ColorCell = default;

	Action m_SampleFinished;

	public override void Setup(Level _Level, Vector3Int _Position)
	{
		base.Setup(_Level, _Position);
		
		StateBehaviour.AddStateBehaviour(Animator, m_SampleStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_SampleStateID, InvokeSampleFinished);
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

	void InvokeSampleFinished()
	{
		InvokeCallback(ref m_SampleFinished);
	}
}
