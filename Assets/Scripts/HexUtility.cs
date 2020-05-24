using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexUtility
{
	public struct HexNeighborEnumerator : IEnumerator<Vector3Int>
	{
		public Vector3Int Current => GetNeighborPosition(m_Position, m_Current);

		object IEnumerator.Current => Current;

		readonly Vector3Int m_Position;

		int m_Current;

		public HexNeighborEnumerator(Vector3Int _Position)
		{
			m_Position = _Position;
			m_Current  = -1;
		}

		public bool MoveNext()
		{
			m_Current++;
			return m_Current < NeighborsCount;
		}

		public void Reset()
		{
			m_Current = 0;
		}

		public HexNeighborEnumerator GetEnumerator()
		{
			return this;
		}

		void IDisposable.Dispose() { }
	}

	public static readonly int NeighborsCount = 6;

	static readonly Vector3Int[][] m_Neighbors =
	{
		new Vector3Int[] 
		{
			new Vector3Int(1, 0, 0),
			new Vector3Int(0, -1, 0),
			new Vector3Int(-1, -1, 0),
			new Vector3Int(-1, 0, 0),
			new Vector3Int(-1, 1, 0),
			new Vector3Int(0, 1, 0),
		},
		new Vector3Int[]
		{
			new Vector3Int(1, 0, 0),
			new Vector3Int(1, -1, 0),
			new Vector3Int(0, -1, 0),
			new Vector3Int(-1, 0, 0),
			new Vector3Int(0, 1, 0),
			new Vector3Int(1, 1, 0),
		}
	};

	public static Vector3Int GetNeighborPosition(Vector3Int _Position, int _Direction)
	{
		return _Position + m_Neighbors[_Position.y & 1][_Direction % 6];
	}

	public static HexNeighborEnumerator GetNeighborPositions(Vector3Int _Position)
	{
		return new HexNeighborEnumerator(_Position);
	}
}