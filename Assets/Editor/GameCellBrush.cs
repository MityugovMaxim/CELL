using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Game Cell Brush", menuName = "Map/Brushes/Game Cell Brush")]
[CustomGridBrush(false, true, false, "Game Cell Brush")]
public class GameCellBrush : GridBrush
{
	[SerializeField] GameCell[] m_GameCells = default;

	[SerializeField, HideInInspector] int m_LayerIndex = default;
	[SerializeField, HideInInspector] int m_CellIndex  = default;

	static GameCell GetGameCell(GridLayout _Grid, Transform _Layer, Vector3Int _Position)
	{
		for (int i = 0; i < _Layer.childCount; i++)
		{
			GameCell cell = _Layer.GetChild(i).GetComponent<GameCell>();
			
			if (cell == null || cell.gameObject.CompareTag("EditorOnly"))
				continue;
			
			Vector3Int position = _Grid.WorldToCell(cell.transform.position);
			
			if (position == _Position)
				return cell;
		}
		return null;
	}

	static bool ContainsCell(GridLayout _Grid, Transform _Layer, Vector3Int _Position, GameCell _Cell)
	{
		for (int i = 0; i < _Layer.childCount; i++)
		{
			GameCell cell = _Layer.GetChild(i).GetComponent<GameCell>();
			
			if (cell == null || cell.gameObject.CompareTag("EditorOnly"))
				continue;
			
			Vector3Int position = _Grid.WorldToCell(cell.transform.position);
			
			if (position == _Position)
				return true;
		}
		return false;
	}

	public override void Paint(GridLayout _Grid, GameObject _Target, Vector3Int _Position)
	{
		if (m_GameCells == null || m_GameCells.Length == 0)
			return;
		
		Tilemap tilemap = _Target.GetComponent<Tilemap>();
		if (tilemap == null || !tilemap.HasTile(_Position))
			return;
		
		Level level = _Grid.GetComponent<Level>();
		
		if (level == null)
			return;
		
		m_CellIndex = Mathf.Clamp(m_CellIndex, 0, m_GameCells.Length - 1);
		
		GameCell cell = m_GameCells[m_CellIndex];
		
		Transform layer = GetLayer(_Grid);
		
		if (layer == null || ContainsCell(_Grid, layer, _Position, cell))
			return;
		
		GameCell instance = (GameCell)PrefabUtility.InstantiatePrefab(cell, layer);
		
		Undo.RegisterCreatedObjectUndo(instance.gameObject, "Paint Game Cells");
		
		instance.transform.position = _Grid.CellToWorld(_Position);
		instance.transform.rotation = Quaternion.identity;
		
		level.SetupDefaultCells();
	}

	public Transform GetLayer(GridLayout _Grid)
	{
		if (_Grid == null)
			return null;
		
		return _Grid.transform.GetChild(m_LayerIndex);
	}

	public override void Erase(GridLayout _Grid, GameObject _Target, Vector3Int _Position)
	{
		Level level = _Grid.GetComponent<Level>();
		
		if (level == null)
			return;
		
		Transform layer = GetLayer(_Grid);
		
		GameCell cell = GetGameCell(_Grid, layer, _Position);
		
		if (cell != null)
			Undo.DestroyObjectImmediate(cell.gameObject);
	}

	[CustomEditor(typeof(GameCellBrush))]
	public class GameCellBrushEditor : GridBrushEditor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			DrawLayers();
			
			DrawCells();
			
