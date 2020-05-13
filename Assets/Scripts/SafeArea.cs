using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeArea : UIBehaviour
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

	[SerializeField] bool m_Left           = false;
	[SerializeField] bool m_Right          = false;
	[SerializeField] bool m_Top            = false;
	[SerializeField] bool m_Bottom         = false;
	[SerializeField] bool m_SyncHorizontal = false;
	[SerializeField] bool m_SyncVertical   = false;

	RectTransform m_RectTransform;
	Rect          m_SafeArea;

	protected override void Awake()
	{
		base.Awake();
		
		ProcessSafeArea();
	}

	void Update()
	{
		ProcessSafeArea();
	}

	void ProcessSafeArea()
	{
		if (m_SafeArea == Screen.safeArea || !m_Left && !m_Right && !m_Top && !m_Bottom)
			return;
		
		m_SafeArea = Screen.safeArea;
		
		Rect safeArea = m_SafeArea;
		
		if (m_SyncHorizontal)
		{
			float horizontal = Mathf.Max(safeArea.xMin, Screen.width - safeArea.xMax);
			safeArea.xMin = horizontal;
			safeArea.xMax = Screen.width - horizontal;
		}
		
		if (m_SyncVertical)
		{
			float vertical = Mathf.Max(safeArea.yMin, Screen.height - safeArea.yMax);
			safeArea.yMin = vertical;
			safeArea.yMax = Screen.height - vertical;
		}
		
		Vector2 anchorMin = RectTransform.anchorMin;
		Vector2 anchorMax = RectTransform.anchorMax;
		
		if (m_Left)
			anchorMin.x = safeArea.xMin / Screen.width;
		
		if (m_Right)
			anchorMax.x = safeArea.xMax / Screen.width;
		
		if (m_Top)
			anchorMax.y = safeArea.yMax / Screen.height;
		
		if (m_Bottom)
			anchorMin.y = safeArea.yMin / Screen.height;
		
		RectTransform.anchorMin = anchorMin;
		RectTransform.anchorMax = anchorMax;
	}
}