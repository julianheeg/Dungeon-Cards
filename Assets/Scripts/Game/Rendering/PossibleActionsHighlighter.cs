using System;
using System.Collections.Generic;
using UnityEngine;
using Type = Assets.Scripts.Game.Cards.CardTypes.Type;

/// <summary>
/// a script that hightlights possible actions when the player for example mouses over a card. This is done by overlaying a colored mesh on the map 
/// </summary>
class PossibleActionsHighlighter : MeshSuperClass
{
    static PossibleActionsHighlighter instance;
    static Cell[] ValidLocations { get; set; }
    static readonly Dictionary<Cell, Color> celltoColorMap = new Dictionary<Cell, Color>();

    /// <summary>
    /// initialize
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        mesh.name = "ActionsHighlightingMesh";
        instance = this;
    }

    /// <summary>
    /// highlights all possible cells where a given card can be dropped to
    /// </summary>
    /// <param name="card">the card to drop</param>
    public static void HighlightValidDropTiles(CardFace card)
    {
        RemoveHighlighting();

        switch (card.Type)
        {
            case Type.Monster:
                //TODO perhaps monster cards can be dragged from the map to another point on the map (for an effect for example). check for this and call another function
                ValidLocations = MapManager.Neighbors(MapManager.GetPlayerPosition(Player.ownIndex));
                instance.Triangulate();

                break;
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// highlights tiles where a given monster can move to
    /// </summary>
    /// <param name="monster">the monster to move</param>
    public static void HighlightValidMovementTiles(Monster monster)
    {
        RemoveHighlighting();
        ValidLocations = MapManager.FloodFill(monster.position, monster.MovementRange);
        instance.Triangulate();
    }

    /// <summary>
    /// removes the highlighting
    /// </summary>
    public static void RemoveHighlighting()
    {
        ValidLocations = null;
        celltoColorMap.Clear();
        instance.Triangulate();
    }

    protected override Color GetColor(Cell cell)
    {
        Color color;
        if (celltoColorMap.TryGetValue(cell, out color))
        {
            return color;
        }
        else
        {
            return Color.blue;
        }
    }

    protected override void TriangulateCells()
    {
        //loop over all cells that this mesh is responsible for
        if (ValidLocations != null)
        {
            foreach (Cell cell in ValidLocations)
            {
                TriangulateCell(cell);
            }
        }
    }

    /// <summary>
    /// checks whether a given location is in the array of valid locations
    /// </summary>
    /// <param name="location">the lovation to check</param>
    /// <returns>true iff the location is a valid location</returns>
    public static bool IsValidLocation(Cell location)
    {
        return Array.Exists(ValidLocations, validLocation => validLocation == location);
    }

    /// <summary>
    /// sets a color for a given cell and triangulates the mesh again
    /// </summary>
    /// <param name="cell">the cell</param>
    /// <param name="color">the color</param>
    public static void SetColor(Cell cell, Color color)
    {
        if (IsValidLocation(cell))
        {
            celltoColorMap[cell] = color;
        }
        instance.Triangulate();
    }

    /// <summary>
    /// sets a color for an entire path and triangulates the mesh again
    /// </summary>
    /// <param name="path">the path</param>
    /// <param name="color">the color</param>
    public static void SetColor(Path path, Color color)
    {
        foreach(Cell cell in path.cells)
        {
            if (IsValidLocation(cell))
            {
                celltoColorMap[cell] = color;
            }
        }
        instance.Triangulate();
    }

    /// <summary>
    /// resets the color of a given cell and triangulates the mesh again
    /// </summary>
    /// <param name="cell">the cell</param>
    public static void ResetColor(Cell cell)
    {
        celltoColorMap.Remove(cell);
        instance.Triangulate();
    }

    /// <summary>
    /// resets the color of an entire path and triangulates the mesh again
    /// </summary>
    /// <param name="path">the path</param>
    public static void ResetColor(Path path)
    {
        foreach (Cell cell in path.cells)
        {
            celltoColorMap.Remove(cell);
        }
        instance.Triangulate();
    }
}
