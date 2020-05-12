using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class ElementPanel : UIBehaviour
{
	RectTransform RectTransform
	{
		get
		{
			if (m_RectTransform == null)
				m_RectTransform = GetComponent<RectTransform>();
			return m_RectTransform;
		}
	}

	[SerializeField] float          m_Size;
	[SerializeField] float          m_Spacing;
	[SerializeField] float          m_Duration;
	[SerializeField] AnimationCurve m_Curve;

	RectTransform m_RectTransform;

	[SerializeField] List<ElementContainer> m_Containers = new List<ElementContainer>();

	protected override void Awake()
	{
		base.Awake();
		
		CollectContainers();
		
		Reposition();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		
		CollectContainers();
		
		Reposition();
	}

	void OnTransformChildrenChanged()
	{
		CollectContainers();
		
		Reposition();
	}

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		UnityEditor.EditorApplication.delayCall += Reposition;
	}
	#endif

	void CollectContainers()
	{
		for (int i = m_Containers.Count - 1; i >= 0; i--)
		{
			ElementContainer container = m_Containers[i];
			
			if (container == null || container.transform.parent != transform)
				m_Containers.RemoveAt(i);
		}
		
		HashSet<ElementContainer> containers = new HashSet<ElementContainer>(m_Containers);
		
		for (int i = 0; i < transform.childCount; i++)
		{
			ElementContainer container = transform.GetChild(i).GetComponent<ElementContainer>();;
			
			container.Setup(this);
			
			if (containers.Contains(container) || container == null || !container.gameObject.activeSelf)
				continue;
			
			m_Containers.Add(container);
		}
	}

	public bool Replace(ElementContainer _Container)
	{
		if (_Container == null)
			return false;
		
		int position = m_Containers.IndexOf(_Container);
		
		if (position < 0)
			return false;
		
		m_Containers.RemoveAt(position);
		
		Rect source = _Container.rectTransform.rect;
		source = new Rect(
			_Container.transform.TransformPoint(source.position),
			_Container.transform.TransformVector(source.size)
		);
		
		float minDistance = float.MaxValue;
		int index = 0;
		for (int i = 0; i < m_Containers.Count; i++)
		{
			ElementContainer container = m_Containers[i];
			
			if (container == null)
				continue;
			
			Rect target = container.GetPixelAdjustedRect();
			target = new Rect(
				container.transform.TransformPoint(target.position),
				container.transform.TransformVector(target.size)
			);
			
			// Check left side
			float leftDistance = Mathf.Abs(source.center.x - target.xMin);
			if (minDistance > leftDistance)
			{
				minDistance = leftDistance;
				index       = i;
			}
			
			// Check right side
			float rightDistance = Mathf.Abs(source.center.x - target.xMax);
			if (minDistance > rightDistance)
			{
				minDistance = rightDistance;
				index       = i + 1;
			}
		}
		
		m_Containers.Insert(index, _Container);
		
		return position != index;
	}

	public void Move(ElementContainer _Container, int _Position)
	{
		if (_Container == null)
			return;
		
		_Position = Mathf.Clamp(_Position, 0, m_Containers.Count - 1);
		
		int position = m_Containers.IndexOf(_Container);
		
		if (position < 0 || position == _Position)
			return;
		
		m_Containers.RemoveAt(position);
		m_Containers.Insert(_Position, _Container);
	}

	public void Reposition()
	{
		if (IsDestroyed())
			return;
		
		StopAllCoroutines();
		
		int count = m_Containers.Count(_Container => !_Container.Drag);
		
		float totalSize = count * (m_Size + m_Spacing) - m_Spacing;
		
		float position = -totalSize * 0.5f;
		
		foreach (ElementContainer container in m_Containers)
		{
			if (container == null || container.IgnoreLayout)
				continue;
			
			RectTransform rectTransform = container.rectTransform;
			
			if (rectTransform == null)
				continue;
			
			rectTransform.anchorMin = new Vector2(0.5f, 0);
			rectTransform.anchorMax = new Vector2(0.5f, 1);
			rectTransform.pivot     = new Vector2(0.5f, 0.5f);
			rectTransform.sizeDelta = new Vector2(m_Size, 0);
			
			if (container.Drag)
				continue;
			
			SetPosition(rectTransform, new Vector2(position + m_Size * 0.5f, 0));
			
			position += m_Size + m_Spacing;
		}
		
		RectTransform.anchorMin = new Vector2(0.5f, 0);
		RectTransform.anchorMax = new Vector2(0.5f, 1);
		RectTransform.pivot     = new Vector2(0.5f, 0.5f);
		
		SetSize(RectTransform, new Vector2(totalSize, RectTransform.sizeDelta.y));
	}

	void SetSize(RectTransform _Target, Vector2 _Size)
	{
		if (Application.isPlaying && gameObject.activeInHierarchy)
			StartCoroutine(SizeRoutine(_Target, _Size));
		else if (_Target != null)
			_Target.sizeDelta = _Size;
	}

	void SetPosition(RectTransform _Target, Vector2 _Position)
	{
		if (Application.isPlaying && gameObject.activeInHierarchy)
			StartCoroutine(PositionRoutine(_Target, _Position));
		else if (_Target != null)
			_Target.anchoredPosition = _Position;
	}

	IEnumerator SizeRoutine(RectTransform _Target, Vector2 _Size)
	{
		Vector2 source = _Target.sizeDelta;
		Vector2 target = _Size;
		
		if (source == target)
			yield break;
		
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / m_Duration);
			
			_Target.sizeDelta = Vector2.Lerp(source, target, phase);
		}
		
		_Target.sizeDelta = target;
	}

	IEnumerator PositionRoutine(RectTransform _Target, Vector2 _Position)
	{
		Vector2 source = _Target.anchoredPosition;
		Vector2 target = _Position;
		
		if (source == target)
			yield break;
		
		float time = 0;
		while (time < m_Duration)
		{
			yield return null;
			
			time += Time.deltaTime;
			
			float phase = m_Curve.Evaluate(time / m_Duration);
			
			_Target.anchoredPosition = Vector2.Lerp(source, target, phase);
		}
		
		_Target.anchoredPosition = target;
	}
}
