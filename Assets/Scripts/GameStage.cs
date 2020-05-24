using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStage : MonoBehaviour
{
	static readonly GameLayerType[] m_ExecutionOrder =
	{
		GameLayerType.Special,
		GameLayerType.Collect,
		GameLayerType.Condition,
		GameLayerType.Color,
	};

	HashSet<Vector3Int> ExecuteCells => m_ExecuteCells ?? (m_ExecuteCells = new HashSet<Vector3Int>());

	Dictionary<GameLayerType, GameLayer> LayersCache => m_LayersCache ?? (m_LayersCache = new Dictionary<GameLayerType, GameLayer>());

	[SerializeField] float m_SampleRate = 0.15f;

	[SerializeField, HideInInspector] GameLayer[] m_Layers = default;

	Dictionary<GameLayerType, GameLayer> m_LayersCache;

	HashSet<Vector3Int> m_ExecuteCells;
	IEnumerator         m_ExecuteRoutine;

	void Awake()
	{
		Setup();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			Execute();
		if (Input.GetKeyDown(KeyCode.Backspace))
			Restore();
	}

	public Bounds GetBounds()
	{
		Bounds bounds = new Bounds(transform.position, Vector3.zero);
		foreach (GameLayer layer in m_Layers)
		{
			if (layer != null)
				bounds.Encapsulate(layer.GetBounds());
		}
		return bounds;
	}

	public void Setup()
	{
		ExecuteCells.Clear();
		
		foreach (GameLayer layer in m_Layers)
		{
			if (layer == null)
				continue;
			
			layer.Setup(this);
			
			foreach (Vector3Int position in layer.Positions)
				ExecuteCells.Add(position);
		}
	}

	public void Execute(Action<GameStageResult> _Finished = null)
	{
		if (m_ExecuteRoutine != null)
			StopCoroutine(m_ExecuteRoutine);
		m_ExecuteRoutine = null;
		
		StartCoroutine(m_ExecuteRoutine = ExecuteRoutine(_Finished));
	}

	public void Restore()
	{
		if (m_ExecuteRoutine != null)
			StopCoroutine(m_ExecuteRoutine);
		m_ExecuteRoutine = null;
		
		ExecuteCells.Clear();
		
		foreach (GameLayer layer in m_Layers)
		{
			if (layer == null)
				continue;
			
			layer.Restore(this);
			
			foreach (Vector3Int position in layer.Positions)
				ExecuteCells.Add(position);
		}
	}

	public void CompleteTask(Vector3Int _Position, GameLayerType _LayerType)
	{
		
	}

	public void FailTask(Vector3Int _Position, GameLayerType _LayerType)
	{
		
	}

	public void ExecuteCell(Vector3Int _Position)
	{
		if (!ExecuteCells.Contains(_Position))
			ExecuteCells.Add(_Position);
	}

	public bool ContainsGround(Vector3Int _Position, GameLayerType _LayerType)
	{
		GameLayer layer = GetLayer(_LayerType);
		
		return layer != null && layer.ContainsGround(_Position);
	}

	public Vector3Int GetGridPosition(Vector3 _Position, GameLayerType _LayerType)
	{
		GameLayer layer = GetLayer(_LayerType);
		
		if (layer == null)
		{
			Debug.LogErrorFormat("[GameStage] Get grid position failed. Layer '{0}' is null.", _LayerType);
			return Vector3Int.zero;
		}
		
		return layer.GetGridPosition(_Position);
	}

	public Vector3 GetWorldPosition(Vector3Int _Position, GameLayerType _LayerType)
	{
		GameLayer layer = GetLayer(_LayerType);
		
		if (layer == null)
		{
			Debug.LogErrorFormat("[GameStage] Get world position failed. Layer '{0}' is null", _LayerType);
			return Vector3.zero;
		}
		
		return layer.GetWorldPosition(_Position);
	}

	public bool ContainsLayer(GameLayerType _LayerType)
	{
		return m_Layers != null && m_Layers.Any(_Layer => _Layer.Type == _LayerType);
	}

	public GameLayer GetLayer(GameLayerType _LayerType)
	{
		if (LayersCache.ContainsKey(_LayerType) && LayersCache[_LayerType] != null)
			return LayersCache[_LayerType];
		
		if (!ContainsLayer(_LayerType))
			return null;
		
		GameLayer layer = m_Layers.FirstOrDefault(_Layer => _Layer.Type == _LayerType);
		
		if (layer == null)
		{
			Debug.LogErrorFormat("[GameStage] Get layer failed. Layer '{0}' is null.", _LayerType);
			return null;
		}
		
		LayersCache[_LayerType] = layer;
		
		return layer;
	}

	public bool ContainsCell(Vector3Int _Position, GameLayerType _LayerType)
	{
		GameLayer layer = GetLayer(_LayerType);
		
		return layer != null && layer.ContainsCell(_Position);
	}

	public bool AddCell(Vector3Int _Position, GameCell _Cell, GameLayerType _LayerType)
	{
		GameLayer layer = GetLayer(_LayerType);
		
		if (layer == null)
		{
			Debug.LogErrorFormat("[GameStage] Add cell failed. Layer '{0}' not found.", _LayerType);
			return false;
		}
		
		layer.AddCell(_Position, _Cell);
		
		return true;
	}

	public bool RemoveCell(Vector3Int _Position, GameLayerType _LayerType)
	{
		GameLayer layer = GetLayer(_LayerType);
		
		if (layer == null)
		{
			Debug.LogErrorFormat("[GameStage] Remove cell failed. Layer '{0}' is null.", _LayerType);
			return false;
		}
		
		layer.RemoveCell(_Position);
		
		return true;
	}

	public GameCell GetCell(Vector3Int _Position, GameLayerType _LayerType)
	{
		if (!ContainsCell(_Position, _LayerType))
		{
			Debug.LogErrorFormat("[GameStage] Get cell failed. Cell at layer '{0}' and position '{1}' doesn't exists.", _LayerType, _Position);
			return null;
		}
		
		GameLayer layer = GetLayer(_LayerType);
		
		return layer.GetCell(_Position);
	}

	Dictionary<GameLayerType, int> m_Result = new Dictionary<GameLayerType, int>();

	IEnumerator ExecuteRoutine(Action<GameStageResult> _Finished = null)
	{
		if (ExecuteCells.Count == 0)
			yield break;
		
		while (ExecuteCells.Count > 0)
		{
			List<Vector3Int> positions = new List<Vector3Int>(ExecuteCells);
			
			ExecuteCells.Clear();
			
			foreach (GameLayerType layerType in m_ExecutionOrder)
			{
				GameLayer layer = GetLayer(layerType);
				
				if (layer != null)
					yield return layer.Sample(positions);
			}
			
			yield return new WaitForSeconds(m_SampleRate);
		}
		
		GameStageResult result = new GameStageResult();
		foreach (GameLayer layer in m_Layers)
		{
			result.Add(
				layer.Type,
				m_Result.ContainsKey(layer.Type) ? m_Result[layer.Type] : 0,
				layer.Count
			);
		}
		
		if (_Finished != null)
			_Finished(result);
	}
}
