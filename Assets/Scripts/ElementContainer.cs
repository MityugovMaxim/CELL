using UnityEngine;
using UnityEngine.EventSystems;

public class ElementContainer : UIEventReceiver, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public bool IgnoreLayout { get; protected set; }

	public bool Drag { get; protected set; }

	ElementPanel m_Panel;

	public void Setup(ElementPanel _Panel)
	{
		m_Panel = _Panel;
	}

	public void BringToFront()
	{
		transform.SetAsLastSibling();
	}

	public void BringToBack()
	{
		transform.SetAsFirstSibling();
	}

	public void OnBeginDrag(PointerEventData _Event)
	{
		if (m_Panel == null || Drag || Mathf.Abs(_Event.delta.x) > Mathf.Abs(_Event.delta.y))
		{
			PassEvent(_Event, ExecuteEvents.beginDragHandler);
			return;
		}
		
		Drag         = true;
		IgnoreLayout = true;
		
		rectTransform.anchoredPosition += GetPosition(_Event.delta);
		
		BringToFront();
		
		if (m_Panel != null)
			m_Panel.Reposition();
		
		_Event.Use();
	}

	public void OnDrag(PointerEventData _Event)
	{
		if (!Drag)
		{
			PassEvent(_Event, ExecuteEvents.dragHandler);
			return;
		}
		
		rectTransform.anchoredPosition += GetPosition(_Event.delta);
		
		_Event.Use();
	}

	public void OnEndDrag(PointerEventData _Event)
	{
		if (!Drag)
		{
			PassEvent(_Event, ExecuteEvents.endDragHandler);
			return;
		}
		
		Drag         = false;
		IgnoreLayout = false;
		
		if (m_Panel != null)
		{
			m_Panel.Replace(this);
			m_Panel.Reposition();
		}
		
		_Event.Use();
	}
}
