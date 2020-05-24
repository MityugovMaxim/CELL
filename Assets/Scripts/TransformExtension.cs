using UnityEngine;

public static class TransformExtension
{
	public static Bounds TransformBounds(this Transform _Transform, Bounds _Bounds)
	{
		return new Bounds(
			_Transform.TransformPoint(_Bounds.center),
			_Transform.TransformVector(_Bounds.size)
		);
	}

	public static Bounds InverseTransformBounds(this Transform _Transform, Bounds _Bounds)
	{
		return new Bounds(
			_Transform.InverseTransformPoint(_Bounds.center),
			_Transform.InverseTransformVector(_Bounds.size)
		);
	}

	public static Rect TransformRect(this Transform _Transform, Rect _Rect)
	{
		return new Rect(
			_Transform.TransformPoint(_Rect.position),
			_Transform.TransformVector(_Rect.size)
		);
	}

	public static Rect InverseTransformRect(this Transform _Transform, Rect _Rect)
	{
		return new Rect(
			_Transform.InverseTransformPoint(_Rect.position),
			_Transform.InverseTransformVector(_Rect.size)
		);
	}
}