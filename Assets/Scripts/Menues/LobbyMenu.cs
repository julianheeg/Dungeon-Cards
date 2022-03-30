using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// a script that handles the lobby stuff
/// </summary>
public class LobbyMenu : MonoBehaviour {
    private int maxPlayers;
    private int currentPlayers;
    public LobbyPlayerPanel[] playerPanels = new LobbyPlayerPanel[4];
    public Button startGameButton;

    public int ownPosition;


    /// <summary>
    /// deserializes a lobby from a data stream and sets all fields accordingly
    /// </summary>
    /// <param name="data">the data to be deserialized</param>
    public void Deserialize(byte[] data)
    {
        currentPlayers = 0;

        int index = 2;

        try
        {
            //read max players
            int maxPlayers = BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index);
            this.maxPlayers = maxPlayers;
            index += 4;

            //read player data
            for (int i = 0; i < maxPlayers; i++)
            {
                Player player = Player.Deserialize(data, ref index);
                if (player != null)
                {
                    playerPanels[i].gameObject.SetActive(true);
                    playerPanels[i].SetPlayer(player);
                    currentPlayers++;

                    //set own position if player id matches own id
                    if (player.id == Player.own.id)
                    {
                        ownPosition = i;
                    }
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
            ErrorMessageDisplayer.DisplayMessage("The lobby couldn't be deserialized correctly. Please try joining another lobby.", false, true);
            MainMenuController.instance.Back();
        }

        Assert.AreEqual(data.Length, index);
    }

    /// <summary>
    /// adds a player to a lobby
    /// </summary>
    /// <param name="playerID">the player's ID</param>
    /// <param name="playerName">the player's name</param>
    /// <param name="position">the index of the place in the lobby</param>
    public void AddPlayer(Player player, int position)
    {
        //set active
        playerPanels[position].gameObject.SetActive(true);
        //set player
        playerPanels[position].SetPlayer(player);
        currentPlayers++;


        ResetReadyForAll();
    }

    /// <summary>
    /// causes a player to be no longer displayed when they leave the lobby 
    /// </summary>
    /// <param name="position">the lobby position of the leaving player</param>
    public void Leave(int position)
    {
        if (playerPanels[position].isActiveAndEnabled)
        {
            playerPanels[position].gameObject.SetActive(false);
            currentPlayers--;

            ResetReadyForAll();
        }
        else
        {
            throw new ArgumentException("The client and the server seem to be out of synch.", "position");
        }
    }

    /// <summary>
    /// resets all ready flags from players. Deactivates the start game button
    /// called when the lobby state changes (player joins/leaves/etc)
    /// </summary>
    private void ResetReadyForAll()
    {
        foreach(LobbyPlayerPanel panel in playerPanels)
        {
            panel.ready = false;
        }
        startGameButton.interactable = false;
    }

    /// <summary>
    /// sets the ready variable for the player at the specified position
    /// </summary>
    /// <param name="position">the player's position</param>
    /// <param name="ready">the ready value</param>
    public void SetReady(int position, bool ready)
    {
        playerPanels[position].ready = ready;
        CheckAllReady();
    }

    /// <summary>
    /// activates the start game button if all players are ready and the amount of players is a valid number for a match
    /// </summary>
    private void CheckAllReady()
    {
        //check if the current number of players is a valid number for a match
        if(Config.allowedNumbersOfPlayers.Contains(currentPlayers))
        {
            //check if all players are ready
            bool allReady = true;
            for (int i = 0; i < maxPlayers; i++)
            {
                if (playerPanels[i].isActiveAndEnabled)
                {
                    allReady &= playerPanels[i].ready;
                }
            }

            if (allReady)
            {
                startGameButton.interactable = true;
                return;
            }
        }
        startGameButton.interactable = false;
    }
}