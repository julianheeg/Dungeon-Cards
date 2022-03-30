using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// a script that is attached to the individual lobbies in the lobby join menu. It is only used to display information about the lobbies
/// like how many people are already in there and who hosted it. 
/// </summary>
public class LobbyPreJoinInfo : MonoBehaviour {
    public int id;
    public int maxPlayers;
    public int currentPlayers;
    public string hostName;

    /// <summary>
    /// deserializes this lobby from a data stream and sets the parameters
    /// </summary>
    /// <param name="data">the data to deserialize</param>
    /// <param name="index">the index at which the information about this lobby starts</param>
    public void Deserialize(byte[] data, ref int index)
    {
        try
        {
            //get ID
            this.id = BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index);
            index += 4;

            //get max players
            this.maxPlayers = BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index);
            index += 4;

            //get current players
            this.currentPlayers = BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index);
            index += 4;

            //get host name
            hostName = Player.Deserialize(data, ref index).name;
        }
        catch (IndexOutOfRangeException)
        {
            ErrorMessageDisplayer.DisplayMessage("A lobby couldn't be deserialized correctly. The server sent a lobby list that is in an invalid format.", false, true);
        }
    }
}