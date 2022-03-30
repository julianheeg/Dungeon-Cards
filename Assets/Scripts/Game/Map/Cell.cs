using System;
using UnityEngine;

/// <summary>
/// a struct that represents a position on the board
/// </summary>
public struct Cell
{
    public readonly int x, y;
    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="data">the data to deserialize</param>
    /// <param name="index">the index at which to start deserializing. This will be incremented to the next position</param>
    public Cell(byte[] data, ref int index)
    {
        this.x = BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index);
        index += 4;
        this.y = BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index);
        index += 4;
    }

    /// <summary>
    /// an array of the corner positions relative to the center (7 for easier Triangulation)
    /// </summary>
    private readonly static Vector3[] corners =
    {
        new Vector3(    1/Mathf.Sqrt(3),            0f,     0f)     * Config.scale,
        new Vector3(    0.5f * 1/Mathf.Sqrt(3),     0f,     -0.5f)  * Config.scale,
        new Vector3(    -0.5f * 1/Mathf.Sqrt(3),    0f,     -0.5f)  * Config.scale,
        new Vector3(    -1/Mathf.Sqrt(3),           0f,     0f)     * Config.scale,
        new Vector3(    -0.5f * 1/Mathf.Sqrt(3),    0f,     0.5f)   * Config.scale,
        new Vector3(    0.5f * 1/Mathf.Sqrt(3),     0f,     0.5f)   * Config.scale,
        new Vector3(    1/Mathf.Sqrt(3),            0f,     0f)     * Config.scale
    };

    /// <summary>
    /// the center of this cell
    /// </summary>
    public Vector3 Center
    {
        get
        {
            //return new Vector3(-x, 0, y * Mathf.Sqrt(3)) * Config.Scale;
            return new Vector3(-x * Mathf.Sqrt(3) * 0.5f, 0, y - x * 0.5f) * Config.scale;
        }
    }

    /// <summary>
    /// a method for Triangulation. Gets the left corner of a triangle when looking from the center in the given direction
    /// </summary>
    /// <param name="direction">the direction to look in</param>
    /// <returns>the left corner of the triangle in that direction as seen from the center</returns>
    public Vector3 GetFirstCorner(HexDirection direction)
    {
        return Center + corners[(int)direction];
    }

    /// <summary>
    /// a method for Triangulation. Gets the right corner of a triangle when looking from the center in the given direction
    /// </summary>
    /// <param name="direction">the direction to look in</param>
    /// <returns>the right corner of the triangle in that direction as seen from the center</returns>
    public Vector3 GetSecondCorner(HexDirection direction)
    {
        return Center + corners[(int)direction + 1];
    }


    /// <summary>
    /// -1 for x and y refers to a position that is irrelevant respectively not known
    /// </summary>
    public bool IsNotSet { get { return this == UnSetGridPosition; } }
    public static Cell UnSetGridPosition = new Cell(-1, -1);

    #region Operators & Overrides

    public static Cell operator+(Cell left, Cell right)
    {
        return new Cell(left.x + right.x, left.y + right.y);
    }

    public static bool operator==(Cell left, Cell right)
    {
        // If both are null, or both are same instance, return true.
        if (object.ReferenceEquals(left, right))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
        {
            return false;
        }

        return (left.x == right.x) && (left.y == right.y);
    }

    public static bool operator !=(Cell left, Cell right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    #endregion

    /// <summary>
    /// serializes this struct for transmission over the network
    /// </summary>
    /// <returns></returns>
    public byte[] Serialize()
    {
        byte[] data = new byte[8];
        Array.Copy(Endianness.ToBigEndian(BitConverter.GetBytes(x)), 0, data, 0, 4);
        Array.Copy(Endianness.ToBigEndian(BitConverter.GetBytes(y)), 0, data, 4, 4);
        return data;
    }

    public override string ToString()
    {
        return "[" + x + "," + y + "]";
    }

    /// <summary>
    /// checks whether the given cell and this cell are adjacent (independent of whether or not one is traversible or not or whatever else)
    /// </summary>
    /// <param name="presumedNeighbor">the adjacency to check</param>
    /// <returns>true iff this cell and the presumed neighbor are actually adjacent</returns>
    public bool IsAdjacent(Cell presumedNeighbor)
    {
        foreach(HexDirection direction in Enum.GetValues(typeof(HexDirection)))
        {
            if(this + direction.ToCell() == presumedNeighbor)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// calculates the distance between this and another cell (as defined by the hexagonal analogon of the taxicab metric)
    /// </summary>
    /// <param name="other">the other cell</param>
    /// <returns>the hexagonal taxicab distance</returns>
    public int Distance(Cell other)
    {
        int distance = (Math.Abs(this.x - other.x) + Math.Abs(this.x - this.y - other.x + other.y) + Math.Abs(this.y - other.y)) / 2;
        return distance;
    }
}