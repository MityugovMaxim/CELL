using UnityEngine;

public static class GameObjectExtension
{
	public static void SetLayer(this GameObject _GameObject, int _Layer)
	{
		foreach (Transform child in _GameObject.transform)
		{
			child.gameObject.layer = _Layer;
			child.gameObject.SetLayer(_Layer);
		}
	}
}