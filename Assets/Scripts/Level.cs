using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
	Dictionary<Vector3Int, GameCell> ColorCells => m_ColorCells ?? (m_ColorCells = new Dictionary<Vector3Int, GameCell>());

	Dictionary<Vector3Int, GameCell> SpecialCells => m_SpecialCells ?? (m_SpecialCells = new Dictionary<Vector3Int, GameCell>());

	Dictionary<Vector3Int, GameCell> ConditionCells => m_ConditionCells ?? (m_ConditionCells = new Dictionary<Vector3Int, GameCell>());

	HashSet<Vector3Int> ExecuteCells => m_ExecuteCells ?? (m_ExecuteCells = new HashSet<Vector3Int>());

	HashSet<Vector3Int> CompletedConditions => m_CompletedConditions ?? (m_CompletedConditions = new HashSet<Vector3Int>());

	HashSet<Vector3Int> FailedConditions => m_FailedConditions ?? (m_FailedConditions = new HashSet<Vector3Int>());

	[SerializeField] float     m_SampleRate   = 0.15f;
	[SerializeField] Tilemap   m_GroundMap    = default;
	[SerializeField] Transform m_ColorMap     = default;
	[SerializeField] Transform m_SpecialMap   = default;
	[SerializeField] Transform m_ConditionMap = default;

	[SerializeField] GameCell[] m_DefaultColorCells     = default;
	[SerializeField] GameCell[] m_DefaultSpecialCells   = default;
	[SerializeField] GameCell[] m_DefaultConditionCells = default;

	Dictionary<Vector3Int, GameCell> m_ColorCells;
	Dictionary<Vector3Int, GameCell> m_SpecialCells;
	Dictionary<Vector3Int, GameCell> m_ConditionCells;

	HashSet<Vector3Int> m_ExecuteCells;
	IEnumerator         m_ExecuteRoutine;

	HashSet<Vector3Int> m_CompletedConditions;
	HashSet<Vector3Int> m_FailedConditions;

	[ContextMenu("Setup default cells")]
	public void SetupDefaultCells()
	{
		m_DefaultColorCells = m_ColorMap.GetComponentsInChildren<GameCell>();
		foreach (GameCell colorCell in m_DefaultColorCells)
		{
			Vector3Int position = GetGridPosition(colorCell);
			colorCell.transform.position = GetWorldPosition(position);
		}
		
		m_DefaultSpecialCells = m_SpecialMap.GetComponentsInChildren<GameCell>();
		foreach (GameCell specialCell in m_DefaultSpecialCells)
		{
			Vector3Int position = GetGridPosition(specialCell);
			specialCell.transform.position = GetWorldPosition(position);
		}
		
		m_DefaultConditionCells = m_ConditionMap.GetComponentsInChildren<GameCell>();
		foreach (GameCell conditionCell in m_DefaultConditionCells)
		{
			Vector3Int position = GetGridPosition(conditionCell);
			conditionCell.transform.position = GetWorldPosition(position);
		}
	}

	void Awake()
	{
		Restore();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			Execute();
		if (Input.GetKeyDown(KeyCode.Backspace))
			Restart();
	}

	public void CompleteCondition(Vector3Int _Position)
	{
		CompletedConditions.Add(_Position);
	}

	public void FailCondition(Vector3Int _Position)
	{
		FailedConditions.Add(_Position);
	}

	public void Execute()
	{
		if (m_ExecuteRoutine != null)
			StopCoroutine(m_ExecuteRoutine);
		m_ExecuteRoutine = null;
		
		StartCoroutine(m_ExecuteRoutine = ExecuteRoutine());
	}

	public void Restart()
	{
		if (m_ExecuteRoutine != null)
			StopCoroutine(m_ExecuteRoutine);
		m_ExecuteRoutine = null;
		
		Restore();
	}

	public void ExecuteCell(Vector3Int _Position)
	{
		if (!ExecuteCells.Contains(_Position))
			ExecuteCells.Add(_Position);
	}

	public void ExecuteCell(GameCell _Cell)
	{
		if (_Cell != null)
			ExecuteCell(_Cell.Position);
	}

	public void ExecuteCell(params GameCell[] _Cells)
	{
		foreach (GameCell cell in _Cells)
			ExecuteCell(cell);
	}

	public bool ContainsGround(Vector3Int _Position)
	{
		return m_GroundMap != null && m_GroundMap.HasTile(_Position);
	}

	public Vector3 GetWorldPosition(Vector3Int _Position)
	{
		return m_GroundMap != null ? m_GroundMap.CellToWorld(_Position) : Vector3.zero;
	}

	public Vector3 GetWorldPosition(GameCell _Cell)
	{
		return _Cell != null ? GetWorldPosition(_Cell.Position) : Vector3.zero;
	}

	public Vector3Int GetGridPosition(Vector3 _Position)
	{
		return m_GroundMap != null ? m_GroundMap.WorldToCell(_Position) : Vector3Int.zero;
	}

	public Vector3Int GetGridPosition(GameCell _Cell)
	{
		return _Cell != null ? GetGridPosition(_Cell.transform.position) : Vector3Int.zero;
	}

	public void AddColorCell(Vector3Int _Position, GameCell _Cell)
	{
		if (_Cell == null || ColorCells.ContainsKey(_Position))
			return;
		
		GameCell cell = GameCellPool.Instantiate(
			_Cell,
			GetWorldPosition(_Position),
			m_ColorMap
		);
		
		cell.Setup(this);
		cell.Show();
		
		ColorCells[_Position] = cell;
	}

	public GameCell GetColorCell(Vector3Int _Position)
	{
		return ColorCells.ContainsKey(_Position) ? ColorCells[_Position] : null;
	}

	public void RemoveColorCell(Vector3Int _Position)
	{ 
		GameCell cell = GetColorCell(_Position);
		
		if (cell != null)
			cell.Hide(() => GameCellPool.Destroy(cell));
		
		ColorCells.Remove(_Position);
	}

	public void AddSpecialCell(Vector3Int _Position, GameCell _Cell)
	{
		if (_Cell == null || SpecialCells.ContainsKey(_Position))
			return;
		
		GameCell cell = GameCellPool.Instantiate(
			_Cell,
			GetWorldPosition(_Position),
			m_SpecialMap
		);
		
		cell.Setup(this);
		cell.Show();
		
		SpecialCells[_Position] = cell;
	}

	public GameCell GetSpecialCell(Vector3Int _Position)
	{
		return SpecialCells.ContainsKey(_Position) ? SpecialCells[_Position] : null;
	}

	public void RemoveSpecialCell(Vector3Int _Position)
	{
		GameCell cell = GetSpecialCell(_Position);
		
		if (cell != null)
			cell.Hide(() => GameCellPool.Destroy(cell));
		
		SpecialCells.Remove(_Position);
	}

	public void AddConditionCell(Vector3Int _Position, GameCell _Cell)
	{
		if (_Cell == null || ConditionCells.ContainsKey(_Position))
			return;
		
		GameCell cell = GameCellPool.Instantiate(
			_Cell,
			GetWorldPosition(_Position),
			m_ConditionMap
		);
		
		cell.Setup(this);
		cell.Show();
		
		ConditionCells[_Position] = cell;
	}

	public GameCell GetConditionCell(Vector3Int _Position)
	{
		return ConditionCells.ContainsKey(_Position) ? ConditionCells[_Position] : null;
	}

	public void RemoveConditionCell(Vector3Int _Position)
	{
		GameCell cell = GetConditionCell(_Position);
		
		if (cell != null)
			cell.Hide(() => GameCellPool.Destroy(cell));
		
		ConditionCells.Remove(_Position);
	}

	void Restore()
	{
		ColorCells.Clear();
		SpecialCells.Clear();
		ConditionCells.Clear();
		CompletedConditions.Clear();
		FailedConditions.Clear();
		ExecuteCells.Clear();
		
		RestoreCells(m_ColorMap, m_DefaultColorCells, ColorCells);
		
		RestoreCells(m_SpecialMap, m_DefaultSpecialCells, SpecialCells);
		
		RestoreCells(m_ConditionMap, m_DefaultConditionCells, ConditionCells);
		
		ExecuteCell(m_DefaultColorCells);
	}

	void RestoreCells(Transform _Layer, IEnumerable<GameCell> _Cells, Dictionary<Vector3Int, GameCell> _Registry)
	{
		HashSet<GameCell> cells = new HashSet<GameCell>(_Cells);
		for (int i = 0; i < _Layer.childCount; i++)
		{
			GameCell cell = _Layer.GetChild(i).GetComponent<GameCell>();
			
			if (cell == null)
				continue;
			
			if (cells.Contains(cell))
			{
				cell.Restore();
				cell.Setup(this);
				
				_Registry[cell.Position] = cell;
			}
			else
			{
				cell.Hide(() => GameCellPool.Destroy(cell));
			}
		}
	}

	void SampleColor(Vector3Int _Position, Action _Finished = null)
	{
		GameCell colorCell = GetColorCell(_Position);
		
		if (colorCell != null)
			colorCell.Sample();
		
		_Finished?.Invoke();
	}

	void SampleSpecial(Vector3Int _Position, Action _Finished = null)
	{
		GameCell specialCell = GetSpecialCell(_Position);
		
		if (specialCell != null)
			specialCell.Sample();
		
		_Finished?.Invoke();
	}

	void SampleCondition(Vector3Int _Position, Action _Finished = null)
	{
		GameCell conditionCell = GetConditionCell(_Position);
		
		if (conditionCell != null)
			conditionCell.Sample();
		
		_Finished?.Invoke();
	}

	IEnumerator ExecuteRoutine()
	{
		Restore();
		
		yield return new WaitForSeconds(0.2f);
		
		while (ExecuteCells.Count > 0)
		{
			List<Vector3Int> positions = new List<Vector3Int>(ExecuteCells);
			
			ExecuteCells.Clear();
			
			// Process special cells
			int specialCount = positions.Count;
			void SpecialFinished() => specialCount--;
			foreach (Vector3Int position in positions)
				SampleSpecial(position, SpecialFinished);
			
			if (specialCount > 0)
				 yield return new WaitWhile(() => specialCount > 0);
			
			// Process color cells
			int colorCount = positions.Count;
			void ColorFinished() => colorCount--;
			foreach (Vector3Int position in positions)
				SampleColor(position, ColorFinished);
			
			if (colorCount > 0)
				yield return new WaitWhile(() => colorCount > 0);
			
			// Process condition cells
			int conditionCount = positions.Count;
			void ConditionFinished() => conditionCount--;
			foreach (Vector3Int position in positions)
				SampleCondition(position, ConditionFinished);
			
			if (conditionCount > 0)
				yield return new WaitWhile(() => conditionCount > 0);
			
			yield return new WaitForSeconds(m_SampleRate);
		}
	}
}

