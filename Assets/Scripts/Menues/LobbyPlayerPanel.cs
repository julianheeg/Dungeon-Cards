using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// a script that handles the displaying of the players within a lobby
/// </summary>
public class LobbyPlayerPanel : MonoBehaviour {
    [SerializeField]
    private Toggle ReadyToggle;
    [SerializeField]
    private Text PlayerName;
    
    public bool ready;

    /// <summary>
    /// places a player in this slot. Resets the ready switch
    /// </summary>
    /// <param name="playerID">the player's id</param>
    /// <param name="playerName">the player name</param>
    public void SetPlayer(Player player)
    {
        //display name
        PlayerName.text = player.name;

        //set ready flag
        this.ready = false;
        this.ReadyToggle.isOn = false;

        //set the toggle interactable only if it belongs to the own player
        ReadyToggle.interactable = (player.id == Player.own.id);
    }

    /// <summary>
    /// sends a message to the server that this client has checked/unchecked the ready checkbox
    /// </summary>
    public void onValueChanged()
    {
        //this check is needed because else the changing of other players' ready state would trigger the OnValueChanged() event
        //which would result in this client sending the same message to the server, declaring it is ready
        if (ReadyToggle.interactable)
        {
            this.ready = ReadyToggle.isOn;
            Messages.SendReadyMessage(ready);
        }

        ReadyToggle.isOn = ready;
    }
}