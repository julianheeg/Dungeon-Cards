using System;
using System.Collections.Generic;

/// <summary>
/// a class that represents a path on the map, i. e. a list of adjacent cells from a start to a destination
/// </summary>
public class Path {
    public readonly List<Cell> cells;

    public int Length   { get { return cells.Count - 1; } } //the starting cell does not count towards the length
    public Cell Start   { get { return cells[0]; } }
    public Cell End     { get { return cells[Length]; } }
    public Cell BeforeEnd { get { return Length >= 1 ? cells[Length - 1] : Cell.UnSetGridPosition; } }

    /// <summary>
    /// copy constructor
    /// </summary>
    /// <param name="path">the other path to copy</param>
    public Path(Path path)
    {
        cells = new List<Cell>(path.cells);
    }

    /// <summary>
    /// constructor that makes a path consisting of a list of cells
    /// </summary>
    /// <param name="cells">the list of cells</param>
    public Path(List<Cell> cells)
    {
        this.cells = cells;
    }

    /// <summary>
    /// constructor that makes a path consisting of only a starting cell
    /// </summary>
    /// <param name="start">the starting cell</param>
    public Path(Cell start)
    {
        cells = new List<Cell> { start };
    }

    /// <summary>
    /// adds a cell to this path
    /// </summary>
    /// <param name="cell">the cell to add</param>
    public void Add(Cell cell)
    {
        cells.Add(cell);
    }

    /// <summary>
    /// removes the last cell of this path
    /// </summary>
    public void RemoveLast()
    {
        cells.RemoveAt(Length);
    }

    /// <summary>
    /// checks whether the path contains a given cell
    /// </summary>
    /// <param name="cell">the cell to check for</param>
    /// <returns>true iff the cell is on this path</returns>
    public bool Contains(Cell cell)
    {
        return cells.Contains(cell);
    }

    /// <summary>
    /// calculates the Hausdorff distance from the set of cells of this path to the set of cells of another path.
    /// </summary>
    /// <param name="other">the other path</param>
    /// <returns>the Hausdorff distance from this path to the other path</returns>
    public int HausdorffDistanceTo(Path other)
    {
        int hausdorffDistance = 0;
        foreach(Cell cellOfThis in this.cells)
        {
            int distanceFromCell = int.MaxValue;
            foreach(Cell cellofOther in other.cells)
            {
                if(distanceFromCell > cellOfThis.Distance(cellofOther))
                {
                    distanceFromCell = cellOfThis.Distance(cellofOther);
                }
            }

            if(hausdorffDistance < distanceFromCell)
            {
                hausdorffDistance = distanceFromCell;
            }
        }

        return hausdorffDistance;
    }

    /// <summary>
    /// serializes this path into an array of bytes, ready to be sent over the network
    /// </summary>
    /// <returns></returns>
    public byte[] Serialize()
    {
        byte[] result = new byte[Length * 8 + 4];
        Array.Copy(Endianness.ToBigEndian(BitConverter.GetBytes(Length)), result, 4);
        for (int i = 1; i <= Length; i++)
        {
            Array.Copy(cells[i].Serialize(), 0, result, i * 8 - 4, 8);
        }
        return result;
    }
}
