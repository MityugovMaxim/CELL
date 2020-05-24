using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Game Cell Brush", menuName = "Map/Brushes/Game Cell Brush")]
[CustomGridBrush(false, true, false, "Game Cell Brush")]
public class GameCellBrush : GridBrush
{
	[SerializeField] GameCell[]    m_GameCells = default;
	[SerializeField] GameLayerType m_LayerType = default;

	[SerializeField, HideInInspector] int m_CellIndex = default;

	GameCell GetGameCell(GridLayout _Grid, Vector3Int _Position)
	{
		GameLayer layer = GetLayer(_Grid);
		
		if (layer == null)
			return null;
		
		Transform layerTransform = layer.transform;
		for (int i = 0; i < layerTransform.childCount; i++)
		{
			GameCell cell = layerTransform.GetChild(i).GetComponent<GameCell>();
			
			if (cell == null || cell.gameObject.CompareTag("EditorOnly"))
				continue;
			
			Vector3Int position = _Grid.WorldToCell(cell.transform.position);
			
			if (position == _Position)
				return cell;
		}
		return null;
	}

	bool ContainsGround(GridLayout _Grid, Vector3Int _Position)
	{
		GameLayer layer = GetLayer(_Grid);
		
		return layer != null && layer.ContainsGround(_Position);
	}

	bool ContainsCell(GridLayout _Grid, Vector3Int _Position)
	{
		GameLayer layer = GetLayer(_Grid);
		
		if (layer == null)
			return false;
		
		Transform layerTransform = layer.transform;
		for (int i = 0; i < layerTransform.childCount; i++)
		{
			GameCell cell = layerTransform.GetChild(i).GetComponent<GameCell>();
			
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
		
		if (!ContainsGround(_Grid, _Position))
			return;
		
		m_CellIndex = Mathf.Clamp(m_CellIndex, 0, m_GameCells.Length - 1);
		
		GameCell cell = m_GameCells[m_CellIndex];
		
		if (ContainsCell(_Grid, _Position))
			return;
		
		GameLayer layer = GetLayer(_Grid);
		
		GameCell instance = (GameCell)PrefabUtility.InstantiatePrefab(cell, layer != null ? layer.transform : null);
		
		Undo.RegisterCreatedObjectUndo(instance.gameObject, "Paint Game Cells");
		
		Transform transform = instance.transform;
		transform.position = _Grid.CellToWorld(_Position);
		transform.rotation = Quaternion.identity;
	}

	GameLayer GetLayer(GridLayout _Grid)
	{
		if (_Grid == null)
			return null;
		
		GameStage stage = _Grid.GetComponent<GameStage>();
		
		if (stage == null)
			return null;
		
		return stage.GetLayer(m_LayerType);
	}

	public override void Erase(GridLayout _Grid, GameObject _Target, Vector3Int _Position)
	{
		GameCell cell = GetGameCell(_Grid, _Position);
		
		if (cell != null)
			Undo.DestroyObjectImmediate(cell.gameObject);
	}

	[CustomEditor(typeof(GameCellBrush))]
	public class GameCellBrushEditor : GridBrushEditor
	{
		static readonly Dictionary<GameCell, Texture2D> m_Thumbnails = new Dictionary<GameCell, Texture2D>();

		GameCell m_Preview;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			DrawCells();
			
			serializedObject.ApplyModifiedProperties();
		}

		void DrawCells()
		{
			if (Event.current.type == EventType.DragPerform)
				return;
			
			SerializedProperty cellsProperty     = serializedObject.FindProperty("m_GameCells");
			SerializedProperty cellIndexProperty = serializedObject.FindProperty("m_CellIndex");
			
			int colCount = 8;
			int rowCount = Mathf.CeilToInt((float)cellsProperty.arraySize / colCount);
			
			Rect view = GUILayoutUtility.GetAspectRect((float)colCount / rowCount);
			
			float width  = view.width / colCount;
			float height = view.height / rowCount;
			
			for (int i = 0; i < cellsProperty.arraySize; i++)
			{
				SerializedProperty cellProperty = cellsProperty.GetArrayElementAtIndex(i);
				
				if (cellProperty == null)
					continue;
				
				GameCell cell = cellProperty.objectReferenceValue as GameCell;
				
				if (cell == null)
					continue;
				
				int x = i % colCount;
				int y = i / colCount;
				
				Rect rect = new Rect(
					view.x + width * x,
					view.y + height * y,
					width,
					height
				);
				
				GUI.DrawTexture(rect, GetThumbnail(cell));
				
				if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
					cellIndexProperty.intValue = i;
				
				if (cellIndexProperty.intValue == i)
				{
					Handles.DrawSolidRectangleWithOutline(
						new RectOffset(2, 2, 2, 2).Remove(rect),
						Color.clear,
						 new Color(0.34f, 0.61f, 0.84f)
					);
				}
			}
		}

		static Texture2D GetThumbnail(GameCell _Cell)
		{
			if (_Cell == null)
				return null;
			
			if (!m_Thumbnails.ContainsKey(_Cell))
				m_Thumbnails[_Cell] = AssetPreview.GetAssetPreview(_Cell.gameObject);
			
			return m_Thumbnails[_Cell];
		}

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
			
			SerializedProperty cellsProperty     = serializedObject.FindProperty("m_GameCells");
			SerializedProperty layerTypeProperty = serializedObject.FindProperty("m_LayerType");
			SerializedProperty cellIndexProperty = serializedObject.FindProperty("m_CellIndex");
			
			Tilemap tilemap = _Grid.GetComponent<Tilemap>();
			if (tilemap == null || !tilemap.HasTile(_Position))
				return;
			
			GridLayout grid = tilemap.layoutGrid;
			if (grid == null)
				return;
			
			GameStage stage = grid.GetComponent<GameStage>();
			if (stage == null)
				return;
			
			GameLayer layer = stage.GetLayer((GameLayerType)layerTypeProperty.enumValueIndex);
			if (layer == null)
				return;
			
			SerializedProperty cellProperty = cellsProperty.GetArrayElementAtIndex(cellIndexProperty.intValue);
			
			GameCell cell = cellProperty.objectReferenceValue as GameCell;
			
			if (cell == null)
				return;
			
			m_Preview = Instantiate(
				cell,
				_Grid.CellToWorld(_Position),
				Quaternion.identity,
				layer.transform
			);
			m_Preview.tag = "EditorOnly";
			m_Preview.gameObject.hideFlags = HideFlags.HideAndDontSave;
		}
	}
}