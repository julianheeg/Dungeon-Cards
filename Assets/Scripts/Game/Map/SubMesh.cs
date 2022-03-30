using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a script that manages triangulation of a certain part of the map
/// </summary>
public class SubMesh : MeshSuperClass {

    int meshXStart, meshYStart;

    protected override void Awake()
    {
        base.Awake();
        mesh.name = "SubMesh";
    }

    /// <summary>
    /// sets the index of this mesh within the meshHolder class
    /// internally this function converts the index to absolute positions
    /// </summary>
    /// <param name="x">the x index</param>
    /// <param name="y">the y index</param>
    public void SetMeshIndex(int x, int y)
    {
        meshXStart = x * Config.cellsPerSubmeshAndDirection;
        meshYStart = y * Config.cellsPerSubmeshAndDirection;
    }

    protected override void TriangulateCells()
    {
        //loop over all cells that this mesh is responsible for
        for (int i = meshXStart; i < meshXStart + Config.cellsPerSubmeshAndDirection; i++)
        {
            for (int j = meshYStart; j < meshYStart + Config.cellsPerSubmeshAndDirection; j++)
            {
                //check if in range
                if (i < MapManager.Length && j < MapManager.Width && MapManager.GetCell(new Cell(i, j)) != MapManager.CellType.Out_Of_Bounds)
                {
                    TriangulateCell(new Cell(i, j));
                }
            }
        }
    }

    protected override Color GetColor(Cell cell)
    {
        return CellTypeExtension.GetColor(MapManager.GetCell(cell));
    }
}