			serializedObject.ApplyModifiedProperties();
		}

		void DrawCells()
		{
			SerializedProperty cellsProperty     = serializedObject.FindProperty("m_GameCells");
			SerializedProperty cellIndexProperty = serializedObject.FindProperty("m_CellIndex");
			
			List<Texture2D> cells = new List<Texture2D>();
			
			for (int i = 0; i < cellsProperty.arraySize; i++)
			{
				SerializedProperty cellProperty = cellsProperty.GetArrayElementAtIndex(i);
				
				GameCell cell = cellProperty.objectReferenceValue as GameCell;
				
				if (cell == null)
					continue;
				
				cells.Add(AssetPreview.GetAssetPreview(cell.gameObject));
			}
			
			if (Event.current.type == EventType.DragPerform)
				return;
			
			const int lineCapacity = 8;
			int linesCount = Mathf.CeilToInt((float)cells.Count / lineCapacity);
			for (int i = 0; i < linesCount; i++)
			{
				Rect lineRect = GUILayoutUtility.GetAspectRect(lineCapacity / 1.0f);
				float width = lineRect.width / lineCapacity;
				for (int j = 0; j < lineCapacity && j < cells.Count - i * lineCapacity; j++)
				{
					int index = i * lineCapacity + j;
					
					Rect cellRect = new Rect(
						lineRect.x + width * j,
						lineRect.y,
						width,
						lineRect.height
					);
					
					cellRect = cellRect.Fit(1, new Vector2(0.5f, 0.5f));
					
					if (cells[index] == null)
						continue;
					
					EditorGUI.DrawTextureTransparent(cellRect, cells[index], ScaleMode.ScaleToFit);
					
					if (cellIndexProperty.intValue == index)
					{
						Handles.DrawSolidRectangleWithOutline(
							new RectOffset(2, 2, 2, 2).Remove(cellRect),
							Color.clear,
							new Color(0.34f, 0.61f, 0.84f)
						);
					}
					
					if (GUI.Button(cellRect, GUIContent.none, GUIStyle.none))
						cellIndexProperty.intValue = index;
				}
			}
		}

		void DrawLayers()
		{
			SerializedProperty layerIndexProperty = serializedObject.FindProperty("m_LayerIndex");
			
			List<string> layers = new List<string>();
			foreach (GameObject gameObject in validTargets)
			{
				Tilemap tilemap = gameObject.GetComponent<Tilemap>();
				if (tilemap == null)
					continue;
				
				GridLayout grid = tilemap.layoutGrid;
				if (grid == null)
					continue;
				
				Level level = grid.GetComponent<Level>();
				if (level == null)
					continue;
				
				foreach(Transform layer in level.transform)
					layers.Add(layer.name);
			}
			
			layerIndexProperty.intValue = EditorGUILayout.Popup("Layer", layerIndexProperty.intValue, layers.ToArray());
		}

		GameCell m_Preview;

		public override void ClearPreview()
		{
			if (m_Preview != null)
				DestroyImmediate(m_Preview.gameObject);
			m_Preview = null;
		}

		public override void PaintPreview(GridLayout _Grid, GameObject _Target, Vector3Int _Position)
		{
			if (m_Preview != null)
				return;
			
			SerializedProperty cellsProperty    = serializedObject.FindProperty("m_GameCells");
			SerializedProperty layerProperty    = serializedObject.FindProperty("m_LayerIndex");
			SerializedProperty selectedProperty = serializedObject.FindProperty("m_CellIndex");
			
			Tilemap tilemap = _Grid.GetComponent<Tilemap>();
			if (tilemap == null || !tilemap.HasTile(_Position))
				return;
			
			GridLayout grid = tilemap.layoutGrid;
			if (grid == null)
				return;
			
			Level level = grid.GetComponent<Level>();
			if (level == null)
				return;
			
			Transform layer = level.transform.GetChild(layerProperty.intValue);
			if (layer == null)
				return;
			
			SerializedProperty cellProperty = cellsProperty.GetArrayElementAtIndex(selectedProperty.intValue);
			
			GameCell cell = cellProperty.objectReferenceValue as GameCell;
			
			if (cell == null)
				return;
			
			m_Preview = Instantiate(
				cell,
				_Grid.CellToWorld(_Position),
				Quaternion.identity,
				layer
			);
			m_Preview.tag = "EditorOnly";
			m_Preview.gameObject.hideFlags = HideFlags.HideAndDontSave;
		}
	}
}