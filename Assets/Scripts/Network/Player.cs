using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// a class that represents a player
/// </summary>
public class Player { 
    public string name;
    public int id;

    public static Player own;   //which player is the client?
    public static int ownIndex; //which index within the game does he have?

    /// <summary>
    /// deserialization constructor
    /// </summary>
    /// <param name="data">the data to be deserialized</param>
    /// <param name="index">the index at which to deserialize</param>
    private Player(byte[] data, ref int index)
    {
        //set id
        this.id = BitConverter.ToInt32(data, index); //this has already been converted from big endian by the deserialize method (when checked for null)
        index += 4;

        //get name length and name
        int nameLength = BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index);
        index += 4;

        this.name = Encoding.UTF8.GetString(data, index, nameLength);
        index += nameLength;
    }

    /// <summary>
    /// calls the deserialization constructor
    /// this method first checks if the player to deserialize is actually a null value
    /// if not, the actual constructor is called
    /// </summary>
    /// <param name="data"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static Player Deserialize(byte[] data, ref int index)
    {
        if (BitConverter.ToInt32(Endianness.FromBigEndian(data, index), index) == 0)
        {
            index += 4;
            return null;
        }
        else
        {
            return new Player(data, ref index);
        }
    }
}