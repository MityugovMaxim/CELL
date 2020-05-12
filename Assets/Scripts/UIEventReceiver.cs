using UnityEngine;
using UnityEngine.UI;

public class UIEventReceiver : Graphic
{
	protected override void OnPopulateMesh(VertexHelper _VertexHelper)
	{
		_VertexHelper.Clear();
	}

	protected Vector2 GetPosition(Vector2 _Position)
	{
		return transform.InverseTransformVector(_Position);
	}
}