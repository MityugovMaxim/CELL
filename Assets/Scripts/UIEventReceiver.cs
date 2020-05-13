using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIEventReceiver : Graphic
{
	static readonly List<RaycastResult> m_RaycastResults = new List<RaycastResult>();

	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
	}

	protected Vector2 GetPosition(Vector2 _Position)
	{
		return transform.InverseTransformVector(_Position);
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