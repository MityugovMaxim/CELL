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

	public static GameCell Instantiate(GameCell _Cell, Vector3 _Position, GameLayer _Layer)
	{
		if (_Cell == null)
		{
			Debug.LogError("[GameCellPool] Instantiate cell failed. Cell is null.");
			return null;
		}
		
		if (_Layer == null)
		{
			Debug.LogError("[GameCellPool] Instantiate cell failed. Layer is null.");
			return null;
		}
		
		GameCell cell = GetCell(_Cell);
		Transform cellTransform = cell.transform;
		cellTransform.position = _Position;
		cellTransform.rotation = Quaternion.identity;
		cellTransform.SetParent(_Layer.transform, true);
		cell.gameObject.SetLayer(_Layer.gameObject.layer);
		
		return cell;
	}

	public static void Destroy(GameCell _Cell)
	{
		if (_Cell == null)
			return;
		
		if (!m_Pool.ContainsKey(_Cell.ID) || m_Pool[_Cell.ID] == null)
			m_Pool[_Cell.ID] = new Queue<GameCell>();
		
		_Cell.transform.SetParent(m_Root);
		
		_Cell.gameObject.SetLayer(m_Root.gameObject.layer);
		
		m_Pool[_Cell.ID].Enqueue(_Cell);
	}

	static void CreateRoot()
	{
		if (!Application.isPlaying)
			return;
		
		GameObject rootObject = new GameObject("game_cell_pool");
		//rootObject.hideFlags = HideFlags.HideInHierarchy;
		rootObject.layer = LayerMask.NameToLayer("Hidden");
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