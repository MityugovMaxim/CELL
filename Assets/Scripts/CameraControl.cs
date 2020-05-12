using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class CameraControl : UIEventReceiver, IScrollHandler, IPointerDownHandler, IDragHandler, IEndDragHandler
{
	public Rect Limit
	{
		get { return m_Limit; }
		set
		{
			if (m_Limit == value)
				return;
			
			m_Limit = value;
			
			ProcessLimit();
		}
	}

	Vector3 Position
	{
		get { return m_Camera.transform.position; }
		set
		{
			if (m_Camera.transform.position == value)
				return;
			
			m_Position                  = value;
			m_Camera.transform.position = value;
			
			ProcessLimit();
		}
	}

	float Size
	{
		get { return m_Camera.orthographicSize; }
		set
		{
			if (Mathf.Approximately(m_Camera.orthographicSize, value))
				return;
			
			m_Camera.orthographicSize = value;
			
			ProcessLimit();
		}
	}

	[SerializeField] Camera         m_Camera;
	[SerializeField] float          m_MinSize;
	[SerializeField] float          m_MaxSize;
	[SerializeField] float          m_MomentumDuration;
	[SerializeField] AnimationCurve m_MomentumCurve;
	[SerializeField] Rect           m_Limit;
	[SerializeField] float          m_SafeZone;

	Vector3 m_Position;
	IEnumerator m_MomentumRoutine;

	void Update()
	{
		if (m_Position != Position)
		{
			m_Position = Position;
			
			ProcessLimit();
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		bool draw = false;
		
		if (UnityEditor.Selection.Contains(gameObject))
			draw = true;
		
		if (m_Camera != null && UnityEditor.Selection.Contains(m_Camera.gameObject))
			draw = true;
		
		if (!draw)
			return;
		
		Vector3 padding = m_Camera.ScreenToWorldPoint(new Vector2(0, 0)) - Position;
		
		float width  = Mathf.Max(0, Limit.width + padding.x * 2 * m_SafeZone);
		float height = Mathf.Max(0, Limit.height + padding.y * 2 * m_SafeZone);
		
		Rect limit = new Rect(
			Limit.center.x - width * 0.5f,
			Limit.center.y - height * 0.5f,
			width,
			height
		);
		
		Color unsafeAreaColor = new Color(1, 0, 0.5f, 0.05f);
		Color safeAreaColor   = new Color(0, 1, 0.5f, 0.05f);
		
		if (m_Limit.Contains(limit.min) || m_Limit.Contains(limit.max))
		{
			UnityEditor.Handles.DrawSolidRectangleWithOutline(
				Rect.MinMaxRect(Limit.xMin, Limit.yMin, limit.xMin, Limit.yMax),
				unsafeAreaColor,
				Color.clear
			);
			
			UnityEditor.Handles.DrawSolidRectangleWithOutline(
				Rect.MinMaxRect(limit.xMax, Limit.yMin, Limit.xMax, Limit.yMax),
				unsafeAreaColor,
				Color.clear
			);
			
			UnityEditor.Handles.DrawSolidRectangleWithOutline(
				Rect.MinMaxRect(limit.xMin, Limit.yMin, limit.xMax, limit.yMin),
				unsafeAreaColor,
				Color.clear
			);
			
			UnityEditor.Handles.DrawSolidRectangleWithOutline(
				Rect.MinMaxRect(limit.xMin, Limit.yMax, limit.xMax, limit.yMax),
				unsafeAreaColor,
				Color.clear
			);
			
			UnityEditor.Handles.DrawSolidRectangleWithOutline(
				Limit,
				Color.clear,
				Color.red
			);
		}
		
		UnityEditor.Handles.DrawSolidRectangleWithOutline(
			limit,
			safeAreaColor,
			Color.green
		);
	}
	#endif

	#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		
		if (gameObject.scene.isLoaded)
			ProcessLimit();
	}
	#endif

	public void OnScroll(PointerEventData _Data)
	{
		if (m_Camera == null)
			return;
		
		float scroll = _Data.scrollDelta.y;
		
		Vector2 position = _Data.position;
		
		Vector3 source = m_Camera.ScreenToWorldPoint(position);
		
		Size = Mathf.Clamp(Size - scroll, m_MinSize, m_MaxSize);
		
		Vector3 target = m_Camera.ScreenToWorldPoint(position);
		
		Vector3 delta = target - source;
		
		Position -= delta;
	}

	public void OnPointerDown(PointerEventData _Data)
	{
		if (m_MomentumRoutine != null)
			StopCoroutine(m_MomentumRoutine);
		m_MomentumRoutine = null;
	}

	public void OnDrag(PointerEventData _Data)
	{
		if (m_Camera == null)
			return;
		
		Vector3 source = m_Camera.ScreenToWorldPoint(_Data.position - _Data.delta);
		Vector3 target = m_Camera.ScreenToWorldPoint(_Data.position);
		Vector3 delta  = target - source;
		
		Position -= delta;
	}

	public void OnEndDrag(PointerEventData _Data)
	{
		if (m_Camera == null)
			return;
		
		Vector3 source = m_Camera.ScreenToWorldPoint(_Data.position - _Data.delta);
		Vector3 target = m_Camera.ScreenToWorldPoint(_Data.position);
		Vector3 delta = target - source;
		
		StartCoroutine(m_MomentumRoutine = MomentumRoutine(delta / Time.deltaTime));
	}

	IEnumerator MomentumRoutine(Vector3 _Speed)
	{
		if (m_Camera == null)
			yield break;
		
		float time = 0;
		while (time < m_MomentumDuration)
		{
			float phase = m_MomentumCurve.Evaluate(time / m_MomentumDuration);
			
			Vector3 speed = _Speed * phase;
			
			Position -= speed * Time.deltaTime;
			
			yield return null;
			
			time += Time.deltaTime;
		}
	}

	void ProcessLimit()
	{
		if (m_Limit == Rect.zero)
			return;
		
		Vector3 padding = m_Camera.ScreenToWorldPoint(new Vector2(0, 0)) - Position;
		
		float width  = Mathf.Max(0, Limit.width + padding.x * 2 * m_SafeZone);
		float height = Mathf.Max(0, Limit.height + padding.y * 2 * m_SafeZone);
		
		Rect limit = new Rect(
			Limit.center.x - width * 0.5f,
			Limit.center.y - height * 0.5f,
			width,
			height
		);
		
		Vector3 position = m_Camera.transform.position;
		position.x = Mathf.Clamp(Position.x, limit.xMin, limit.xMax);
		position.y = Mathf.Clamp(Position.y, limit.yMin, limit.yMax);
		m_Camera.transform.position = position;
	}
}
