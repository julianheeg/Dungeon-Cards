using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// an enumeration of the six directions
/// </summary>
public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

/// <summary>
/// an extension class for the directions
/// </summary>
public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }

    /// <summary>
    /// turns a direction into a cell where the cell is the neighbor of [0,0] in that direction. Example useage:
    ///     Cell leftOfCell = <cell> + Hexdirection.W.ToCell();
    /// </summary>
    /// <param name="direction">the direction</param>
    /// <returns>the neighboring cell in the direction from the [0,0] cell</returns>
    public static Cell ToCell(this HexDirection direction)
    {
        switch (direction)
        {
            case HexDirection.W:
                return new Cell(0, -1);
            case HexDirection.NW:
                return new Cell(1, 0);
            case HexDirection.NE:
                return new Cell(1, 1);
            case HexDirection.E:
                return new Cell(0, 1);
            case HexDirection.SE:
                return new Cell(-1, 0);
            case HexDirection.SW:
                return new Cell(-1, -1);
            default:
                throw new NotImplementedException();
        }
    }
}