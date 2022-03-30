using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// just configurable variables
/// </summary>
public static class Config
{
    //Lobby
    public const int maxPlayers = 4;
    public static readonly List<int> allowedNumbersOfPlayers = new List<int> { 1, 2 }; //allowed numbers of players for a match. not const because compiler says so
    
    //Player
    public const int minPlayerNameLength = 3;
    public const int maxPlayerNameLength = 20;

    //Map
    public const int cellsPerSubmeshAndDirection = 5;
    public const float wallHeight = 0f; //TODO set
    public const float scale = 1f;

    //Cards
    public const float cardThickness = 0.005f;
    public const float translationMaxDistanceDelta = 30f; //how far does a card get moved per second?
    public const float rotationMaxDegreesDelta = 300f; //how far does a card get rotated per second?

    //Rendering
    public const float raycastIntersectionPlaneY = 1f;

}