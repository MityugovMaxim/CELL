using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class ElementPanel : UIBehaviour
{
	[SerializeField] RectTransform m_Content;

	[SerializeField] float m_Width;

	List<ElementContainer>            m_Containers = new List<ElementContainer>();

	public void Swap(int _SourcePosition, int _TargetPosition)
	{
		
	}

	public void Reposition()
	{
		
	}

	public void Normalize()
	{
		
	}

	void Awake()
	{
		CollectContainers();
	}

	void OnEnable()
	{
		CollectContainers();
	}

	void OnTransformChildrenChanged()
	{
		CollectContainers();
	}

	void CollectContainers()
	{
		m_Containers.Clear();
		
		for (int i = 0; i < transform.childCount; i++)
		{
			ElementContainer child = transform.GetChild(i).GetComponent<ElementContainer>();;
			
			if (child == null || !child.gameObject.activeSelf)
				continue;
			
			child.Setup(this);
			
			m_Containers.Add(child);
		}
		
		Normalize();
		
		ProcessLayout();
	}

	public void Remap(ElementContainer _Element)
	{
		foreach (var element in m_Containers)
		{
			if (element != _Element && element.Position >= _Element.Position)
				element.Position++;
		}
		
		m_Containers.Sort((_A, _B) => _A.Position.CompareTo(_B.Position));
		
		for (int i = 0; i < m_Containers.Count; i++)
		{
			m_Containers[i].Position = i;
		}
	}

	public void Reposition()
	{
		m_Containers.Sort((_A, _B) => _A.Position.CompareTo(_B.Position));
		
		ProcessLayout();
	}

	void ProcessLayout()
	{
		StopAllCoroutines();
		
		const float spacing = 5;
		
		float position = 0;
		
		for (int i = 0; i < m_Containers.Count; i++)
		{
			ElementContainer child = m_Containers[i];
			
			if (child == null)
				continue;
			
			child.rectTransform.anchorMin = new Vector2(0, 0);
			child.rectTransform.anchorMax = new Vector2(0, 1);
			child.rectTransform.sizeDelta = new Vector2(160, 0);
			
			Vector2 pos = child.rectTransform.anchoredPosition;
			Vector2 size   = child.rectTransform.sizeDelta;
			Vector2 offset = Vector2.Scale(size, child.rectTransform.pivot);
			
			pos = new Vector2(offset.x + position, offset.y);
			
			if (Application.isPlaying && gameObject.activeInHierarchy)
				StartCoroutine(LayoutRoutine(child.rectTransform, pos));
			else
				child.rectTransform.anchoredPosition = pos;
			
			position += size.x + spacing;
		}
		
		RectTransform rectTransform = transform as RectTransform;
		rectTransform.anchorMin = new Vector2(0.5f, 0);
		rectTransform.anchorMax = new Vector2(0.5f, 1);
		rectTransform.pivot     = new Vector2(0.5f, 0.5f);
		
		if (Application.isPlaying && gameObject.activeInHierarchy)
			StartCoroutine(SizeRoutine(rectTransform, new Vector2(position - spacing, 0)));
		else
			rectTransform.sizeDelta = new Vector2(position - spacing, 0);
	}

	IEnumerator SizeRoutine(RectTransform _Target, Vector2 _Size)
	{
		Vector2 source = _Target.sizeDelta;
		Vector2 target = _Size;
		
		if (source == target)
			yield break;
		
		const float duration = 0.5f;
		
		float time = 0;
		while (time < duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			_Target.sizeDelta = Vector2.Lerp(source, target, time / duration);
		}
		
		_Target.sizeDelta = target;
	}

	IEnumerator LayoutRoutine(RectTransform _Target, Vector2 _Position)
	{
		Vector2 source = _Target.anchoredPosition;
		Vector2 target = _Position;
		
		if (source == target)
			yield break;
		
		const float duration = 0.5f;
		
		float time = 0;
		while (time < duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			_Target.anchoredPosition = Vector2.Lerp(source, target, time / duration);
		}
		
		_Target.anchoredPosition = target;
	}
}
