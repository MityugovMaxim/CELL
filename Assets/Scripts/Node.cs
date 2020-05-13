using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
[CreateAssetMenu(fileName = "Node", menuName = "Map/Node")]
public class Node : TileBase
{
	[SerializeField] Sprite            m_Sprite       = default;
	[SerializeField] Color             m_Color        = Color.white;
	[SerializeField] Matrix4x4         m_Transform    = Matrix4x4.identity;
	[SerializeField] Tile.ColliderType m_ColliderType = Tile.ColliderType.Sprite;
	[SerializeField] TileFlags         m_Flags        = TileFlags.LockColor;

	public override void GetTileData(Vector3Int _Position, ITilemap _Tilemap, ref TileData _TileData)
	{
		_TileData.sprite       = m_Sprite;
		_TileData.color        = m_Color;
		_TileData.transform    = m_Transform;
		_TileData.colliderType = m_ColliderType;
		_TileData.flags        = m_Flags;
	}
}