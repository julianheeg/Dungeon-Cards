using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// a script that handles error messages that are shown to the player in game
/// </summary>
public class ErrorMessageDisplayer : MonoBehaviour {

    /// <summary>
    /// displays a message (currently only to console, TODO: make it a dialog box)
    /// </summary>
    /// <param name="message">the message to display</param>
    /// <param name="critical">whether or not the error is critical in the sense that the program can't recover from it. This could for example be the result of an invalid game state.</param>
    /// <param name="sendToServer">whether this error message will be sent to the server for logging</param>
    public static void DisplayMessage(string message, bool critical, bool sendToServer)
    {
        Debug.Log("ErrorMessageDisplayer.DisplayMessage: " + message);
        //TODO: implement proper popup
        //TODO: implement send to server
        //throw new NotImplementedException();
    }

    /// <summary>
    /// prints an array and the function calling this function to the console
    /// </summary>
    /// <param name="source">the function which calls this function</param>
    /// <param name="array">the array to print</param>
    public static void LogArray(string source, byte[] array)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < array.Length; i++)
        {
            if (i % 4 == 2)
                sb.Append("| ");
            sb.Append(array[i].ToString() + ' ');
        }
        Debug.Log(source + ": " + sb);
    }
}