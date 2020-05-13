using UnityEngine;

public class NodePanel : ElementPanel
{
	[SerializeField] RectTransform m_Root = default;
	[SerializeField] Map           m_Map  = default;
	[SerializeField] Node          m_Node = default;

	public void DragContainer(NodeContainer _Container)
	{
		if (_Container == null)
		{
			m_Map.ClearHighlight();
			return;
		}
		
		if (_Container.Intersect(m_Root))
		{
			if (_Container.rectTransform.parent != RectTransform)
				_Container.rectTransform.SetParent(RectTransform, true);
			
			if (Replace(_Container))
				Reposition();
			
			m_Map.ClearHighlight();
		}
		else
		{
			if (_Container.rectTransform.parent != m_Root)
				_Container.rectTransform.SetParent(m_Root, true);
			
			Rect rect = _Container.GetWorldRect();
			
			Vector3 position = rect.center;
			
			m_Map.Highlight(position);
		}
	}

	public void DropContainer(NodeContainer _Container)
	{
		m_Map.ClearHighlight();
		
		if (_Container == null)
		{
			Reposition();
			return;
		}
		
		Rect rect = _Container.GetWorldRect();
		
		Vector3 position = rect.center;
		
		if (m_Map.Contains(position) || _Container.Intersect(m_Root))
		{
			if (_Container.rectTransform.parent != RectTransform)
				_Container.rectTransform.SetParent(RectTransform, true);
			
			Replace(_Container);
			Reposition();
		}
		else
		{
			m_Map.Add(position, m_Node);
			
			Destroy(_Container.gameObject);
		}
	}
}