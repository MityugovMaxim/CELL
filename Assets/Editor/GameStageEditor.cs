using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(GameStage))]
public class GameStageEditor : Editor
{
	ReorderableList m_LayersList;

	readonly HashSet<string> m_LayerNames = new HashSet<string>();
	readonly HashSet<int>    m_LayerTypes = new HashSet<int>();

	void OnEnable()
	{
		CreateLayers();
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		DrawLayers();
		
		DrawWarning();
		
		serializedObject.ApplyModifiedProperties();
	}

	void CreateLayers()
	{
		SerializedProperty layersProperty = serializedObject.FindProperty("m_Layers");
		
		m_LayersList = new ReorderableList(serializedObject, layersProperty, true, true, true, true);
		
		m_LayersList.drawElementBackgroundCallback += DrawBackground; 
		
		m_LayersList.drawHeaderCallback += DrawHeader;
		
		m_LayersList.drawElementCallback += DrawLayer;
		
		m_LayersList.onAddCallback += AddLayer;
		
		m_LayersList.onRemoveCallback += RemoveLayer;
		
		m_LayersList.onReorderCallback += SortLayers;
		
		m_LayersList.onSelectCallback += SelectLayer;
	}

	void DrawLayers()
	{
		m_LayerNames.Clear();
		m_LayerTypes.Clear();
		
		if (m_LayersList != null)
			m_LayersList.DoLayoutList();
	}

	void DrawWarning()
	{
		if (m_LayersList == null)
			return;
		
		HashSet<string> layerNames = new HashSet<string>();
		HashSet<int>    layerTypes = new HashSet<int>();
		
		StringBuilder errors   = new StringBuilder();
		StringBuilder warnings = new StringBuilder();
		
		for (int i = 0; i < m_LayersList.count; i++)
		{
			SerializedProperty layerProperty = m_LayersList.serializedProperty.GetArrayElementAtIndex(i);
			
			if (layerProperty == null)
			{
				errors.AppendFormat(" - Layer at index '{0}' is missing", i).AppendLine();
				continue;
			}
			
			GameLayer layer = layerProperty.objectReferenceValue as GameLayer;
			
			if (layer == null)
			{
				errors.AppendFormat(" - Layer at index '{0}' is null", i).AppendLine();
				continue;
			}
			
			SerializedObject layerObject = new SerializedObject(layer);
			
			if (layerNames.Contains(layer.name))
				warnings.AppendFormat(" - Layer at index '{0}' using already existing name '{1}'", i, layer.name);
			
			SerializedProperty typeProperty = layerObject.FindProperty("m_Type");
			if (layerTypes.Contains(typeProperty.enumValueIndex))
				warnings.AppendFormat(" - Layer at index '{0}' using already existing type '{1}'", i, (GameLayerType)typeProperty.enumValueIndex).AppendLine();
			
			SerializedProperty groundProperty = layerObject.FindProperty("m_Ground");
			if (groundProperty.objectReferenceValue == null)
				warnings.AppendFormat(" - Layer at index '{0}' ground is null", i).AppendLine();
			
			layerNames.Add(layer.name);
			layerTypes.Add(typeProperty.enumValueIndex);
		}
		
		if (errors.Length > 0)
			EditorGUILayout.HelpBox(errors.ToString().TrimEnd(), MessageType.Error);
		
		if (warnings.Length > 0)
			EditorGUILayout.HelpBox(warnings.ToString().TrimEnd(), MessageType.Warning);
	}

	void DrawHeader(Rect _Rect)
	{
		EditorGUI.LabelField(_Rect, "Layers", EditorStyles.boldLabel);
	}

	void DrawBackground(Rect _Rect, int _Index, bool _Active, bool _Focused)
	{
		if (m_LayersList.count == 0)
			return;
		
		if (_Active || _Focused)
			EditorGUI.DrawRect(_Rect, new Color(0.34f, 0.61f, 0.84f));
		
		Rect indicatorRect = new Rect(_Rect.x, _Rect.y, 4, _Rect.height);
		
		bool error = false;
		
		SerializedProperty layerProperty = m_LayersList.serializedProperty.GetArrayElementAtIndex(_Index);
		
		if (layerProperty == null)
			error = true;
		
		GameLayer layer = layerProperty?.objectReferenceValue as GameLayer;
		if (layer == null)
			error = true;
		
		if (error)
		{
			Handles.DrawSolidRectangleWithOutline(
				indicatorRect,
				new Color(0.81f, 0.36f, 0.34f),
				new Color(0.12f, 0.12f, 0.12f)
			);
			return;
		}
		
		bool warning = false;
		
		SerializedObject layerObject = new SerializedObject(layer);
		
		if (m_LayerNames.Contains(layer.name))
			warning = true;
		
		SerializedProperty typeProperty = layerObject.FindProperty("m_Type");
		if (m_LayerTypes.Contains(typeProperty.enumValueIndex))
			warning = true;
		
		SerializedProperty groundProperty = layerObject.FindProperty("m_Ground");
		if (groundProperty.objectReferenceValue == null)
			warning = true;
		
		m_LayerNames.Add(layer.name);
		m_LayerTypes.Add(typeProperty.enumValueIndex);
		
		if (warning)
		{
			Handles.DrawSolidRectangleWithOutline(
				indicatorRect,
				new Color(1f, 0.72f, 0f),
				new Color(0.12f, 0.12f, 0.12f)
			);
			return;
		}
		
		Handles.DrawSolidRectangleWithOutline(
			indicatorRect,
			new Color(0.3f, 0.77f, 0.67f),
			new Color(0.12f, 0.12f, 0.12f)
		);
	}

