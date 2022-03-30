using Assets.Scripts.Game.Cards.CardTypes;
using System.Collections.Generic;
using UnityEngine;

//TODO make class static and detach from gameobject
/// <summary>
/// a class that controls the monsters on the board
/// </summary>
public class MonsterManager : MonoBehaviour {

    static Dictionary<int, Monster> monsterDictionary;
    public GameObject TestMonsterPrefab;
    static MonsterManager instance;

    private void Awake()
    {
        monsterDictionary = new Dictionary<int, Monster>();
        instance = this;
        //TODO init monster loading from database
    }

    /// <summary>
    /// spawns a monster
    /// </summary>
    /// <param name="card">the card which the monster originates from</param>
    /// <param name="monsterInstanceID">the instance ID of that monster</param>
    /// <param name="cell">the place where it is spawned</param>
    public static void SpawnMonster(MonsterCard card, int monsterInstanceID, Cell cell)
    {
        if (!cell.IsNotSet)
        {
            //spawn monster visually
            Debug.Log("MonsterManager.SpawnMonster(): Spawning monster with id " + monsterInstanceID +  " on " + cell.ToString());
            GameObject monsterGO = Instantiate(instance.TestMonsterPrefab, cell.Center, Quaternion.identity);
            monsterGO.transform.SetParent(instance.gameObject.transform);

            //Add Monster to dictionary
            Monster monster = monsterGO.GetComponent<Monster>();
            monsterDictionary.Add(monsterInstanceID, monster);

            monster.Init(monsterInstanceID, card, cell);

            //TODO remove debug color
            monsterGO.gameObject.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 0f);
            //todo init monster with position, effects, etc
        }
    }
}