using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class ElementContainer : UIEventReceiver, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public TileBase Tile { get; set; }

	public int Position
	{
		get { return m_Position; }
		set { m_Position = value; }
	}

	[SerializeField] int m_Position;

	ElementPanel m_Panel;
	Transform    m_Parent;

	bool m_Drag;

	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (m_Panel != null)
			m_Panel.Reposition();
	}

	public void Setup(ElementPanel _Panel)
	{
		m_Panel = _Panel;
	}

	public void ToFront()
	{
		m_Position = 0;
		
		if (m_Panel != null)
		{
			m_Panel.Remap(this);
			m_Panel.Reposition();
		}
	}

	public void OnBeginDrag(PointerEventData _Data)
	{
		if (m_Drag || Mathf.Abs(_Data.delta.x) > Mathf.Abs(_Data.delta.y))
			return;
		
		m_Drag = true;
		rectTransform.GetSiblingIndex();
		m_Parent = rectTransform.parent;
		
		rectTransform.SetParent(rectTransform.parent.parent, true);
		
		_Data.Use();
	}

	public void OnDrag(PointerEventData _Data)
	{
		if (!m_Drag)
			return;
		
		Vector2 delta = _Data.delta;
		
		delta = GetPosition(delta);
		
		rectTransform.anchoredPosition += delta;
		
		_Data.Use();
	}

	public void OnEndDrag(PointerEventData _Data)
	{
		if (!m_Drag)
			return;
		
		m_Drag = false;
		
		rectTransform.SetParent(m_Parent, true);
		rectTransform.SetAsLastSibling();
	}
}
