using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// a class that parses messages received from the server
/// </summary>
public static class DataParser {

    private enum ClientTopLevel { Main = 0, Game, GameState, Ping };

    /// <summary>
    /// parses the first byte of the data
    /// </summary>
    /// <param name="data">the data to parse</param>
    public static void ParseClientTopLevel(byte[] data) {
        if (data.Length == 0)
        {
            if (!NetworkManager.IsConnected())
            {
                ErrorMessageDisplayer.DisplayMessage("Connection to the server has been closed.", false, false);
            }
            else //TODO: remove
            {
                Debug.Log("DataParser.parseClientTopLevel(): received 0 bytes, but connection is still open?");
            }
        }
        else {
            switch ((ClientTopLevel)data[0])
            {
                case ClientTopLevel.Main:
                    ParseClientMenuLevel(data);
                    break;
                case ClientTopLevel.Game:
                    ParseClientGameLevel(data);
                    break;
                case ClientTopLevel.GameState:
                    GameManager.ParseGameStateLevel(data);
                    break;
                case ClientTopLevel.Ping:
                    if (data.Length != 1)
                    {
                        Debug.LogError("Parse error on Client top level. First byte was ClientTopLevel.Ping, but the message was longer than 1 byte");
                    }
                    break;
                default:
                    Debug.LogError("Parse error on client top level. First Byte was " + data[0]);
                    break;
            }
        }
    }

    private enum ClientMenuLevel { List = 0, LobbyFull, LobbyIDNotFound, LobbyJoin, LobbyLeave, LoginAccept, LoginReject, LobbyOtherJoin, PlayerReady };
    private enum LoginReject { WrongLogin = 0 };

    /// <summary>
    /// parses the second byte of the data when the first byte was ClientTopLevel.Main
    /// </summary>
    /// <param name="data">the data to parse</param>
    private static void ParseClientMenuLevel(byte[] data)
    {
        if (data.Length == 1)
        {
            Debug.LogError("Parse error on Client Menu Level. Data contained only one byte");
        }
        else
        {
            switch((ClientMenuLevel)data[1])
            {
                //displays the currently open lobbies
                case ClientMenuLevel.List:
                    MainMenuController.instance.ClearLobbyList();
                    int currentIndex = 2;
                    try
                    {
                        while (currentIndex < data.Length)
                        {

                            MainMenuController.instance.AddLobby(data, ref currentIndex);
                        }
                    }
                    catch (IndexOutOfRangeException) 
                    {
                        ErrorMessageDisplayer.DisplayMessage("There was an error during deserialization of the lobby list.", true, true);
                    }
                    break;

                //returns an error message
                case ClientMenuLevel.LobbyFull:
                    ErrorMessageDisplayer.DisplayMessage("The lobby you tried to join is already full. Please try joining another lobby or create your own.", false, false);
                    break;

                //returns an error message
                case ClientMenuLevel.LobbyIDNotFound:
                    ErrorMessageDisplayer.DisplayMessage("You tried to join a lobby that apparently doesn't exist. Perhaps it has been closed? If not, please try joining the lobby via inputting the lobby ID", false, false);
                    break;
                
                //sets the lobby field in the main menu controller
                case ClientMenuLevel.LobbyJoin:
                    try
                    {
                        MainMenuController.instance.EnterLobby(data);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        ErrorMessageDisplayer.DisplayMessage("There was an error during deserialization of the lobby.", true, true);
                    }
                    break;

                case ClientMenuLevel.LobbyLeave:
                    if (data.Length == 6)
                    {
                        MainMenuController.instance.LobbyLeave(BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2));
                    }
                    else
                    {
                        Debug.LogError("Parse error on Client Menu Level. Second byte was LOBBY_LEAVE, but received " + data.Length + " bytes (expected 6)");
                    }
                    break;

                case ClientMenuLevel.LoginAccept:
                    MainMenuController.instance.ActivateMultiplayerMenu();
                    int index = 2;
                    Player.own = Player.Deserialize(data, ref index);
                    break;

                case ClientMenuLevel.LoginReject:
                    if (data.Length == 3)
                    {
                        switch ((LoginReject)data[2])
                        {
                            case LoginReject.WrongLogin:
                                ErrorMessageDisplayer.DisplayMessage("The username and password do not match", false, false);
                                break;

                            default:
                                Debug.LogError("Parse error on Client Menu Level: Login_reject. Third byte was " + data[2]);
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError("Parse error on Client Menu Level. Second byte was LOGIN_REJECT, but received " + data.Length + " bytes (expected 3)");
                    }
                    break;

                case ClientMenuLevel.LobbyOtherJoin:
                    try
                    {
                        int position = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                        int ind = 6;
                        Player player = Player.Deserialize(data, ref ind);
                        MainMenuController.instance.AddPlayerToLobby(player, position);
                    }
                    catch(IndexOutOfRangeException)
                    {
                        ErrorMessageDisplayer.DisplayMessage("There was an error during deserialization of a joining player.", true, true);
                    }
                    break;

                case ClientMenuLevel.PlayerReady:
                    if (data.Length == 7)
                    {
                        int position = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                        bool ready = BitConverter.ToBoolean(data, 6);
                        MainMenuController.instance.SetReady(position, ready);
                    }
                    else
                    {
                        Debug.LogError("Parse error on Client Menu Level. Second byte was PLAYER_READY, but received " + data.Length + " bytes (expected 7)");
                    }
                    break;

                default:
                    Debug.LogError("Parse error on Client Menu Level. Second byte was " + data[1]);
                    break;
            }
        }
    }


