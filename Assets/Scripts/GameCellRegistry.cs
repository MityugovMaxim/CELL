using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameCellRegistry : IEnumerable<int>
{
	[SerializeField] int[] m_GameCellIDs = default;

	public bool Contains(int _GameCellID)
	{
		return m_GameCellIDs.Contains(_GameCellID);
	}

	public bool Contains(GameCell _GameCell)
	{
		return _GameCell != null && Contains(_GameCell.ID);
	}

	public IEnumerator<int> GetEnumerator()
	{
		return m_GameCellIDs.AsEnumerable().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_GameCellIDs.GetEnumerator();
	}
}