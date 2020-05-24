using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public abstract class GameCell : MonoBehaviour, IEquatable<GameCell>
{
	public int ID => m_ID;

	protected GameStage Stage { get; private set; }

	protected GameLayerType LayerType { get; private set; }

	protected Vector3Int Position { get; private set; }

	protected Animator Animator
	{
		get
		{
			if (m_Animator == null)
				m_Animator = GetComponent<Animator>();
			return m_Animator;
		}
	}

	[SerializeField] int m_ID = default;

	Animator m_Animator;

	public virtual void Setup(GameStage _Stage, GameLayerType _LayerType, Vector3Int _Position)
	{
		if (_Stage == null)
		{
			Debug.LogError("[GameCell] Setup cell failed. Level not found.");
			return;
		}
		
		Stage     = _Stage;
		LayerType = _LayerType;
		Position  = _Position;
		
		transform.position = Stage.GetWorldPosition(Position, LayerType);
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

	public static bool operator ==(GameCell _A, GameCell _B)
	{
		if (ReferenceEquals(_A, _B))
			return true;
		if (ReferenceEquals(_A, null))
			return false;
		if (ReferenceEquals(_B, null))
			return false;
		return _A.ID == _B.ID;
	}

	public static bool operator !=(GameCell _A, GameCell _B)
	{
		return !(_A == _B);
	}

	public bool Equals(GameCell _Cell)
	{
		if (ReferenceEquals(null, _Cell))
			return false;
		if (ReferenceEquals(this, _Cell))
			return true;
		if (ID == _Cell.ID)
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