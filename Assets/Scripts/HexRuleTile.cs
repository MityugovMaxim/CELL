using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum HexRuleMode
{
	None      = 0,
	Any       = 6,
	Same      = 12,
	NotSame   = 18,
	NotEquals = 24,
}

public static class HexRuleExtension
{
	public static bool Match(this HexRuleMode _Mode, int _Pattern, int _Direction)
	{
		return (_Pattern & (1 << (_Direction + (int)_Mode))) > 0;
	}
}

[CanEditMultipleObjects]
[CustomEditor(typeof(HexRuleTile), true)]
public class HexRuleTileEditor : Editor
{
	struct Side
	{ 
		public readonly int     Direction;
		public readonly Vector2 Source;
		public readonly Vector2 Target;

		public HexRuleMode Mode;

		public Side(int _Direction, Vector2 _Source, Vector2 _Target, HexRuleMode _Mode)
		{
			Direction = _Direction;
			Source    = _Source;
			Target    = _Target;
			Mode      = _Mode;
		}
	}

	ReorderableList m_RulesList;

	void Awake()
	{
		m_RulesList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Rules"), true, true, true, true);
		
		m_RulesList.elementHeight = EditorGUIUtility.singleLineHeight * 4;
		
		m_RulesList.drawHeaderCallback += DrawHeader;
		
		m_RulesList.drawElementCallback += DrawRule;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		if (m_RulesList != null)
			m_RulesList?.DoLayoutList();
		
		serializedObject.ApplyModifiedProperties();
	}

