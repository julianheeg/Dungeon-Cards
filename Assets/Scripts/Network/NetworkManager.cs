using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// a script that handles networking with the server
/// </summary>
public class NetworkManager : MonoBehaviour {
    //config
    private static readonly int BUFFERSIZE = 8192;
    private static readonly string IP = "127.0.0.1";
    private static readonly int port = 62480;

    //singleton
    public static NetworkManager instance;
    private static bool setup;

    //network
    private Socket socket;
    private byte[] buffer;

    //message reconstruction
    //this is used to reconstruct the messages if they get fragmented during transmission 
    private List<byte> messageBuilder;

    //holds the messages from the server. necessary because the client listens asynchronously, but methods need to be called in the main thread (synchronously)
    //also several commands at once may be issued by the server
    private Queue<byte[]> messagesList;

    /// <summary>
    /// initializes all the fields and begins a connection
    /// </summary>
    public static void Setup()
    {
        if (!setup)
        {
            instance.buffer = new byte[BUFFERSIZE];

            instance.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            instance.socket.BeginConnect(IPAddress.Parse(IP), port, instance.ConnectCallback, null);
        }
    }

    /// <summary>
    /// don't destroy on load
    /// set singleton
    /// initialize fields
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad(this);

        instance = this;
        setup = false;

        messageBuilder = new List<byte>();
        messagesList = new Queue<byte[]>();
        

    }

    /// <summary>
    /// returns whether the socket is still connected; if not setup gets set to false, so the next time Setup() is called, another socket will be instantiated
    /// </summary>
    /// <returns></returns>
    public static bool IsConnected()
    {
        bool connected = !((instance.socket.Poll(1000, SelectMode.SelectRead) && (instance.socket.Available == 0)) || !instance.socket.Connected);
        if (!connected)
        {
            setup = false;
        }
        return connected;
    }

    /// <summary>
    /// parses and executes the messages from the server
    /// </summary>
    private void Update()
    {
        lock (messagesList)
        {
            while (messagesList.Count > 0)
            {
                byte[] data = messagesList.Dequeue();
                DataParser.ParseClientTopLevel(data);
            } 
        }
    }


    #region asynchronous callbacks
    //usual network stuff here

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Debug.Log("NetworkManager.ConnectCallback(): Calling EndConnect().");
            socket.EndConnect(ar);
            Debug.Log("NetworkManager.ConnectCallback(): EndConnect() called.");
            socket.BeginReceive(buffer, 0, BUFFERSIZE, SocketFlags.None, ReceiveCallback, null);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            //copy array
            int received = socket.EndReceive(ar);
            byte[] trimmedBuffer = new byte[received];
            Array.Copy(buffer, trimmedBuffer, received);

            //add array to the message builder
            messageBuilder.AddRange(trimmedBuffer);

            //loop
            while (true)
            {
                //if the length can't be read out, break out of the loop
                if (messageBuilder.Count <= 3)
                {
                    break;
                }

                //get length
                byte[] lengthInBytes = new byte[4];
                messageBuilder.CopyTo(0, lengthInBytes, 0, 4);
                int length = BitConverter.ToInt32(Endianness.FromBigEndian(lengthInBytes, 0), 0);

                //if the message builder contains at least a full message, add it to the messages list and remove it from the message builder
                if (messageBuilder.Count >= length)
                {
                    //get the message and write it into the queue
                    byte[] data = new byte[length];
                    messageBuilder.CopyTo(4, data, 0, length);
                    lock (messagesList)
                    {
                        messagesList.Enqueue(data);
                    }

                    //remove the current message
                    messageBuilder.RemoveRange(0, 4 + length);
                }
                //else break out of the loop
                else
                {
                    break;
                }
            }

            //receive again
            socket.BeginReceive(buffer, 0, BUFFERSIZE, SocketFlags.None, ReceiveCallback, null);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static void Send(List<byte> data)
    {
        //insert length at start
        byte[] _data = new byte[data.Count + 4];
        byte[] length = Endianness.ToBigEndian(BitConverter.GetBytes(data.Count));
        Array.Copy(length, _data, 4);

        //copy data
        data.CopyTo(_data, 4);

        instance.socket.BeginSend(_data, 0, _data.Length, SocketFlags.None, instance.SendCallback, null);
    }

    private void SendCallback(IAsyncResult ar)
    {
        socket.EndSend(ar);
    }

    #endregion
}