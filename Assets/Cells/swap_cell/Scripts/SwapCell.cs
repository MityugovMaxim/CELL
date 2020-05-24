using System;
using UnityEngine;

public class SwapCell : GameCell
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
		if (!gameObject.activeInHierarchy || !Stage.ContainsCell(Position, GameLayerType.Color))
		{
			if (_Finished != null)
				_Finished();
			return;
		}
		
		Stage.RemoveCell(Position, GameLayerType.Color);
		Stage.AddCell(Position, m_ColorCell, GameLayerType.Color);
		
		m_SampleFinished = _Finished;
		
		Animator.SetTrigger(m_SampleParameterID);
	}

	void InvokeSampleFinished()
	{
		InvokeCallback(ref m_SampleFinished);
	}
}