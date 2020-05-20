using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameCellRegistry), true)]
public class GameCellRegistryDrawer : PropertyDrawer
{
	const float LINE_HEIGHT  = 18;
	const float LINE_SPACING = 2;

	public override float GetPropertyHeight(SerializedProperty _Property, GUIContent _Label)
	{
		SerializedProperty gameCellIDs = _Property.FindPropertyRelative("m_GameCellIDs");
		
		return gameCellIDs.isExpanded
			? (LINE_HEIGHT + LINE_SPACING) * (gameCellIDs.arraySize + 2) - LINE_SPACING
			: base.GetPropertyHeight(_Property, _Label);
	}

	public override void OnGUI(Rect _Rect, SerializedProperty _Property, GUIContent _Label)
	{
		SerializedProperty gameCellIDs = _Property.FindPropertyRelative("m_GameCellIDs");
		
		DragDrop(GetRect(_Rect, 0), gameCellIDs);
		
		gameCellIDs.isExpanded = EditorGUI.Foldout(GetRect(_Rect, 0), gameCellIDs.isExpanded, _Label, true);
		
		if (!gameCellIDs.isExpanded)
			return;
		
		EditorGUI.indentLevel += 1;
		
		int arraySize = EditorGUI.DelayedIntField(GetRect(_Rect, 1), "Size", gameCellIDs.arraySize);
		
		if (gameCellIDs.arraySize != arraySize)
			gameCellIDs.arraySize = arraySize;
		
		DrawGameCells(_Rect, gameCellIDs);
		
		EditorGUI.indentLevel -= 1;
	}

	static void DrawGameCells(Rect _Rect, SerializedProperty _Property)
	{
		for (int i = 0; i < _Property.arraySize; i++)
		{
			SerializedProperty gameCellID = _Property.GetArrayElementAtIndex(i);
			
			GameCell gameCell = EditorUtility.InstanceIDToObject(gameCellID.intValue) as GameCell;
			
			Rect rect = GetRect(_Rect, i + 2);
			
			Rect gameCellRect = new RectOffset(0, 20, 0, 0).Remove(rect);
			
			Rect removeButtonRect = new RectOffset((int)rect.width - 20, 0, 0, 0).Remove(rect);
			
			gameCell = EditorGUI.ObjectField(gameCellRect, gameCell, typeof(GameCell), false) as GameCell;
			
			GUI.backgroundColor = new Color(0.81f, 0.36f, 0.34f);
			if (GUI.Button(removeButtonRect, "X"))
			{
				_Property.DeleteArrayElementAtIndex(i);
				return;
			}
			GUI.backgroundColor = Color.white;
			
			int instanceID = gameCell != null ? gameCell.GetInstanceID() : 0;
			
			if (gameCellID.intValue != instanceID)
				gameCellID.intValue = instanceID;
		}
	}

	static void DragDrop(Rect _Rect, SerializedProperty _Property)
	{
		switch (Event.current.type)
		{
			case EventType.DragUpdated:
				if (_Rect.Contains(Event.current.mousePosition))
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					
					Event.current.Use();
				}
				break;
			case EventType.DragPerform:
				if (_Rect.Contains(Event.current.mousePosition))
				{
					DragAndDrop.AcceptDrag();
					
					int[] gameCellIDs = DragAndDrop.paths
						.Select(AssetDatabase.LoadAssetAtPath<GameCell>)
						.Select(_GameCell => _GameCell.ID)
						.ToArray();
					
					int index = _Property.arraySize;
					for (int i = 0; i < gameCellIDs.Length; i++)
					{
						_Property.InsertArrayElementAtIndex(index + i);
						SerializedProperty gameCellID = _Property.GetArrayElementAtIndex(i);
						gameCellID.intValue = gameCellIDs[i];
					}
					
					_Property.isExpanded = true;
					
					Event.current.Use();
				}
				break;
		}
	}

	static Rect GetRect(Rect _Rect, int _Index)
	{
		return new Rect(
			_Rect.x,
			_Rect.y + (LINE_HEIGHT + LINE_SPACING) * _Index - LINE_SPACING,
			_Rect.width,
			LINE_HEIGHT
		);
	}
}