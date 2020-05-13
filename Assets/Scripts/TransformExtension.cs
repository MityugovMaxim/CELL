using UnityEngine;

public static class TransformExtension
{
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