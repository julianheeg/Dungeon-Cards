using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// an abstract script class that provides some basic mesh functionality to child classes
/// </summary>
[RequireComponent(typeof(MeshFilter), (typeof(MeshRenderer)))]
public abstract class MeshSuperClass : MonoBehaviour {
    protected Mesh mesh;
    protected List<Vector3> vertices;
    protected List<int> triangles;
    protected List<Color> colors;

    protected MeshCollider meshCollider;

    protected Dictionary<int, Cell> triangleDictionary;

    /// <summary>
    /// initialization
    /// </summary>
    protected virtual void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
        triangleDictionary = new Dictionary<int, Cell>();
    }

    /// <summary>
    /// recalculates everything in this mesh
    /// </summary>
    public void Triangulate()
    {
        ClearLists();
        TriangulateCells();
        SetMesh();
    }

    /// <summary>
    /// clears all the lists so that they can be repopulated and reassigned to the mesh
    /// </summary>
    private void ClearLists()
    {
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        triangleDictionary.Clear();
    }

    /// <summary>
    /// loop over the cells that this mesh is responsible for and triangulate them
    /// </summary>
    protected abstract void TriangulateCells();

    /// <summary>
    /// reassigns the lists to the mesh so that they are displayed
    /// </summary>
    private void SetMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// triangulates an individual cell
    /// </summary>
    /// <param name="cell">the cell to triangulate</param>
    protected void TriangulateCell(Cell cell)
    {
        float elevation = MapManager.GetCell(cell) == MapManager.CellType.Traversible ? 0f : Config.wallHeight;
        Vector3 elevationVector = new Vector3(0, elevation, 0);
        Color color = GetColor(cell);

        int currentIndex = vertices.Count;

        foreach (HexDirection direction in Enum.GetValues(typeof(HexDirection)))
        {
            vertices.Add(cell.Center + elevationVector);
            vertices.Add(cell.GetFirstCorner(direction) + elevationVector);
            vertices.Add(cell.GetSecondCorner(direction) + elevationVector);

            triangles.Add(currentIndex);
            triangles.Add(currentIndex + 1);
            triangles.Add(currentIndex + 2);
            triangleDictionary.Add(currentIndex / 3, cell); //don't forget to add the triangle to the dictionary            
            currentIndex += 3;

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
        }
    }

    /// <summary>
    /// gets the color that a given cell should have
    /// </summary>
    /// <param name="cell">the cell of which the color to get</param>
    /// <returns>the color that the cell has</returns>
    protected abstract Color GetColor(Cell cell);

    /// <summary>
    /// turns a RaycastHit.triangleIndex into the cell it corresponds to
    /// </summary>
    /// <param name="index">the triangle's index</param>
    /// <returns>the corresponding cell or null, if no cell corresponds to the triangle</returns>
    public Cell? TriangleIndexToCell(int index)
    {
        Cell cell;
        triangleDictionary.TryGetValue(index, out cell);
        return cell;
    }
}