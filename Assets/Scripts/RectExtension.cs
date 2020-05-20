using UnityEngine;

public static class RectExtension
{
	public static Rect Fit(this Rect _Rect, float _Aspect, Vector2 _Anchor)
	{
		Vector2 hFit  = new Vector2(_Rect.width, _Rect.width / _Aspect);
		Vector2 vFit  = new Vector2(_Rect.height * _Aspect, _Rect.height);
		float   hArea = hFit.x * hFit.y;
		float   vArea = vFit.x * hFit.y;
		Vector2 size  = hArea < vArea ? hFit : vFit;
		return new Rect(
			_Rect.x + (_Rect.width - size.x) * _Anchor.x,
			_Rect.y + (_Rect.height - size.y) * _Anchor.y,
			size.x,
			size.y
		);
	}

	public static Rect Fill(this Rect _Rect, float _Aspect, Vector2 _Anchor)
	{
		Vector2 hFit  = new Vector2(_Rect.width, _Rect.width / _Aspect);
		Vector2 vFit  = new Vector2(_Rect.height * _Aspect, _Rect.height);
		float   hArea = hFit.x * hFit.y;
		float   vArea = vFit.x * hFit.y;
		Vector2 size  = hArea > vArea ? hFit : vFit;
		return new Rect(
			_Rect.x + (_Rect.width - size.x) * _Anchor.x,
			_Rect.y + (_Rect.height - size.y) * _Anchor.y,
			size.x,
			size.y
		);
	}
}