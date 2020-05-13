using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class ElementPanel : UIBehaviour
{
	public enum Direction
	{
		Horizontal,
		Vertical
	}

	public enum Alignment
	{
		Start,
		Center,
		End
	}

	RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	[SerializeField] Direction      m_Direction = Direction.Horizontal;
	[SerializeField] Alignment      m_Alignment = Alignment.Center;
	[SerializeField] float          m_Size      = 100;
	[SerializeField] float          m_Spacing   = default;
	[SerializeField] float          m_Duration  = 0.5f;
	[SerializeField] AnimationCurve m_Curve     = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[SerializeField] List<ElementContainer> m_Containers = new List<ElementContainer>();

	RectTransform m_RectTransform;
	IEnumerator   m_ResizeRoutine;

	protected override void Awake()
	{
		base.Awake();
		
		CollectContainers();
		
		Reposition(true);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		CollectContainers();
		
		Reposition(true);
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		
		Reposition();
	}

	void OnTransformChildrenChanged()
	{
		CollectContainers();
		
		#if UNITY_EDITOR
		if (Application.isPlaying)
			Reposition(true);
		else
			UnityEditor.EditorApplication.delayCall += () => Reposition();
		#else
		Reposition();
		#endif
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		UnityEditor.EditorApplication.delayCall += () => Reposition(true);
	}
	#endif

	public bool Replace(ElementContainer _Container)
	{
		if (_Container == null)
			return false;
		
		int sourceIndex = m_Containers.IndexOf(_Container);
		int targetIndex = sourceIndex;
		
		if (sourceIndex < 0)
			return false;
		
		Rect source = _Container.GetWorldRect();
		
		float position;
		switch (m_Direction)
		{
			case Direction.Horizontal:
				position = source.center.x;
				break;
			case Direction.Vertical:
				position = source.center.y;
				break;
			default:
				return false;
		}
		
		float distance = float.MaxValue;
		int index = 0;
		foreach (ElementContainer container in m_Containers)
		{
			if (container == null || container == _Container)
				continue;
			
			Rect target = container.GetWorldRect();
			
			float minDistance;
			float maxDistance;
			
			switch (m_Direction)
			{
				case Direction.Horizontal:
					minDistance = Mathf.Abs(position - target.xMin);
					maxDistance = Mathf.Abs(position - target.xMax);
					break;
				case Direction.Vertical:
					minDistance = Mathf.Abs(position - target.yMax);
					maxDistance = Mathf.Abs(position - target.yMin);
					break;
				default:
					return false;
			}
			
			if (distance > minDistance)
			{
				distance    = minDistance;
				targetIndex = index;
			}
			
			if (distance > maxDistance)
			{
				distance    = maxDistance;
				targetIndex = index + 1;
			}
			
			index++;
		}
		
		if (sourceIndex == targetIndex)
			return false;
		
		m_Containers.RemoveAt(sourceIndex);
		m_Containers.Insert(targetIndex, _Container);
		
		return true;
	}

	public void Reposition(bool _Instant = false)
	{
		if (IsDestroyed())
			return;
		
		float alignment;
		switch (m_Alignment)
		{
			case Alignment.Start:
				alignment = 0;
				break;
			case Alignment.Center:
				alignment = 0.5f;
				break;
			case Alignment.End:
				alignment = 1;
				break;
			default:
				return;
		}
		
		int count = m_Containers.Count(_Container => !_Container.IgnoreLayout);
		
		float totalSize = count * (m_Size + m_Spacing) - m_Spacing;
		
		float position = totalSize * alignment * (m_Direction == Direction.Horizontal ? -1 : 1);
		
		foreach (ElementContainer container in m_Containers)
		{
			if (container == null || container.IgnoreLayout)
				continue;
			
			RectTransform rectTransform = container.rectTransform;
			
			if (rectTransform == null)
				continue;
			
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			
			switch (m_Direction)
			{
				case Direction.Horizontal:
					rectTransform.anchorMin = new Vector2(alignment, 0);
					rectTransform.anchorMax = new Vector2(alignment, 1);
					rectTransform.sizeDelta = new Vector2(m_Size, 0);
					container.Move(new Vector2(position + m_Size * 0.5f, 0), _Instant);
					position += m_Size + m_Spacing;
					break;
				case Direction.Vertical:
					rectTransform.anchorMin = new Vector2(0, 1 - alignment);
					rectTransform.anchorMax = new Vector2(1, 1 - alignment);
					rectTransform.sizeDelta = new Vector2(0, m_Size);
					container.Move(new Vector2(0, position - m_Size * 0.5f), _Instant);
					position -= m_Size + m_Spacing;
					break;
				default:
					return;
			}
		}
		
		switch (m_Direction)
		{
			case Direction.Horizontal:
				RectTransform.anchorMin = new Vector2(alignment, 0);
				RectTransform.anchorMax = new Vector2(alignment, 1);
				RectTransform.pivot     = new Vector2(alignment, 0.5f);
				Resize(new Vector2(totalSize, 0), _Instant);
				break;
			case Direction.Vertical:
				RectTransform.anchorMin = new Vector2(0, 1 - alignment);
				RectTransform.anchorMax = new Vector2(1, 1 - alignment);
				RectTransform.pivot     = new Vector2(0.5f, 1 - alignment);
				Resize(new Vector2(0, totalSize), _Instant);
				break;
			default:
				return;
		}
	}

	void CollectContainers()
	{
		for (int i = m_Containers.Count - 1; i >= 0; i--)
		{
			ElementContainer container = m_Containers[i];
			
			if (container == null || container.transform.parent != transform || container.IsDestroying() || container.IsDestroyed())
				m_Containers.RemoveAt(i);
		}
		
		HashSet<ElementContainer> containers = new HashSet<ElementContainer>(m_Containers);
		
		int axis;
		switch (m_Direction)
		{
			case Direction.Horizontal:
				axis = 0;
				break;
			case Direction.Vertical:
				axis = 1;
				break;
			default:
				return;
		}
		
		for (int i = 0; i < transform.childCount; i++)
		{
			ElementContainer container = transform.GetChild(i).GetComponent<ElementContainer>();;
			
			if (container == null || container.IsDestroying() || container.IsDestroyed())
				continue;
			
			container.Setup(this, axis);
			
			if (containers.Contains(container))
				continue;
			
			m_Containers.Add(container);
		}
	}

	void Resize(Vector2 _Size, bool _Instant = false)
	{
		if (m_ResizeRoutine != null)
			StopCoroutine(m_ResizeRoutine);
		m_ResizeRoutine = null;
		
		if (!_Instant && Application.isPlaying && gameObject.activeInHierarchy)
			StartCoroutine(m_ResizeRoutine = ResizeRoutine(_Size));
		else
			RectTransform.sizeDelta = _Size;
	}

	IEnumerator ResizeRoutine(Vector2 _Size)
	{
		Vector2 source = RectTransform.sizeDelta;
		Vector2 target = _Size;
		
		if (source == target)
			yield break;
		
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / m_Duration);
			
			RectTransform.sizeDelta = Vector2.Lerp(source, target, phase);
		}
		
		RectTransform.sizeDelta = target;
	}
}
