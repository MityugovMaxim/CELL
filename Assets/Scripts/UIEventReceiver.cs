using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIEventReceiver : Graphic
{
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
		return GetPixelAdjustedRect();
	}

	public Rect GetWorldRect()
	{
		Rect rect = GetPixelAdjustedRect();
		return new Rect(
			rectTransform.TransformPoint(rect.position),
			rectTransform.TransformVector(rect.size)
		);
	}

	protected Vector2 GetLocalPosition(Vector2 _Position)
	{
		return transform.InverseTransformVector(_Position);
	}

	protected Vector2 GetWorldPosition(Vector2 _Position)
	{
		return transform.TransformVector(_Position);
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