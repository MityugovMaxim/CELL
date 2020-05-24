using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameLayer : MonoBehaviour
{
	public int Count => m_Cells.Count;

	public Vector3Int[] Positions => m_Cells.Keys.ToArray();

	public GameLayerType Type => m_Type;

	GameStage Stage { get; set; }

	[SerializeField] GameLayerType m_Type   = default;
	[SerializeField] Tilemap       m_Ground = default;

	readonly Dictionary<Vector3Int, GameCell> m_Cells        = new Dictionary<Vector3Int, GameCell>();
	readonly HashSet<Vector3Int>              m_DefaultCells = new HashSet<Vector3Int>();
	readonly HashSet<Vector3Int>              m_PlayerCells  = new HashSet<Vector3Int>();

	void OnDrawGizmosSelected()
	{
		if (m_Ground == null)
			return;
		
		m_Ground.CompressBounds();
		
		Bounds bounds = GetBounds();
		
		float   depth  = m_Ground.transform.position.z;
		Vector3 anchor = bounds.center;
		Vector3 min    = bounds.min;
		Vector3 max    = bounds.max;
		
		Handles.color = new Color(0.34f, 0.61f, 0.84f);
		Gizmos.color  = new Color(0.34f, 0.61f, 0.84f);
		
		Handles.DrawWireDisc(anchor, Vector3.back, 10);
		
		Gizmos.DrawLine(
			new Vector3(min.x, min.y, depth),
			new Vector3(max.x, min.y, depth)
		);
		
		Gizmos.DrawLine(
			new Vector3(max.x, min.y, depth),
			new Vector3(max.x, max.y, depth)
		);
		
		Gizmos.DrawLine(
			new Vector3(max.x, max.y, depth),
			new Vector3(min.x, max.y, depth)
		);
		
		Gizmos.DrawLine(
			new Vector3(min.x, max.y, depth),
			new Vector3(min.x, min.y, depth)
		);
		
		Handles.color = Color.white;
		Gizmos.color  = Color.white;
	}

	public Bounds GetBounds()
	{
		if (m_Ground == null)
			return new Bounds(transform.position, Vector3.zero);
		
		m_Ground.CompressBounds();
		
		return m_Ground.transform.TransformBounds(m_Ground.localBounds);
	}

	public void Setup(GameStage _Stage)
	{
		Stage = _Stage;
		
		m_Cells.Clear();
		m_DefaultCells.Clear();
		m_PlayerCells.Clear();
		
		foreach (Transform child in transform)
		{
			GameCell cell = child.GetComponent<GameCell>();
			
			if (cell == null)
			{
				Debug.LogWarning("[GameLayer] Setup. Foreign object.", child);
				continue;
			}
			
			Vector3Int position = GetGridPosition(cell.transform.position);
			
			cell.Setup(Stage, Type, position);
			cell.Show();
			
			m_Cells.Add(position, cell);
			
			m_DefaultCells.Add(position);
		}
	}

	public void Restore(GameStage _Stage)
	{
		Stage = _Stage;
		
		foreach (Vector3Int position in Positions)
		{
			if (m_DefaultCells.Contains(position))
				continue;
			
			if (m_PlayerCells.Contains(position))
				continue;
			
			GameCell cell = GetCell(position);
			
			if (cell != null)
				cell.Hide(() => GameCellPool.Destroy(cell));
			
			m_Cells.Remove(position);
		}
		
		foreach (Vector3Int position in m_DefaultCells)
		{
			GameCell cell = GetCell(position);
			
			if (cell != null)
				cell.Restore();
		}
		
		foreach (Vector3Int position in m_PlayerCells)
		{
			GameCell cell = GetCell(position);
			
			if (cell != null)
				cell.Restore();
		}
	}

	public Vector3Int GetGridPosition(Vector3 _Position)
	{
		if (m_Ground == null)
		{
			Debug.LogErrorFormat("[GameLayer] Get cell position failed. Ground map not found.");
			return Vector3Int.zero;
		}
		return m_Ground.WorldToCell(_Position);
	}

	public Vector3 GetWorldPosition(Vector3Int _Position)
	{
		if (m_Ground == null)
		{
			Debug.LogErrorFormat("[GameStage] Get world position failed. Ground map not found.");
			return Vector3Int.zero;
		}
		return m_Ground.CellToWorld(_Position);
	}

	public bool ContainsGround(Vector3Int _Position)
	{
		return m_Ground != null && m_Ground.HasTile(_Position);
	}

	public bool AddCell(Vector3Int _Position, GameCell _Cell)
	{
		if (_Cell == null)
		{
			Debug.LogError("[GameLayer] Add cell failed. Cell is null.");
			return false;
		}
		
		if (ContainsCell(_Position))
		{
			Debug.LogErrorFormat("[GameLayer] Add cell failed. Cell at position '{0}' already exists.", _Position);
			return false;
		}
		
		GameCell cell = GameCellPool.Instantiate(_Cell, GetWorldPosition(_Position), this);
		
		cell.Setup(Stage, Type, _Position);
		cell.Show();
		
		m_Cells.Add(_Position, cell);
		
		return true;
	}

	public bool RemoveCell(Vector3Int _Position)
	{
		if (!ContainsCell(_Position))
		{
			Debug.LogErrorFormat("[GameLayer] Remove cell failed. Cell at position '{0}' doesn't exists.", _Position);
			return false;
		}
		
		GameCell cell = GetCell(_Position);
		if (cell == null)
		{
			Debug.LogErrorFormat(
				"[GameStage] Remove cell failed. Cell at layer '{0}' and position '{1}' is null",
				Type,
				_Position
			);
			return false;
		}
		
		cell.Hide(() => GameCellPool.Destroy(cell));
		
		m_Cells.Remove(_Position);
		
		return true;
	}

	public GameCell GetCell(Vector3Int _Position)
	{
		return ContainsCell(_Position) ? m_Cells[_Position] : null;
	}

	public bool ContainsCell(Vector3Int _Position)
	{
		return m_Cells.ContainsKey(_Position);
	}

	public IEnumerator Sample(List<Vector3Int> _Positions)
	{
		int count = _Positions.Count;
		
		void SampleFinished() => count--;
		foreach (Vector3Int position in _Positions)
		{
			GameCell cell = GetCell(position);
			
			if (cell != null)
				cell.Sample();
			
			SampleFinished();
		}
		
		if (count > 0)
			yield return new WaitWhile(() => count > 0);
	}
}