	void DrawHeader(Rect _Rect)
	{
		EditorGUI.LabelField(_Rect, "Rules", EditorStyles.boldLabel);
		
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
					
					foreach (string path in DragAndDrop.paths)
					foreach (Sprite sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
						AddRule(sprite);
					
					Event.current.Use();
				}
				break;
		}
	}

	void AddRule(Sprite _Sprite)
	{
		if (m_RulesList == null)
			return;
		
		SerializedProperty rulesProperty  = serializedObject.FindProperty("m_Rules");
		
		int index = rulesProperty.arraySize;
		rulesProperty.InsertArrayElementAtIndex(index);
		
		SerializedProperty ruleProperty   = rulesProperty.GetArrayElementAtIndex(index);
		SerializedProperty spriteProperty = ruleProperty.FindPropertyRelative("m_Sprite");
		
		spriteProperty.objectReferenceValue = _Sprite;
		
		serializedObject.ApplyModifiedProperties();
	}

	void DrawRule(Rect _Rect, int _Index, bool _Active, bool _Focused)
	{
		Rect spriteRect = new Rect(
			_Rect.x,
			_Rect.y,
			_Rect.height,
			_Rect.height
		);
		
		Rect patternRect = new Rect(
			_Rect.x + _Rect.height,
			_Rect.y,
			_Rect.height,
			_Rect.height
		);
		
		Rect previewRect = new Rect(
			_Rect.x + _Rect.height * 3,
			_Rect.y,
			_Rect.height,
			_Rect.height
		);
		
		SerializedProperty ruleProperty     = m_RulesList.serializedProperty.GetArrayElementAtIndex(_Index);
		SerializedProperty spriteProperty   = ruleProperty.FindPropertyRelative("m_Sprite");
		SerializedProperty patternProperty  = ruleProperty.FindPropertyRelative("m_Pattern");
		
		DrawRuleSprite(spriteRect, spriteProperty);
		
		DrawRulePattern(patternRect, patternProperty);
		
		DrawPreview(previewRect, spriteProperty);
		
		ruleProperty.serializedObject.ApplyModifiedProperties();
	}

	static void DrawRuleSprite(Rect _Rect, SerializedProperty _Property)
	{
		_Property.objectReferenceValue = EditorGUI.ObjectField(_Rect, _Property.objectReferenceValue, typeof(Sprite), false) as Sprite;
	}

	static void DrawRulePattern(Rect _Rect, SerializedProperty _Property)
	{
		int pattern = _Property.intValue;
		
		float size = _Rect.height / 3;
		
		Side[] sides = new Side[HexUtility.NeighborsCount];
		for (int i = 0; i < sides.Length; i++)
		{
			float sourceAngle = 60 * i - 30;
			float targetAngle = 60 * (i + 1) - 30;
			
			Vector2 source = new Vector2(
				_Rect.center.x + size * Mathf.Cos(sourceAngle * Mathf.Deg2Rad),
				_Rect.center.y + size * Mathf.Sin(sourceAngle * Mathf.Deg2Rad)
			);
			
			Vector2 target = new Vector2(
				_Rect.center.x + size * Mathf.Cos(targetAngle * Mathf.Deg2Rad),
				_Rect.center.y + size * Mathf.Sin(targetAngle * Mathf.Deg2Rad)
			);
			
			sides[i] = new Side(
				i,
				source,
				target,
				GetMode(pattern, i)
			);
		}
		
		foreach (Side side in sides)
		{
			switch (side.Mode)
			{
				case HexRuleMode.None:
					Handles.color = new Color(0.4f, 0.4f, 0.4f);
					break;
				case HexRuleMode.Any:
					Handles.color = new Color(1, 1, 1);
					break;
				case HexRuleMode.Same:
					Handles.color = new Color(0f, 1f, 0.5f);
					break;
				case HexRuleMode.NotSame:
					Handles.color = new Color(1, 0.5f, 0.25f);
					break;
				case HexRuleMode.NotEquals:
					Handles.color = new Color(1, 0.25f, 0.25f);
					break;
				default:
					Handles.color = new Color(1, 0, 1);
					break;
			}
			
			Handles.DrawAAPolyLine(4, side.Source, side.Target);
			
			Handles.color = Color.white;
		}
		
		if (sides.Length != HexUtility.NeighborsCount)
			return;
		
		switch (Event.current.type)
		{
			case EventType.MouseDown:
			{
				Vector3 position = Event.current.mousePosition;
				
				if (!_Rect.Contains(position))
					break;
				
				Event.current.Use();
				
				int   direction   = 0;
				float minDistance = float.MaxValue;
				foreach (Side side in sides)
				{
					float distance = HandleUtility.DistanceToLine(side.Source, side.Target);
					
					if (minDistance > distance)
					{
						minDistance = distance;
						direction   = side.Direction;
					}
				}
				
				switch (sides[direction].Mode)
				{
					case HexRuleMode.None:
						sides[direction].Mode = HexRuleMode.Any;
						break;
					case HexRuleMode.Any:
						sides[direction].Mode = HexRuleMode.Same;
						break;
					case HexRuleMode.Same:
						sides[direction].Mode = HexRuleMode.NotSame;
						break;
					case HexRuleMode.NotSame:
						sides[direction].Mode = HexRuleMode.NotEquals;
						break;
					case HexRuleMode.NotEquals:
						sides[direction].Mode = HexRuleMode.None;
						break;
				}
				
				pattern = 0;
				foreach (Side side in sides)
					pattern |= 1 << (side.Direction + (int)side.Mode);
				
				_Property.intValue = pattern;
				
				break;
			}
		}
	}

	static HexRuleMode GetMode(int _Pattern, int _Direction)
	{
		foreach (HexRuleMode mode in Enum.GetValues(typeof(HexRuleMode)))
		{
			if (mode.Match(_Pattern, _Direction))
				return mode;
		}
		return HexRuleMode.Any;
	}

	static void DrawPreview(Rect _Rect, SerializedProperty _SpriteProperty)
	{
		Sprite sprite = _SpriteProperty.objectReferenceValue as Sprite;
		
		if (sprite == null || sprite.texture == null)
			return;
		
		EditorGUI.DrawTextureTransparent(_Rect, sprite.texture);
	}
}

[CreateAssetMenu(fileName = "Hex Rule Tile", menuName = "Map/Hex Rule Tile")]
public class HexRuleTile : TileBase
{
	[Serializable]
	public struct Rule
	{
		public int Pattern
		{
			get { return m_Pattern; }
		}

		public Sprite Sprite
		{
			get { return m_Sprite; }
		}

		[SerializeField] int        m_Pattern;
		[SerializeField] Sprite     m_Sprite;

