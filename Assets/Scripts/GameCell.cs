using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class GameCell : MonoBehaviour, IEquatable<GameCell>
{
	public int ID
	{
		get { return m_ID; }
	}

	public Level Level
	{
		get { return m_Level; }
	}

	public Vector3Int Position
	{
		get { return m_Position; }
	}

	protected Animator Animator
	{
		get
		{
			if (m_Animator == null)
				m_Animator = GetComponent<Animator>();
			return m_Animator;
		}
	}

	[SerializeField, HideInInspector] int        m_ID       = default;
	[SerializeField, HideInInspector] Level      m_Level    = default;
	[SerializeField, HideInInspector] Vector3Int m_Position = default;

	Animator m_Animator;

	#if UNITY_EDITOR
	protected virtual void OnValidate()
	{
		if (Application.isPlaying)
			return;
		
		int instanceID = GetInstanceID();
		
		UnityEditor.EditorApplication.delayCall += () =>
		{
			GameCell cell = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as GameCell;
			
			if (cell == null)
				return;
			
			if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(cell) && cell.m_ID != instanceID)
				m_ID = GetInstanceID();
			
			if (UnityEditor.PrefabUtility.IsPartOfVariantPrefab(cell) && cell.m_ID != instanceID)
				m_ID = GetInstanceID();
		};
	}
	#endif

	public virtual void Setup(Level _Level, Vector3Int _Position)
	{
		m_Level    = _Level;
		m_Position = _Position;
	}

	public virtual void Remove()
	{
		Destroy(gameObject);
	}

	public virtual void Show(Action _Finished = null)
	{
		if (_Finished != null)
			_Finished();
	}

	public virtual void Hide(Action _Finished = null)
	{
		if (_Finished != null)
			_Finished();
	}

	public virtual void Restore(Action _Finished = null)
	{
		if (_Finished != null)
			_Finished();
	}

	public abstract void Sample(Action _Finished = null);

	public bool Equals(GameCell _Cell)
	{
		if (ReferenceEquals(null, _Cell))
			return false;
		if (ReferenceEquals(this, _Cell))
			return true;
		if (m_ID == _Cell.ID)
			return true;
		return base.Equals(_Cell);
	}

	public override bool Equals(object _Object)
	{
		if (ReferenceEquals(null, _Object))
			return false;
		if (ReferenceEquals(this, _Object))
			return true;
		if (_Object.GetType() != GetType())
			return false;
		return Equals((GameCell)_Object);
	}

	public override int GetHashCode()
	{
		return unchecked ((base.GetHashCode() * 397) ^ ID);
	}

	protected void InvokeCallback(ref Action _Action)
	{
		Action action = _Action;
		_Action = null;
		if (action != null)
			action();
	}
}