using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
	[SerializeField] Tilemap m_Ground     = default;
	[SerializeField] float   m_SampleRate = 0.15f;

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
			Vector3Int position = GetCellPosition(colorCell.transform.position);
			colorCell.transform.position = GetWorldPosition(position);
		}
		
		m_DefaultSpecialCells = m_SpecialMap.GetComponentsInChildren<GameCell>();
		foreach (GameCell specialCell in m_DefaultSpecialCells)
		{
			Vector3Int position = GetCellPosition(specialCell.transform.position);
			specialCell.transform.position = GetWorldPosition(position);
		}
		
		m_DefaultConditionCells = m_ConditionMap.GetComponentsInChildren<GameCell>();
		foreach (GameCell conditionCell in m_DefaultConditionCells)
		{
			Vector3Int position = GetCellPosition(conditionCell.transform.position);
			conditionCell.transform.position = GetWorldPosition(position);
		}
	}

	void Awake()
	{
		RestoreCells();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			Execute();
		if (Input.GetKeyDown(KeyCode.Backspace))
			RestoreCells();
	}

	public void CompleteCondition(Vector3Int _Position)
	{
		if (m_CompletedConditions == null)
			m_CompletedConditions = new HashSet<Vector3Int>();
		
		m_CompletedConditions.Add(_Position);
	}

	public void FailCondition(Vector3Int _Position)
	{
		if (m_FailedConditions == null)
			m_FailedConditions = new HashSet<Vector3Int>();
		
		m_FailedConditions.Add(_Position);
	}

	public void Execute()
	{
		RestoreCells();
		
		foreach (GameCell colorCell in m_DefaultColorCells)
		{
			if (colorCell == null)
				continue;
			
			Vector3Int position = GetCellPosition(colorCell.transform.position);
			
			colorCell.Setup(this, position);
			
			m_ColorCells[position] = colorCell;
			
			ExecuteCell(position);
		}
		
		foreach (GameCell specialCell in m_DefaultSpecialCells)
		{
			if (specialCell == null)
				continue;
			
			Vector3Int position = GetCellPosition(specialCell.transform.position);
			
			specialCell.Setup(this, position);
			
			m_SpecialCells[position] = specialCell;
		}
		
		foreach (GameCell conditionCell in m_DefaultConditionCells)
		{
			if (conditionCell == null)
				continue;
			
			Vector3Int position = GetCellPosition(conditionCell.transform.position);
			
			conditionCell.Setup(this, position);
			
			 m_ConditionCells[position] = conditionCell;
		}
		
		StartCoroutine(m_ExecuteRoutine = ExecuteRoutine());
	}

	public void RestoreCells()
	{
		if (m_ExecuteRoutine != null)
			StopCoroutine(m_ExecuteRoutine);
		m_ExecuteRoutine = null;
		
		if (m_CompletedConditions == null)
			m_CompletedConditions = new HashSet<Vector3Int>();
		else
			m_CompletedConditions.Clear();
		
		if (m_FailedConditions == null)
			m_FailedConditions = new HashSet<Vector3Int>();
		else
			m_FailedConditions.Clear();
		
		if (m_ColorCells == null)
			m_ColorCells = new Dictionary<Vector3Int, GameCell>();
		else
			m_ColorCells.Clear();
		
		if (m_SpecialCells == null)
			m_SpecialCells = new Dictionary<Vector3Int, GameCell>();
		else
			m_SpecialCells.Clear();
		
		if (m_ConditionCells == null)
			m_ConditionCells = new Dictionary<Vector3Int, GameCell>();
		else
			m_ConditionCells.Clear();
		
		if (m_ExecuteCells == null)
			m_ExecuteCells = new HashSet<Vector3Int>();
		else
			m_ExecuteCells.Clear();
		
		RestoreCells(m_ColorMap, m_DefaultColorCells);
		
		RestoreCells(m_SpecialMap, m_DefaultSpecialCells);
		
		RestoreCells(m_ConditionMap, m_DefaultConditionCells);
	}

	void RestoreCells(Transform _Layer, GameCell[] _DefaultCells)
	{
		HashSet<GameCell> defaultCells = new HashSet<GameCell>(_DefaultCells);
		for (int i = 0; i < _Layer.childCount; i++)
		{
			GameCell cell = _Layer.GetChild(i).GetComponent<GameCell>();
			
			if (cell == null)
				continue;
			
			if (defaultCells.Contains(cell))
				cell.Restore();
			else
				cell.Remove();
		}
	}

	public void ExecuteCell(Vector3Int _Position)
	{
		if (m_ExecuteCells == null)
			m_ExecuteCells = new HashSet<Vector3Int>();
		
		if (m_ExecuteCells.Contains(_Position))
			return;
		
		m_ExecuteCells.Add(_Position);
	}

	public bool ContainsGround(Vector3Int _Position)
	{
		return m_Ground != null && m_Ground.HasTile(_Position);
	}

	public Vector3 GetWorldPosition(Vector3Int _Position)
	{
		return m_Ground != null ? m_Ground.CellToWorld(_Position) : Vector3.zero;
	}

	public Vector3Int GetCellPosition(Vector3 _Position)
	{
		return m_Ground != null ? m_Ground.WorldToCell(_Position) : Vector3Int.zero;
	}

	public void AddColorCell(Vector3Int _Position, GameCell _ColorCell)
	{
		if (_ColorCell == null)
			return;
		
		if (m_ColorCells == null)
			m_ColorCells = new Dictionary<Vector3Int, GameCell>();
		
		if (m_ColorCells.ContainsKey(_Position))
			return;
		
		GameCell colorCell = Instantiate(
			_ColorCell,
			GetWorldPosition(_Position),
			Quaternion.identity,
			m_ColorMap
		);
		
		colorCell.Setup(this, _Position);
		colorCell.Show();
		
		m_ColorCells[_Position] = colorCell;
	}

	public GameCell GetColorCell(Vector3Int _Position)
	{
		if (m_ColorCells == null)
			return null;
		
		if (!m_ColorCells.ContainsKey(_Position))
			return null;
		
		return m_ColorCells[_Position];
	}

	public void RemoveColorCell(Vector3Int _Position)
	{ 
		GameCell colorCell = GetColorCell(_Position);
		
		if (colorCell == null)
			return;
		
		colorCell.Remove();
		
		m_ColorCells.Remove(_Position);
	}

	public void AddSpecialCell(Vector3Int _Position, GameCell _SpecialCell)
	{
		if (_SpecialCell == null)
			return;
		
		if (m_SpecialCells == null)
			m_SpecialCells = new Dictionary<Vector3Int, GameCell>();
		
		if (m_SpecialCells.ContainsKey(_Position))
			return;
		
		GameCell specialCell = Instantiate(
			_SpecialCell,
			GetWorldPosition(_Position),
			Quaternion.identity,
			m_SpecialMap
		);
		
		specialCell.Setup(this, _Position);
		
		m_SpecialCells[_Position] = specialCell;
	}

	public GameCell GetSpecialCell(Vector3Int _Position)
	{
		if (m_SpecialCells == null)
			return null;
		
		if (!m_SpecialCells.ContainsKey(_Position))
			return null;
		
		return m_SpecialCells[_Position];
	}

	public void RemoveSpecialCell(Vector3Int _Position)
	{
		GameCell specialCell = GetSpecialCell(_Position);
		
		if (specialCell == null)
			return;
		
		specialCell.Remove();
		
		m_SpecialCells.Remove(_Position);
	}

	public GameCell GetConditionCell(Vector3Int _Position)
	{
		if (m_ConditionCells == null)
			return null;
		
		if (!m_ConditionCells.ContainsKey(_Position))
			return null;
		
		return m_ConditionCells[_Position];
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
		while (m_ExecuteCells.Count > 0)
		{
			List<Vector3Int> positions = new List<Vector3Int>(m_ExecuteCells);
			
			m_ExecuteCells.Clear();
			
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

