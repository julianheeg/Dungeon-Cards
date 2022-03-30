using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// a script that controls the traversal of the different menues starting from the main menu
/// </summary>
public class MainMenuController : MonoBehaviour {

    //currently and previously active menues
    private enum Menu { MAIN_MENU, LOGIN_MENU, MULTIPLAYER_MENU, LOBBYSELECTOR_MENU, LOBBY_MENU};
    private Menu currentMenu = Menu.MAIN_MENU;
    private Menu previousMenu;

#pragma warning disable 0649
    //game objects to activate/deactivate, necessary references to other game objects
    [SerializeField]
    private GameObject
        mainMenu, loginMenu, multiplayerMenu, lobbyselectorMenu, lobbyMenu,
        backButton,
        lobbyJoinButton,
        lobbyPlayerPanel;
#pragma warning restore 0649
    //lobby list in the lobby selector menu
    [SerializeField]
    private Transform scrollPanel;

    //singleton
    public static MainMenuController instance;

    //whether the client is logged in. This is necessary because the client could go from multiplayer to main menu to multiplayer
    private bool loggedIn;


    /// <summary>
    /// set the singleton
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        loggedIn = false;
    }

    /// <summary>
    /// activator for main menu
    /// </summary>
    public void ActivateMainMenu()
    {
        previousMenu = currentMenu;
        currentMenu = Menu.MAIN_MENU;
        SetActiveMenu();
    }

    /// <summary>
    /// activator for the login menu
    /// if player is already logged in, skip this menu and go straight to multiplayer
    /// </summary>
    public void ActivateLoginMenu()
    {
        if (!loggedIn || !NetworkManager.IsConnected())
        {
            NetworkManager.Setup();
            previousMenu = currentMenu;
            currentMenu = Menu.LOGIN_MENU;
            SetActiveMenu();
        }
        else
        {
            ActivateMultiplayerMenu();
        }
    }

    /// <summary>
    /// activator for multiplayer menu
    /// </summary>
    public void ActivateMultiplayerMenu()
    {
        loggedIn = true;
        previousMenu = currentMenu;
        currentMenu = Menu.MULTIPLAYER_MENU;
        SetActiveMenu();
    }

    /// <summary>
    /// activator for lobby selector menu
    /// </summary>
    public void ActivateLobbySelectorMenu()
    {
        previousMenu = currentMenu;
        currentMenu = Menu.LOBBYSELECTOR_MENU;
        SetActiveMenu();

        Messages.SendLobbyListMessage();
    }

    /// <summary>
    /// clears the list of the lobbies. Called when the list should be refreshed.
    /// </summary>
    public void ClearLobbyList()
    {
        for (int i = scrollPanel.childCount - 1; i >= 0; i--) //always remove the last gameObject in list
        {
            Destroy(scrollPanel.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// adds a lobby to the lobby list in the lobby selector menu
    /// </summary>
    /// <param name="data">the lobby list data that the server sent</param>
    /// <param name="index">the index at which to deserialize next</param>
    public void AddLobby(byte[] data, ref int index)
    {
        //instantiate object
        GameObject newLobby = Instantiate(lobbyJoinButton);
        newLobby.transform.SetParent(scrollPanel);

        //deserialize from stream
        LobbyPreJoinInfo lobbyState = newLobby.GetComponent<LobbyPreJoinInfo>();
        lobbyState.Deserialize(data, ref index);

        //set text
        newLobby.transform.Find("PlayersMaxPlayers").gameObject.GetComponent<Text>().text = lobbyState.currentPlayers.ToString() + '/' + lobbyState.maxPlayers.ToString();
        newLobby.transform.Find("HostName").gameObject.GetComponent<Text>().text = lobbyState.hostName;
    }

    /// <summary>
    /// activator for lobby menu
    /// </summary>
    private void ActivateLobbyMenu()
    {
        //change to menu
        previousMenu = currentMenu;
        currentMenu = Menu.LOBBY_MENU;
        SetActiveMenu();
    }

    /// <summary>
    /// takes the user to the previous menu
    /// </summary>
    public void Back()
    {
        switch(currentMenu)
        {
            case Menu.LOGIN_MENU:
                currentMenu = Menu.MAIN_MENU;
                break;
            case Menu.MULTIPLAYER_MENU:
                currentMenu = Menu.MAIN_MENU;
                break;
            case Menu.LOBBYSELECTOR_MENU:
                currentMenu = Menu.MULTIPLAYER_MENU;
                break;
            case Menu.LOBBY_MENU:
                Messages.SendLobbyLeaveMessage();
                
                currentMenu = previousMenu;
                break;
        }
        SetActiveMenu();
    }


    /// <summary>
    /// quits to desktop
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// changes the menu to the lobby menu and deserializes data into a lobby menu object
    /// </summary>
    /// <param name="data">the data to be deserialized</param>
    public void EnterLobby(byte[] data)
    {
        ActivateLobbyMenu();
        lobbyMenu.GetComponent<LobbyMenu>().Deserialize(data);
    }

    /// <summary>
    /// adds a player to the lobby menu
    /// </summary>
    /// <param name="player">the player object</param>
    /// <param name="position">the player's position in the lobby</param>
    public void AddPlayerToLobby(Player player, int position)
    {
        lobbyMenu.GetComponent<LobbyMenu>().AddPlayer(player, position);
    }

    /// <summary>
    /// sets a player's ready flag in the lobby menu
    /// </summary>
    /// <param name="position">the player's position in the lobby</param>
    /// <param name="ready">the ready state</param>
    public void SetReady(int position, bool ready)
    {
        lobbyMenu.GetComponent<LobbyMenu>().SetReady(position, ready);
    }

    /// <summary>
    /// makes the player with the specified lobby position leave the lobby on the client side
    /// if the position is the own positino, it goes back to the previously opened menu
    /// </summary>
    /// <param name="position">the lobby position of the player who left the lobby</param>
    public void LobbyLeave(int position)
    {
        if (position == lobbyMenu.GetComponent<LobbyMenu>().ownPosition)
        {
            //back();
        }
        else
        {
            lobbyMenu.GetComponent<LobbyMenu>().Leave(position);
        }
    }

    /// <summary>
    /// activates/deactivates the menues according to the currently set menu
    /// </summary>
    private void SetActiveMenu()
    {
        mainMenu.SetActive(false);
        loginMenu.SetActive(false);
        multiplayerMenu.SetActive(false);
        lobbyselectorMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        backButton.SetActive(true);

        switch (currentMenu)
        {
            case Menu.MAIN_MENU:
                Debug.Log("changing to Main Menu");
                mainMenu.SetActive(true);
                backButton.SetActive(false);
                break;
            case Menu.LOGIN_MENU:
                Debug.Log("changing to Login Menu");
                loginMenu.SetActive(true);
                break;
            case Menu.MULTIPLAYER_MENU:
                Debug.Log("changing to Multiplayer Menu");
                multiplayerMenu.SetActive(true);
                break;
            case Menu.LOBBYSELECTOR_MENU:
                Debug.Log("changing to Lobby Selector Menu");
                lobbyselectorMenu.SetActive(true);
                break;
            case Menu.LOBBY_MENU:
                Debug.Log("changing to Lobby Menu");
                lobbyMenu.SetActive(true);
                break;
        }
    }



    #region GUI OnClick

    /// <summary>
    /// requests creation of a lobby
    /// </summary>
    public void OnClickCreateLobby()
    {
        Messages.SendCreateLobbyMessage();
    }

    /// <summary>
    /// requests joining a lobby
    /// </summary>
    /// <param name="lobby"></param>
    public void OnClickLobbyJoin(LobbyPreJoinInfo lobby)
    {
        Messages.SendLobbyJoinMessage(lobby);
    }

    /// <summary>
    /// sends a message to the server that the game should be started with the current lobby
    /// called from the start game button
    /// </summary>
    public void OnClickStartGame()
    {
        Messages.SendStartGameMessage();
    }

    #endregion
}