		public Rule(int _Pattern, Sprite _Sprite)
		{
			m_Pattern  = _Pattern;
			m_Sprite   = _Sprite;
		}
	}

	[SerializeField] Sprite            m_Sprite       = default;
	[SerializeField] GameObject        m_Object       = default;
	[SerializeField] Color             m_Color        = Color.white;
	[SerializeField] TileFlags         m_Flags        = TileFlags.LockTransform | TileFlags.LockColor;
	[SerializeField] Matrix4x4         m_Transform    = Matrix4x4.identity;
	[SerializeField] Tile.ColliderType m_ColliderType = Tile.ColliderType.Sprite;
	[SerializeField] Rule[]            m_Rules        = default;

	[ContextMenu("Reset transform")]
	public void ResetTransform()
	{
		m_Transform = Matrix4x4.identity;
	}

	public override void GetTileData(Vector3Int _Position, ITilemap _Tilemap, ref TileData _TileData)
	{
		_TileData.sprite       = m_Sprite;
		_TileData.gameObject   = m_Object;
		_TileData.color        = m_Color;
		_TileData.transform    = m_Transform;
		_TileData.flags        = m_Flags;
		_TileData.colliderType = m_ColliderType;
		
		foreach (Rule rule in m_Rules)
		{
			if (MatchRule(rule, _Position, _Tilemap, out int rotation))
			{
				_TileData.sprite    =  rule.Sprite;
				_TileData.transform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, -rotation * 60));
				break;
			}
		}
	}

	public override void RefreshTile(Vector3Int _Position, ITilemap _Tilemap)
	{
		base.RefreshTile(_Position, _Tilemap);
		
		for (var i = 0; i < HexUtility.NeighborsCount; i++)
		{
			Vector3Int position = HexUtility.GetNeighborPosition(_Position, i);
			if (ContainsThis(position, _Tilemap))
				_Tilemap.RefreshTile(position);
		}
	}

	bool ContainsThis(Vector3Int _Position, ITilemap _Tilemap)
	{
		TileBase tile = _Tilemap.GetTile(_Position);
		return tile != null && tile == this;
	}

	bool ContainsOther(Vector3Int _Position, ITilemap _Tilemap)
	{
		TileBase tile = _Tilemap.GetTile(_Position);
		return tile != null && tile != this;
	}

	bool MatchRule(Rule _Rule, Vector3Int _Position, ITilemap _Tilemap, out int _Rotation)
	{
		_Rotation = 0;
		for (int i = 0; i < HexUtility.NeighborsCount; i++)
		{
			_Rotation = i;
			if (MatchPattern(_Rule.Pattern, _Position, _Tilemap, _Rotation))
				return true;
		}
		return false;
	}

	bool MatchPattern(int _Pattern, Vector3Int _Position, ITilemap _Tilemap, int _Rotation)
	{
		for (int i = 0; i < HexUtility.NeighborsCount; i++)
		{
			Vector3Int position = HexUtility.GetNeighborPosition(_Position, (i + _Rotation) % HexUtility.NeighborsCount);
			
			HexRuleMode mode = GetMode(_Pattern, i);
			
			if (MatchMode(mode, position, _Tilemap))
				continue;
			
			return false;
		}
		return true;
	}

	static HexRuleMode GetMode(int _Pattern, int _Direction)
	{
		foreach (HexRuleMode mode in Enum.GetValues(typeof(HexRuleMode)))
		{
			if (mode.Match(_Pattern, _Direction))
				return mode;
		}
		return HexRuleMode.None;
	}

	bool MatchMode(HexRuleMode _Mode, Vector3Int _Position, ITilemap _Tilemap)
	{
		TileBase tile = _Tilemap.GetTile(_Position);
		switch (_Mode)
		{
			case HexRuleMode.None:      return tile == null;
			case HexRuleMode.Any:       return true;
			case HexRuleMode.Same:      return tile != null && tile == this;
			case HexRuleMode.NotSame:   return tile != null && tile == this;
			case HexRuleMode.NotEquals: return tile != this;
			default:                    return false;
		}
	}
}
