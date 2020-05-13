using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
	[SerializeField] Tilemap  m_Nodes         = default;
	[SerializeField] Tilemap  m_Highlight     = default;
	[SerializeField] TileBase m_HighlightTile = default;

	public bool Contains(Vector3 _Position)
	{
		Vector3Int position = m_Nodes.WorldToCell(_Position);
		
		return m_Nodes.HasTile(position);
	}

	public void Add(Vector3 _Position, Node _Node)
	{
		Vector3Int position = m_Nodes.WorldToCell(_Position);
		
		m_Nodes.SetTile(position, _Node);
	}

	public void Remove(Vector3 _Position)
	{
		Vector3Int position = m_Nodes.WorldToCell(_Position);
		
		m_Nodes.SetTile(position, null);
	}

	public void Highlight(Vector3 _Position)
	{
		Vector3Int position = m_Nodes.WorldToCell(_Position);
		
		Color color = m_Nodes.HasTile(position)
			? Color.red
			: Color.green;
		
		m_Highlight.ClearAllTiles();
		m_Highlight.SetTile(position, m_HighlightTile);
		m_Highlight.SetColor(position, color);
	}

	public void ClearHighlight()
	{
		m_Highlight.ClearAllTiles();
	}
}