    private enum ClientGameLevel { GameMeta, MapRow, GameStart, CardInit, CardFaceInit }

    /// <summary>
    /// parses the second byte of the data when the first byte was ClientTopLevel.Game
    /// </summary>
    /// <param name="data">the data to parse</param>
    private static void ParseClientGameLevel(byte[] data)
    {
        if (data.Length == 1)
        {
            Debug.LogError("Parse error on Client Game Level. Data contained only one byte");
        }
        else
        {
            switch ((ClientGameLevel)data[1])
            {
                case ClientGameLevel.GameMeta:
                    if (data.Length >= 27 && data.Length % 8 == 3)
                    {
                        Debug.Log("received meta data");
                        GameManager.SetMetaData(data);
                    }
                    else
                    {
                        Debug.LogError("Parse error on Client Game Level. Second byte was GAME_META, but received " + data.Length + " bytes (expected at least 27 and 3 mod 8)");
                    }
                    break;

                case ClientGameLevel.MapRow:
                    if (data.Length == 6 + MapManager.Width)
                    {
                        //Debug.Log("setting row");

                        int row = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                        for (int i = 0; i < MapManager.Width; i++)
                        {
                            MapManager.SetCell(new Cell(row, i), (MapManager.CellType)data[i + 6]);
                        }
                    }
                    else
                    {
                        Debug.LogError("Parse error on Client Game Level. Second byte was MAP_ROW, but received " + data.Length + " bytes (expected " + (MapManager.Width + 6) + ")");
                    }
                    break;

                case ClientGameLevel.GameStart:
                    if (data.Length == 2)
                    {
                        Debug.Log("loading game scene");
                        SceneManager.LoadSceneAsync("Game");
                    }
                    else
                    {
                        Debug.LogError("Parse error on Client Game Level. Second byte was GAME_START, but received " + data.Length + " bytes (expected 2)");
                    }
                    break;

                case ClientGameLevel.CardInit:
                    if (data.Length == 11)
                    {
                        int cardInstanceID = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                        int owner = BitConverter.ToInt32(Endianness.FromBigEndian(data, 6), 6);
                        CardMeta.Location location = (CardMeta.Location)data[10];
                        
                        CardManager.InitCard(cardInstanceID, owner, location);
                    }
                    else
                    {
                        Debug.LogError("Parse error on Client Game Level. Second byte was CardInit, but received " + data.Length + " bytes (expected 11)");
                    }
                    break;

                case ClientGameLevel.CardFaceInit:
                    if (data.Length == 10)
                    {
                        int cardInstanceID = BitConverter.ToInt32(Endianness.FromBigEndian(data, 2), 2);
                        int cardID = BitConverter.ToInt32(Endianness.FromBigEndian(data, 6), 6);

                        CardManager.InitCardFace(cardInstanceID, cardID);
                    }
                    else
                    {
                        Debug.LogError("Parse error on Client Game Level. Second byte was CardFaceInit, but received " + data.Length + " bytes (expected 10)");
                    }
                    break;

                default:
                    Debug.LogError("Parse error on Client Game Level. Second byte was " + data[1]);
                    break;
            }
        }
    }
}