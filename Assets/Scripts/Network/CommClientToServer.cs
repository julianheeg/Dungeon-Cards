using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

//enums used for communication as defined in the network protocol file
public enum ServerTopLevel { Server = 0, Lobby, Game, Player };
public enum ServerLevel { Quit = 0, JoinLobby, CreateLobby, List, Login };
public enum LobbyLevel { Leave = 0, Ready, Start };
public enum GameLevel { LevelLoaded = 0, CardActivation, MonsterMovement };
public enum PlayerLevel { ChangeDeck = 0, AddDeck, RemoveDeck };

/// <summary>
/// a helper class which converts integers to and from big endian into the format that the running computer uses
/// </summary>
public static class Endianness
{
    /// <summary>
    /// turns a sequence of bytes that define an integer into big endian format
    /// </summary>
    /// <param name="data">the bytes</param>
    /// <returns>a big endian representation of the input</returns>
    public static byte[] ToBigEndian(byte[] data)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(data);
        }
        return data;
    }

    /// <summary>
    /// turns a sequence of bytes that define an integer into the running computer's endianness at a specific index
    /// </summary>
    /// <param name="data">the bytes, usually given as an entire networking message</param>
    /// <param name="index">the start index of the byte sequence</param>
    /// <returns>the byte sequence where the integer is in the running computer's endianness</returns>
    public static byte[] FromBigEndian(byte[] data, int index)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(data, index, 4);
        }
        return data;
    }
}

/// <summary>
/// all the possible network messages that need to be sent
/// </summary>
public static class Messages
{

    #region MainMenuController

    /// <summary>
    /// requests the server to create a lobby for this player
    /// </summary>
    internal static void SendCreateLobbyMessage()
    {
        List<byte> createLobbyMessage = new List<byte>(2);
        createLobbyMessage.Add((byte)ServerTopLevel.Server);
        createLobbyMessage.Add((byte)ServerLevel.CreateLobby);
        NetworkManager.Send(createLobbyMessage);
    }

    /// <summary>
    /// requests a list of all open lobbies from the server
    /// </summary>
    internal static void SendLobbyListMessage()
    {
        //request a list of open lobbies
        List<byte> listMessage = new List<byte>(2);
        listMessage.Add((byte)ServerTopLevel.Server);
        listMessage.Add((byte)ServerLevel.List);
        NetworkManager.Send(listMessage);
    }

    /// <summary>
    /// requests the server to join the specified lobby
    /// </summary>
    /// <param name="lobby"></param>
    internal static void SendLobbyJoinMessage(LobbyPreJoinInfo lobby)
    {
        //send a message to the server that the player wants to join the lobby with the attached ID
        List<byte> lobbyJoinMessage = new List<byte>(6);
        lobbyJoinMessage.Add((byte)ServerTopLevel.Server);
        lobbyJoinMessage.Add((byte)ServerLevel.JoinLobby);
        lobbyJoinMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(lobby.id)));
        
        NetworkManager.Send(lobbyJoinMessage);
    }

    /// <summary>
    /// sends a message to the server that the client wants to leave the lobby
    /// </summary>
    internal static void SendLobbyLeaveMessage()
    {
        List<byte> lobbyLeaveMessage = new List<byte>(2);
        lobbyLeaveMessage.Add((byte)ServerTopLevel.Lobby);
        lobbyLeaveMessage.Add((byte)LobbyLevel.Leave);
        NetworkManager.Send(lobbyLeaveMessage);
    }

    /// <summary>
    /// requests the server to start a game
    /// </summary>
    internal static void SendStartGameMessage()
    {
        List<byte> startGameMessage = new List<byte>(2);
        startGameMessage.Add((byte)ServerTopLevel.Lobby);
        startGameMessage.Add((byte)LobbyLevel.Start);
        NetworkManager.Send(startGameMessage);
    }

    #endregion


    #region LoginButton

    /// <summary>
    /// sends a message to the server with the specified log in data
    /// </summary>
    /// <param name="username">the local user's username</param>
    /// <param name="password">the local user's password</param>
    internal static void SendSignInMessage(String username, string password)
    {
        //initialize and add header
        List<byte> loginData = new List<byte>();
        loginData.Add((byte)ServerTopLevel.Server);
        loginData.Add((byte)ServerLevel.Login);

        //add username
        byte[] encodedUsername = Encoding.UTF8.GetBytes(username);
        int usernameLength = encodedUsername.Length;
        loginData.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(usernameLength)));
        loginData.AddRange(encodedUsername);

        //add password
        byte[] encodedPassword = Encoding.UTF8.GetBytes(password);
        int passwordLength = encodedPassword.Length;
        loginData.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(passwordLength)));
        loginData.AddRange(encodedPassword);

        //send
        NetworkManager.Send(loginData);
    }
    #endregion


    #region LobbyPlayerPanel

    /// <summary>
    /// sends a message to the server that the local player is ready/not ready
    /// </summary>
    /// <param name="ready">the ready value</param>
    internal static void SendReadyMessage(bool ready)
    {
        List<byte> readyMessage = new List<byte>(3);
        readyMessage.Add((byte)ServerTopLevel.Lobby);
        readyMessage.Add((byte)LobbyLevel.Ready);
        readyMessage.AddRange(BitConverter.GetBytes(ready));
        NetworkManager.Send(readyMessage);
    }

    #endregion


    #region GameManager

    /// <summary>
    /// sends a message to the server that this client has loaded the level
    /// </summary>
    internal static void SendLevelLoadedMessage()
    {
        List<byte> readyMessage = new List<byte>(3);
        readyMessage.Add((byte)ServerTopLevel.Game);
        readyMessage.Add((byte)GameLevel.LevelLoaded);
        NetworkManager.Send(readyMessage);
    }

    #endregion


    #region Gameplay


    /// <summary>
    /// sends a message to the server that the user wants to activate a card
    /// </summary>
    /// <param name="instanceID">the instance ID of the card to activate</param>
    /// <param name="cell">the location where the card is activated. Can be null to specify that a location is not applicable (for example if the spell affects every tile)</param>
    internal static void SendCardActivation(int instanceID, Cell? cell)
    {
        List<byte> cardActivationMessage = new List<byte>(14);
        cardActivationMessage.Add((byte)ServerTopLevel.Game);
        cardActivationMessage.Add((byte)GameLevel.CardActivation);

        cardActivationMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(instanceID)));

        if (cell != null)
        {
            cardActivationMessage.AddRange(((Cell)cell).Serialize());
        }
        else
        {
            cardActivationMessage.AddRange(Cell.UnSetGridPosition.Serialize());
        }

        NetworkManager.Send(cardActivationMessage);
    }

    /// <summary>
    /// sends a message to the server that the user wants to move a monster
    /// </summary>
    /// <param name="instanceID">the instance ID of the monster to move</param>
    /// <param name="path">the movement path</param>
    internal static void SendMonsterMovement(int instanceID, Path path)
    {
        List<byte> monsterMovementMessage = new List<byte>(10 + (path.Length) * 8);
        monsterMovementMessage.Add((byte)ServerTopLevel.Game);
        monsterMovementMessage.Add((byte)GameLevel.MonsterMovement);
        monsterMovementMessage.AddRange(Endianness.ToBigEndian(BitConverter.GetBytes(instanceID)));
        monsterMovementMessage.AddRange(path.Serialize());
        NetworkManager.Send(monsterMovementMessage);
    }

    #endregion
}