using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a script that manages everything that happens to and with the map
/// </summary>
public class MapManager : MonoBehaviour {

    /// <summary>
    /// possible types that a cell might have
    /// </summary>
    public enum CellType { Out_Of_Bounds = 0, Uninitialized, Wall, Traversible, Player1, Player2, Player3, Player4, TempMark1, TempMark2, Monster };

    //static MapManager instance;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        //instance = this;
    }

    static public int Length { get; private set; }
    static public int Width { get; private set; }
    static public bool IsHexGrid { get; private set; }
    static CellType[,] map;
    static Cell[] playerPositions;

    /// <summary>
    /// initializes the map for the current game
    /// </summary>
    /// <param name="length"></param>
    /// <param name="width"></param>
    /// <param name="isHexGrid"></param>
    public static void Init(int length, int width, bool isHexGrid)
    {
        MapManager.IsHexGrid = isHexGrid;
        MapManager.Length = length;
        MapManager.Width = width;

        map = new CellType[length, width];
        playerPositions = new Cell[GameManager.NumberOfPlayers];

        InitializeShape();
    }

    /// <summary>
    /// separates the tiles that are within bounds from the tiles out of bounds (in case of hexagon map
    /// after calling this method, all tiles within bounds will have state "uninitialized"
    /// the tiles out of bounds will have state "out of bounds"
    /// </summary>
    private static void InitializeShape()
    {
        if (IsHexGrid)
        {
            for (int i = 0; i < Length; i++)
                for (int j = 0; j < Width; j++)
                    if (i - j <= Width / 2 && j - i <= Width / 2)
                    {
                        SetCell(new Cell(i, j), CellType.Uninitialized);
                    }
                    else
                    {
                        SetCell(new Cell(i, j), CellType.Out_Of_Bounds);
                    }
        }
        else
        {
            for (int i = 0; i < Length; i++)
                for (int j = 0; j < Width; j++)
                    SetCell(new Cell(i, j), CellType.Uninitialized);
        }
    }

    /// <summary>
    /// sets the tile at the specified grid position and calls the submesh's triangulation method
    /// </summary>
    /// <param name="position">the position at which the type should be set</param>
    /// <param name="type">the tile type that the tile should be set to</param>
    public static void SetCell(Cell position, CellType type)
    {
        map[position.x, position.y] = type;

        switch (type)
        {
            case CellType.Player1:
                playerPositions[0] = position;
                break;
            case CellType.Player2:
                playerPositions[1] = position;
                break;
            case CellType.Player3:
                playerPositions[2] = position;
                break;
            case CellType.Player4:
                playerPositions[3] = position;
                break;
            default:
                break;
        }

        //send update
        OnCellSet(position);

        //TODO edit the whole function so that it can take an array of changes and only updates at the end (BUT UPDATE EVERYTHING THAT IS EFFECTED)
    }

    /// <summary>
    /// returns the tile type at the specified position
    /// </summary>
    /// <param name="position">the position at which the tile type is requested</param>
    /// <returns>the tile type at that position</returns>
    public static CellType GetCell(Cell position)
    {
        return map[position.x, position.y];
    }

    /// <summary>
    /// returns all the traversible neighbors of a cell
    /// </summary>
    /// <param name="pos">the position of that cell</param>
    /// <returns>an array of all the neighbors of that cell</returns>
    public static Cell[] Neighbors(Cell pos)
    {
        int x = pos.x;
        int y = pos.y;
        List<Cell> neighbors = new List<Cell>();

        //the four axis
        if (map[x - 1, y] == CellType.Traversible)
            neighbors.Add(new Cell(x - 1, y));
        if (map[x + 1, y] == CellType.Traversible)
            neighbors.Add(new Cell(x + 1, y));
        if (map[x, y - 1] == CellType.Traversible)
            neighbors.Add(new Cell(x, y + 1));
        if (map[x, y - 1] == CellType.Traversible)
            neighbors.Add(new Cell(x, y - 1));

        //two additional axis for hex grid
        if (IsHexGrid)
        {
            if (map[x - 1, y - 1] == CellType.Traversible)
                neighbors.Add(new Cell(x - 1, y - 1));
            if (map[x + 1, y + 1] == CellType.Traversible)
                neighbors.Add(new Cell(x + 1, y + 1));
        }
        return neighbors.ToArray();
    }

    /// <summary>
    /// gets the position of a specified player
    /// </summary>
    /// <param name="playerIndex">the player index</param>
    /// <returns>the requested player's position</returns>
    public static Cell GetPlayerPosition(int playerIndex)
    {
        return playerPositions[playerIndex];
    }

    public static Cell[] FloodFill(Cell start, int distance)
    {
        /*
        List<Cell> list = new List<Cell>();
        list.Add(start);
        int firstIndexOfCurrentIteration = 0;
        while (distance > 0)
        {
            int oldListCount = list.Count;
            for (int i = firstIndexOfCurrentIteration; i < list.Count; i++)
            {
                list.AddRange(Neighbors(list[i]));
            }
            firstIndexOfCurrentIteration = oldListCount;
            distance--;
        }
        */
        Debug.Log("MapManager.FloodFill(): calling with cell " + start + ", distance: " + distance);

        HashSet<Cell> hashSet = new HashSet<Cell>();
        Queue<Cell> queue = new Queue<Cell>();
        queue.Enqueue(start);
        while (distance > 0)
        {
            int queueCount = queue.Count;
            for(int i=0; i < queueCount; i++)
            {
                Cell currentCell = queue.Dequeue();
                Cell[] neighbors = Neighbors(currentCell);
                for(int j = 0; j < neighbors.Length; j++)
                {
                    hashSet.Add(neighbors[j]);
                    queue.Enqueue(neighbors[j]);

                    Debug.Log("MapManager.FloodFill(): adding" + neighbors[j] + " to the list.");
                }
            }
            distance--;
        }

        Cell[] result = new Cell[hashSet.Count];
        hashSet.CopyTo(result);
        return result;
    }

    /// <summary>
    /// looks for a path that is similar to a given path. The origin will be the same as in the given path and if there is a path that reaches the destination
    /// and that is shorter than maxLength it will return that path. Otherwise it will return null.
    /// </summary>
    /// <param name="path">the path to which a similar path should be found</param>
    /// <param name="destination">the desired destination of the path</param>
    /// <param name="maxLength">the maximum length of the path</param>
    /// <returns>a path that is similar to the provided path, but reaches the destination if possible. If not possible, returns null</returns>
    public static Path GetMostSimilarPath(Path path, Cell destination, int maxLength)
    {
        Queue<Path> pathQueue = new Queue<Path>();
        pathQueue.Enqueue(new Path(path.Start));

        //get all paths that lead to the destination and are at most maxLength long
        while(pathQueue.Count > 0 && pathQueue.Peek().Length < maxLength)
        {
            Path currentPath = pathQueue.Dequeue();
            foreach(Cell possibleNextCell in Neighbors(currentPath.End))
            {
                int remainingMaxLength = maxLength - currentPath.Length - 1;

                //if the cell is not in the path and it is possible for the path to still reach the destination if the next cell is the one that is currently evaluated, add it to the queue
                if (!currentPath.Contains(possibleNextCell) && possibleNextCell.Distance(destination) <= remainingMaxLength)
                {
                    Path pathWithThisCell = new Path(currentPath);
                    pathWithThisCell.Add(possibleNextCell);
                    pathQueue.Enqueue(pathWithThisCell);
                }
            }
        }

        //if no paths are left, return null, else return the path with lowest dissimilarity
        if(pathQueue.Count == 0)
        {
            Debug.Log("MapManager.GetMostSimilarPath(): found no paths");
            return null;
        }
        else
        {
            Path pathWithSmallestHausdorffDistance = null;
            int smallestHausdorffDistance = int.MaxValue;
            foreach(Path candidate in pathQueue)
            {
                if(path.HausdorffDistanceTo(candidate) < smallestHausdorffDistance)
                {
                    smallestHausdorffDistance = path.HausdorffDistanceTo(candidate);
                    pathWithSmallestHausdorffDistance = candidate;
                }
            }
            Debug.Log("MapManager.GetMostSimilarPath(): found a path");
            return pathWithSmallestHausdorffDistance;
        }
    }


    #region Events

    public delegate void CellSetEventHandler(object source, CellEventArgs args);
    public static event CellSetEventHandler CellSet;

    /// <summary>
    /// sends an event that a cell has been altered
    /// </summary>
    /// <param name="cell">the cell that has been altered</param>
    protected static void OnCellSet(Cell cell)
    {
        if (CellSet != null)
        {
            CellSet(null, new CellEventArgs() { Cell = cell });
        }
    }

    public class CellEventArgs : EventArgs
    {
        public Cell Cell { get; set; }
    }

    #endregion
}


public static class CellTypeExtension
{
    /// <summary>
    /// gets the color of a cell type for visualization
    /// </summary>
    /// <param name="cellType">the cell type whose color is queried</param>
    /// <returns>the color of that type of cell</returns>
    public static Color GetColor(this MapManager.CellType cellType)
    {
        Color color = Color.magenta;
        switch (cellType)
        {
            case MapManager.CellType.Out_Of_Bounds:
                break;
            case MapManager.CellType.Uninitialized:
                break;
            case MapManager.CellType.Wall:
                color = Color.black;
                break;
            case MapManager.CellType.Traversible:
                color = Color.white;
                break;
            case MapManager.CellType.Player1:
            case MapManager.CellType.Player2:
            case MapManager.CellType.Player3:
            case MapManager.CellType.Player4:
                color = Color.red;
                break;
            case MapManager.CellType.TempMark1:
                color = Color.green;
                break;
            case MapManager.CellType.TempMark2:
                color = Color.blue;
                break;
            default:
                break;
        }
        return color;
    }
}