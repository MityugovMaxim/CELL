using System.Collections.Generic;
using UnityEngine;

public class GameStageResult
{
	readonly struct Data
	{
		public readonly int Progress;
		public readonly int Target;

		public Data(int _Progress, int _Target)
		{
			Progress = _Progress;
			Target   = _Target;
		}
	}

	readonly Dictionary<GameLayerType, Data> m_Data = new Dictionary<GameLayerType, Data>();

	public void Clear()
	{
		m_Data.Clear();
	}

	public void Add(GameLayerType _LayerType, int _Progress, int _Target)
	{
		if (m_Data.ContainsKey(_LayerType))
		{
			Debug.LogErrorFormat("[GameStageResult] Add result failed. Result for layer '{0}' already exists.", _LayerType);
			return;
		}
		m_Data[_LayerType] = new Data(_Progress, _Target);
	}

	public int GetProgress(GameLayerType _LayerType, int _Default = 0)
	{
		return m_Data.ContainsKey(_LayerType) ? m_Data[_LayerType].Progress : _Default;
	}

	public int GetTarget(GameLayerType _LayerType, int _Default = 0)
	{
		return m_Data.ContainsKey(_LayerType) ? m_Data[_LayerType].Target : _Default;
	}
}