	void DrawLayer(Rect _Rect, int _Index, bool _Active, bool _Focused)
	{
		Rect indexRect = new Rect(_Rect.x, _Rect.y, 15, _Rect.height);
		
		SerializedProperty layerProperty = m_LayersList.serializedProperty.GetArrayElementAtIndex(_Index);
		
		GUI.contentColor = _Active || _Focused ? Color.grey : Color.white;
		EditorGUI.LabelField(indexRect, _Index.ToString(), EditorStyles.label);
		GUI.contentColor = Color.white;
		
		RectOffset padding = new RectOffset(2, 2, 0, 0);
		
		GameLayer layer = layerProperty.objectReferenceValue as GameLayer;
		if (layer == null)
		{
			Rect layerRect = new Rect(_Rect.x + 15, _Rect.y + 1, _Rect.width - 15, _Rect.height - 2);
			
			layerRect = padding.Remove(layerRect);
			
			EditorGUI.PropertyField(layerRect, layerProperty, GUIContent.none);
		}
		else
		{
			float step = (_Rect.width - 15) / 3;
			
			Rect nameRect   = new Rect(_Rect.x + step * 0 + 15, _Rect.y + 1, step, _Rect.height - 2);
			Rect typeRect   = new Rect(_Rect.x + step * 1 + 15, _Rect.y + 1.5f, step, _Rect.height);
			Rect groundRect = new Rect(_Rect.x + step * 2 + 15, _Rect.y + 1, step, _Rect.height - 2);
			
			nameRect   = padding.Remove(nameRect);
			typeRect   = padding.Remove(typeRect);
			groundRect = padding.Remove(groundRect);
			
			layer.name = EditorGUI.DelayedTextField(nameRect, layer.name);
			
			SerializedObject layerObject = new SerializedObject(layer);
			
			EditorGUI.BeginChangeCheck();
			
			SerializedProperty typeProperty = layerObject.FindProperty("m_Type");
			
			EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
			
			SerializedProperty groundProperty = layerObject.FindProperty("m_Ground");
			EditorGUI.PropertyField(groundRect, groundProperty, GUIContent.none);
			
			if (EditorGUI.EndChangeCheck())
				layerObject.ApplyModifiedProperties();
		}
	}

	static void SelectLayer(ReorderableList _List)
	{
		SerializedProperty layerProperty = _List.serializedProperty.GetArrayElementAtIndex(_List.index);
		
		if (layerProperty == null)
			return;
		
		GameLayer layer = layerProperty.objectReferenceValue as GameLayer;
		
		if (layer == null)
			return;
		
		EditorGUIUtility.PingObject(layer.gameObject);
	}

	static void SortLayers(ReorderableList _List)
	{
		for (int i = 0; i < _List.count; i++)
		{
			SerializedProperty layerProperty = _List.serializedProperty.GetArrayElementAtIndex(i);
			
			if (layerProperty == null)
				continue;
			
			GameLayer layer = layerProperty.objectReferenceValue as GameLayer;
			
			if (layer == null)
				continue;
			
			SortingGroup sortingGroup = layer.GetComponent<SortingGroup>();
			
			if (sortingGroup == null)
				continue;
			
			sortingGroup.sortingOrder = i;
			
			sortingGroup.transform.SetAsLastSibling();
		}
	}

	static void AddLayer(ReorderableList _List)
	{
		GameStage stage = _List.serializedProperty.serializedObject.targetObject as GameStage;
		
		if (stage == null)
			return;
		
		int index = _List.count;
		
		GameObject gameObject = new GameObject(
			"layer",
			typeof(GameLayer),
			typeof(SortingGroup)
		);
		
		gameObject.transform.SetParent(stage.transform);
		
		GameLayer layer = gameObject.GetComponent<GameLayer>();
		
		ReorderableList.defaultBehaviours.DoAddButton(_List);
		
		SerializedProperty layerProperty = _List.serializedProperty.GetArrayElementAtIndex(index);
		
		_List.index = index;
		
		layerProperty.objectReferenceValue = layer;
		
		SerializedObject layerObject = new SerializedObject(layer);
		SerializedProperty groundProperty = layerObject.FindProperty("m_Ground");
		groundProperty.objectReferenceValue = stage.GetComponentInChildren<Tilemap>();
		layerObject.ApplyModifiedProperties();
		
		SortLayers(_List);
	}

	static void RemoveLayer(ReorderableList _List)
	{
		int index = _List.index;
		
		SerializedProperty layerProperty = _List.serializedProperty.GetArrayElementAtIndex(index);
		
		GameLayer layer = layerProperty.objectReferenceValue as GameLayer;
		
		if (layer != null)
			DestroyImmediate(layer.gameObject);
		
		layerProperty.objectReferenceValue = null;
		
		ReorderableList.defaultBehaviours.DoRemoveButton(_List);
		
		_List.index = Mathf.Clamp(index, 0, _List.count - 1);
		
		SortLayers(_List);
	}
}