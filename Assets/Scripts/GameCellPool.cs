using System.Collections.Generic;
using UnityEngine;

public static class GameCellPool
{
	static readonly Dictionary<int, Queue<GameCell>> m_Pool = new Dictionary<int, Queue<GameCell>>();

	static Transform m_Root;

	static GameCellPool()
	{
		Application.lowMemory += Clear;
		Application.quitting  += Clear;
		
		CreateRoot();
	}

	public static GameCell Instantiate(GameCell _Cell, Vector3 _Position, Transform _Parent)
	{
		if (_Cell == null)
			return null;
		
		if (!Application.isPlaying)
			return GameObject.Instantiate(_Cell, _Position, Quaternion.identity, _Parent);
		
		GameCell cell = GetCell(_Cell);
		cell.transform.position = _Position;
		cell.transform.rotation = Quaternion.identity;
		cell.transform.SetParent(_Parent, true);
		
		if (_Parent != null)
			cell.gameObject.layer = _Parent.gameObject.layer;
		
		return cell;
	}

	public static void Destroy(GameCell _Cell)
	{
		if (_Cell == null)
			return;
		
		if (!Application.isPlaying)
		{
			GameObject.Destroy(_Cell.gameObject);
			return;
		}
		
		if (!m_Pool.ContainsKey(_Cell.ID) || m_Pool[_Cell.ID] == null)
			m_Pool[_Cell.ID] = new Queue<GameCell>();
		
		_Cell.transform.SetParent(m_Root);
		
		_Cell.gameObject.layer = m_Root.gameObject.layer;
		
		m_Pool[_Cell.ID].Enqueue(_Cell);
	}

	static void CreateRoot()
	{
		if (!Application.isPlaying)
			return;
		
		GameObject rootObject = new GameObject("game_cell_pool");
		rootObject.layer     = LayerMask.NameToLayer("Hidden");
		rootObject.hideFlags = HideFlags.HideInHierarchy;
		m_Root               = rootObject.transform;
	}

	static GameCell GetCell(GameCell _Cell)
	{
		if (_Cell == null)
			return null;
		
		if (m_Pool.ContainsKey(_Cell.ID) && m_Pool[_Cell.ID] != null)
		{
			while (m_Pool[_Cell.ID].Count > 0)
			{
				GameCell cell = m_Pool[_Cell.ID].Dequeue();
				
				if (cell != null)
					return cell;
			}
		}
		
		return CreateCell(_Cell);
	}

	static GameCell CreateCell(GameCell _Cell)
	{
		GameCell cell = GameCell.Instantiate(_Cell, m_Root);
		cell.name = _Cell.name;
		return cell;
	}

	static void Clear()
	{
		m_Pool.Clear();
		
		if (m_Root != null)
		{
			for (int i = 0; i < m_Root.childCount; i++)
			{
				GameObject cellObject = m_Root.GetChild(i).gameObject;
				
				GameObject.Destroy(cellObject);
			}
		}
	}
}