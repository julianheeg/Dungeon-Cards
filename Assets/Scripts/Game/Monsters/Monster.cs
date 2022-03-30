using Assets.Scripts.Game.Cards.CardTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

/// <summary>
/// a script that represents a monster
/// </summary>
public class Monster : MonoBehaviour
{
    public static readonly int visionRange = 3;

    public int instanceID;
    public int owner;
    public Cell position;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int Cost { get; private set; }
    public int Damage { get; private set; }
    public int MovementRange { get; private set; }

    private static Path movementPath; //this is used to display the path when the player attempts to move a monster
    private static Path previousPath;
    private static Color defaultMovementColor = Color.yellow;

    /// <summary>
    /// initialization
    /// </summary>
    /// <param name="instanceID">the instance ID of this monster</param>
    /// <param name="card">the card that this monster originates from</param>
    /// <param name="position">the position where the monster is at</param>
    public void Init(int instanceID, MonsterCard card, Cell position)
    {
        this.instanceID = instanceID;

        //MonsterCardTemplate cardTemplate = CardDatabase.IDToTemplate<MonsterCardTemplate>(cardID);
        MaxHealth = card.health;
        CurrentHealth = MaxHealth; //todo look for modifications
        Cost = card.cost;
        Damage = card.damage;
        MovementRange = card.movementRange;

        this.position = position;
        gameObject.name = card.name;
    }


    #region movement input
    /// <summary>
    /// highlights possible actions when a monster is clicked at
    /// </summary>
    void OnMouseDown()
    {
        //TODO: Check for own monster or do a different kind of highlighting for enemy monsters
        PossibleActionsHighlighter.HighlightValidMovementTiles(this);
        movementPath = new Path(position);
        PossibleActionsHighlighter.SetColor(position, defaultMovementColor);
    }

    /// <summary>
    /// performs a raycast repeatedly and prints the results to console for debugging purposes.
    /// TODO: check for visual effects or anything else that should happen
    /// </summary>
    void OnMouseDrag()
    {
        //cast ray onto map
        RaycastHit hit;
        if (PlayerCamera.RaycastOntoMap(Input.mousePosition, out hit, LayerMask.GetMask("Map")))
        {
            //cast to mesh collider and get submesh
            if (hit.collider is MeshCollider)
            {
                MeshCollider meshCollider = (MeshCollider)hit.collider;
                MeshSuperClass mesh = meshCollider.GetComponent<MeshSuperClass>();
                Assert.IsNotNull(mesh);

                //get cell
                Cell? maybeCell = mesh.TriangleIndexToCell(hit.triangleIndex);
                if (maybeCell != null)
                {
                    Cell cell = (Cell)maybeCell;
                    Debug.Log("Monster.OnMouseDrag(): Monster is being dragged over cell " + cell.ToString());

                    //TODO check if something should happen
                    //TODO visual effect
                    if (cell.IsAdjacent(movementPath.End))
                    {
                        //if the user wants to go back, remove the current end cell and return
                        if (movementPath.Length >= 1)
                        {
                            if (cell == movementPath.BeforeEnd)
                            {
                                PossibleActionsHighlighter.ResetColor(movementPath.End);
                                movementPath.RemoveLast();
                                return;
                            }
                        }

                        //if cell is traversible, add it to the path
                        if (MapManager.GetCell(cell) == MapManager.CellType.Traversible)
                        {

                            //TODO: add check for when to return to the previous path

                            if (movementPath.Length < MovementRange)
                            {
                                movementPath.Add(cell);
                                PossibleActionsHighlighter.SetColor(cell, defaultMovementColor);
                            }
                            else if(PossibleActionsHighlighter.IsValidLocation(cell))
                            {
                                previousPath = movementPath;
                                movementPath = MapManager.GetMostSimilarPath(previousPath, cell, MovementRange);
                                PossibleActionsHighlighter.ResetColor(previousPath);
                                PossibleActionsHighlighter.SetColor(movementPath, defaultMovementColor);
                            }
                        }

                        //TODO: Check for Attack Targets
                    }
                }
                
            }
            else
            {
                Debug.LogError("Monster.OnMouseDrag(): raycast hit an object that is not a mesh collider. hit object: " + hit.collider.ToString() + "at " + hit.point.ToString());
            }
        }
    }

    /// <summary>
    /// Checks whether the mouse location is a valid target tile for an action when it is released and if so, performs it.
    /// Removes the valid location highlighting afterwards.
    /// </summary>
    void OnMouseUp()
    {
        Cell? cell = null;

        //cast ray onto map
        RaycastHit hit;
        if (PlayerCamera.RaycastOntoMap(Input.mousePosition, out hit, LayerMask.GetMask("Map")))
        {
            //cast to mesh collider and get mesh
            if (hit.collider is MeshCollider)
            {
                MeshCollider meshCollider = (MeshCollider)hit.collider;
                Debug.Log("Monster.OnMouseUp(): Hit a mesh collider of type " + meshCollider.GetComponent<MeshSuperClass>().GetType().ToString());
                MeshSuperClass mesh = meshCollider.GetComponent<MeshSuperClass>();
                Assert.IsNotNull(mesh);

                //get cell
                cell = mesh.TriangleIndexToCell(hit.triangleIndex);
                Assert.IsTrue(cell != null);
                Debug.Log("Monster.OnMouseUp(): Card is being dropped on cell " + cell.ToString());

                //compare cell to valid locations
                if (cell != null && PossibleActionsHighlighter.IsValidLocation((Cell)cell))
                {
                    Debug.Log("Monster.OnMouseUp(): Drop valid");

                    //TODO check for other activation conditions
                    //TODO visual effect
                
                    Messages.SendMonsterMovement(instanceID, movementPath);
                }
                else
                {
                    Debug.Log("Monster.OnMouseUp(): Drop not valid");
                }
            }
            else
            {
                Debug.LogError("Monster.OnMouseUp(): raycast hit an object that is not a mesh collider. hit object: " + hit.collider.ToString() + "at " + hit.point.ToString());
            }
        }

        movementPath = null;
        previousPath = null;
        PossibleActionsHighlighter.RemoveHighlighting();
    }
    #endregion
}