using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// a script that is attached to the login button. It sends the login information to the server when clicked
/// </summary>
public class LoginButton : MonoBehaviour {
    //config
    private static readonly int MINIMUM_USERNAME_LENGTH = 3;
    private static readonly int MINIMUM_PASSWORD_LENGTH = 4;

#pragma warning disable 0649
    [SerializeField]
    private InputField UsernameInputField, PasswordInputField;
#pragma warning restore 0649

    /// <summary>
    /// checks for minimum length and then passes the login information to the method which sends the data to the server
    /// </summary>
    public void OnClick()
    {
        String username = UsernameInputField.text;
        String password = PasswordInputField.text;
        if (username.Length >= MINIMUM_USERNAME_LENGTH && password.Length >= MINIMUM_PASSWORD_LENGTH)
        {
            Messages.SendSignInMessage(username, password);
            Debug.Log("LoginButton.OnClick(): sent sign in message.");
        }
    }
}