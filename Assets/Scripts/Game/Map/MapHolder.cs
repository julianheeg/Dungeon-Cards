using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// a script for a game object that holds the submeshes that make up the entire map
/// </summary>
public class MapHolder : MonoBehaviour {

    [SerializeField]
    GameObject meshPrefab;
    SubMesh[,] subMeshes;

    /// <summary>
    /// initialize and instantiate all the meshes
    /// </summary>
    private void Awake()
    {
        int horizontalMeshes = (MapManager.Length + Config.cellsPerSubmeshAndDirection - 1) / Config.cellsPerSubmeshAndDirection;
        int verticalMeshes = (MapManager.Width + Config.cellsPerSubmeshAndDirection - 1) / Config.cellsPerSubmeshAndDirection;
        subMeshes = new SubMesh[horizontalMeshes, verticalMeshes];

        InstantiateMeshes();

        //listen for altered cells
        MapManager.CellSet += OnCellSet;
    }

    /// <summary>
    /// inform the server that the level has been loaded. It is here because it doesn't matter which object's start function it uses.
    /// </summary>
    private void Start()
    {
        Messages.SendLevelLoadedMessage();
    }

    /// <summary>
    /// instantiates the submeshes
    /// </summary>
    private void InstantiateMeshes()
    {
        for (int i = 0; i < subMeshes.GetLength(0); i++)
        {
            for (int j = 0; j < subMeshes.GetLength(1); j++)
            {
                GameObject mesh = Instantiate(meshPrefab) as GameObject;
                mesh.transform.SetParent(this.transform);
                subMeshes[i, j] = mesh.GetComponent<SubMesh>();
                subMeshes[i, j].SetMeshIndex(i, j);
                subMeshes[i, j].Triangulate();
            }
        }
    }

    /// <summary>
    /// when a cell is altered, triangulate the corresponding mesh again
    /// </summary>
    /// <param name="args"></param>
    private void OnCellSet(object sender, MapManager.CellEventArgs args)
    {
        Cell position = args.Cell;
        subMeshes[position.x / Config.cellsPerSubmeshAndDirection, position.y / Config.cellsPerSubmeshAndDirection].Triangulate();
    }

    /// <summary>
    /// unsubscribe from cell set when destroyed
    /// </summary>
    private void OnDestroy()
    {
        MapManager.CellSet -= OnCellSet;
    }
}