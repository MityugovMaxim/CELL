using System;
using UnityEngine;

public class OriginCell : GameCell
{
	static readonly int m_SampleStateID     = Animator.StringToHash("Sample");
	static readonly int m_SampleParameterID = Animator.StringToHash("Sample");

	[SerializeField] ColorCell m_ColorCell = default;

	Action m_SampleFinished;

	public override void Setup(GameStage _Stage, GameLayerType _LayerType, Vector3Int _Position)
	{
		base.Setup(_Stage, _LayerType, _Position);
		
		StateBehaviour.AddStateBehaviour(Animator, m_SampleStateID);
		StateBehaviour.SetCompleteStateListener(Animator, m_SampleStateID, InvokeSampleFinished);
	}

	public override void Sample(Action _Finished = null)
	{
		int count = 0;
		foreach (Vector3Int position in HexUtility.GetNeighborPositions(Position))
		{
			if (!Stage.ContainsGround(position, GameLayerType.Color))
				continue;
			
			if (Stage.ContainsCell(position, GameLayerType.Color))
				continue;
			
			Stage.AddCell(position, m_ColorCell, GameLayerType.Color);
			
			Stage.ExecuteCell(position);
			
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
