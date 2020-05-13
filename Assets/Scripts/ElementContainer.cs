using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElementContainer : UIEventReceiver, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
	public static ElementContainer Create(ElementPanel _Panel, int _Axis)
	{
		GameObject containerObject = new GameObject("container");
		ElementContainer container = containerObject.AddComponent<ElementContainer>();
		container.Setup(_Panel, _Axis);
		return container;
	}

	public bool IgnoreLayout { get; protected set; }

	protected ScrollRect ScrollRect
	{
		get
		{
			if (m_ScrollRect == null)
				m_ScrollRect = GetComponentInParent<ScrollRect>();
			return m_ScrollRect;
		}
	}

	protected bool Drag { get; set; }

	[SerializeField] float          m_MoveDuration = 0.5f;
	[SerializeField] AnimationCurve m_MoveCurve    = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[NonSerialized] ScrollRect   m_ScrollRect;
	[NonSerialized] ElementPanel m_Panel;
	[NonSerialized] int          m_Axis;
	[NonSerialized] Vector2      m_MovePosition;
	[NonSerialized] IEnumerator  m_MoveRoutine;

	public void Setup(ElementPanel _Panel, int _Axis)
	{
		m_Panel = _Panel;
		m_Axis  = _Axis;
	}

	public void BringToFront()
	{
		transform.SetAsLastSibling();
	}

	public void BringToBack()
	{
		transform.SetAsFirstSibling();
	}

	public void Move(Vector2 _Position, bool _Instant = false)
	{
		if (Drag || m_MovePosition == _Position)
			return;
		
		if (m_MoveRoutine != null)
			StopCoroutine(m_MoveRoutine);
		
		m_MovePosition = _Position;
		
		if (!_Instant && Application.isPlaying && gameObject.activeInHierarchy)
			StartCoroutine(m_MoveRoutine = MoveRoutine());
		else
			rectTransform.anchoredPosition = m_MovePosition;
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData _Event)
	{
		if (m_Panel == null || Drag)
		{
			PassEvent(_Event, ExecuteEvents.beginDragHandler);
			return;
		}
		
		switch (m_Axis)
		{
			case 0:
				if (Mathf.Abs(_Event.delta.x) > Mathf.Abs(_Event.delta.y))
				{
					PassEvent(_Event, ExecuteEvents.beginDragHandler);
					return;
				}
				break;
			case 1:
				if (Mathf.Abs(_Event.delta.x) < Mathf.Abs(_Event.delta.y))
				{
					PassEvent(_Event, ExecuteEvents.beginDragHandler);
					return;
				}
				break;
		}
		
		if (ScrollRect != null)
			ScrollRect.StopMovement();
		
		Drag = true;
		
		m_MovePosition = Vector2.positiveInfinity;
		
		rectTransform.anchoredPosition += GetLocalPosition(_Event.delta);
		
		BringToFront();
		
		if (m_Panel != null)
			m_Panel.Reposition();
		
		_Event.Use();
	}

	void IDragHandler.OnDrag(PointerEventData _Event)
	{
		if (!Drag)
		{
			PassEvent(_Event, ExecuteEvents.dragHandler);
			return;
		}
		
		rectTransform.anchoredPosition += GetLocalPosition(_Event.delta);
		
		if (m_Panel != null && m_Panel.Replace(this))
			m_Panel.Reposition();
		
		_Event.Use();
	}

	void IEndDragHandler.OnEndDrag(PointerEventData _Event)
	{
		if (!Drag)
		{
			PassEvent(_Event, ExecuteEvents.endDragHandler);
			return;
		}
		
		Drag = false;
		
		if (m_Panel != null)
		{
			m_Panel.Replace(this);
			m_Panel.Reposition();
		}
		
		_Event.Use();
	}

	public void OnPointerDown(PointerEventData _Event)
	{
		if (ScrollRect != null)
			ScrollRect.StopMovement();
	}

	IEnumerator MoveRoutine()
	{
		Vector2 source = rectTransform.anchoredPosition;
		Vector2 target = m_MovePosition;
		
		if (source == target)
			yield break;
		
		float time = 0;
		while (time < m_MoveDuration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_MoveCurve.Evaluate(time / m_MoveDuration);
			
			rectTransform.anchoredPosition = Vector2.Lerp(source, target, phase);
		}
		rectTransform.anchoredPosition = target;
	}
}
