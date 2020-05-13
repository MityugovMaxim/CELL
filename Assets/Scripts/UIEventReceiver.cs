using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIEventReceiver : Graphic
{
	public override Material material
	{
		get { return null; }
	}

	public override Color color
	{
		get { return Color.clear; }
	}

	static readonly List<RaycastResult> m_RaycastResults = new List<RaycastResult>();

	bool m_Destroying;

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		
		m_Destroying = true;
	}

	public bool IsDestroying()
	{
		return m_Destroying;
	}

	public Rect GetLocalRect()
	{
		return rectTransform.rect;
	}

	public Rect GetWorldRect()
	{
		return rectTransform.TransformRect(rectTransform.rect);
	}

	public bool Intersect(RectTransform _Target)
	{
		if (_Target == null)
			return false;
		
		Rect source = GetWorldRect();
		Rect target = _Target.TransformRect(_Target.rect);
		
		return source.Overlaps(target);
	}

	protected Vector2 GetDelta(PointerEventData _Event)
	{
		return GetDelta(_Event.pressEventCamera, _Event);
	}

	protected Vector2 GetDelta(Camera _Camera, PointerEventData _Event)
	{
		Vector2 source = _Camera.ScreenToWorldPoint(_Event.position - _Event.delta);
		Vector2 target = _Camera.ScreenToWorldPoint(_Event.position);
		source = rectTransform.InverseTransformPoint(source);
		target = rectTransform.InverseTransformPoint(target);
		return target - source;
	}

	protected Vector2 GetPosition(PointerEventData _Event)
	{
		return GetPosition(_Event.pressEventCamera, _Event);
	}

	protected Vector2 GetPosition(Camera _Camera, PointerEventData _Event)
	{
		Vector2 position = _Camera.ScreenToWorldPoint(_Event.position);
		return rectTransform.InverseTransformPoint(position);
	}

	protected void PassEvent<T>(PointerEventData _Event, ExecuteEvents.EventFunction<T> _Function) where T : IEventSystemHandler
	{
		if (_Event == null || _Event.used)
			return;
		
		EventSystem.current.RaycastAll(_Event, m_RaycastResults);
		
		GameObject target = null;
		foreach (RaycastResult raycastResult in m_RaycastResults)
		{
			if (raycastResult.gameObject == gameObject)
				continue;
			
			target = raycastResult.gameObject;
			
			break;
		}
		
		if (target == null)
			return;
		
		ExecuteEvents.Execute(target, _Event, _Function);
	}
}