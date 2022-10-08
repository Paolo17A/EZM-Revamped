using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Security.Cryptography;
using System.Text;
using System;

public class LoginCore : MonoBehaviour
{
    //====================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private EntryCore EntryCore;

    private int failedCallbackCounter;
    private Guid myGUID;
    private RegisterPlayFabUserRequest RegisterPlayFabUser;
    private LoginWithPlayFabRequest loginWithPlayFab;
    private GetUserDataRequest getUserData;
    private UpdateUserDataRequest updateUserData;

    //====================================================================================

    private void Awake()
    {
        RegisterPlayFabUser = new RegisterPlayFabUserRequest();
        loginWithPlayFab = new LoginWithPlayFabRequest();
        updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>();
    }
    public void RegisterNewUserPlayfab()
    {
        EntryCore.DisplayLoadingPanel();
        RegisterPlayFabUser.Email = "test@gmail.com";
        RegisterPlayFabUser.Username = "test123";
        RegisterPlayFabUser.DisplayName = "test123";
        RegisterPlayFabUser.Password = "password";

        PlayFabClientAPI.RegisterPlayFabUser(RegisterPlayFabUser,
            resultCallback =>
            {
                RegisterUserInitialization(RegisterPlayFabUser.Password);
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    FailedAction,
                    RegisterNewUserPlayfab,
                    () => GameManager.Instance.DisplayErrorPanel(errorCallback.ErrorMessage));
            });
    }
    private void RegisterUserInitialization(string rawPassword)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "RegistrationUserDataInitialization",
            FunctionParameter = new { encryptedPassword = Encrypt(rawPassword) }
        },
        resultCallback =>
        {
            EntryCore.HideLoadingPanel();
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                    FailedAction,
                    () => RegisterUserInitialization(rawPassword),
                    () => GameManager.Instance.DisplayErrorPanel(errorCallback.ErrorMessage));
        });
    }
    public void LoginWithPlayFab(string username, string password)
    {
        EntryCore.DisplayLoadingPanel();
        loginWithPlayFab.Username = username;
        loginWithPlayFab.Password = "password";

        PlayFabClientAPI.LoginWithPlayFab(loginWithPlayFab,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.PlayfabID = resultCallback.PlayFabId;
                PlayerData.DisplayName = username;
                ProcessUserData(password);
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                   FailedAction,
                   () => LoginWithPlayFab(username, password),
                   () => GameManager.Instance.DisplayErrorPanel(errorCallback.ErrorMessage));
            });
    }

    private void ProcessUserData(string rawPassword)
    {
        PlayFabClientAPI.GetUserData(getUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Data.ContainsKey("EncryptedPassword") && resultCallback.Data["EncryptedPassword"].Value != Encrypt(rawPassword))
                {
                    EntryCore.HideLoadingPanel();
                    GameManager.Instance.DisplayErrorPanel("Incorrect Password");
                }
                else if (resultCallback.Data.ContainsKey("SubscriptionLevel") && resultCallback.Data["SubscriptionLevel"].Value == "UNPAID")
                {
                    EntryCore.HideLoadingPanel();
                    GameManager.Instance.DisplayErrorPanel("You have not yet subscribed");
                }
                else
                    UpdateLUID();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    FailedAction,
                    () => ProcessUserData(rawPassword),
                    () => GameManager.Instance.DisplayErrorPanel(errorCallback.ErrorMessage));
            });
    }

    private void UpdateLUID()
    {
        myGUID = Guid.NewGuid();
        updateUserData.Data.Clear();
        updateUserData.Data.Add("LUID", myGUID.ToString());
        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.LUID = myGUID.ToString();
                PlayerPrefs.SetString("Username", EntryCore.UsernameLoginTMP.text);
                PlayerPrefs.SetString("Password", EntryCore.PasswordLoginTMP.text);
                GameManager.Instance.SceneController.CurrentScene = "LobbyScene";
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    FailedAction,
                    UpdateLUID,
                    () => GameManager.Instance.DisplayErrorPanel(errorCallback.ErrorMessage));
            });
    }

    #region UTILITY
    private string Encrypt(string _password)
    {
        string salt = "CBS";
        string pepper = "EZMONEY";

        using (SHA256 hash = SHA256.Create())
        {
            byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(_password));

            // Convert byte array to a string   
            StringBuilder firstHash = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                firstHash.Append(bytes[i].ToString("x2"));
            }

            bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(firstHash + salt));
            StringBuilder secondHash = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                secondHash.Append(bytes[i].ToString("x2"));
            }

            bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(secondHash + pepper));
            StringBuilder thirdHash = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                thirdHash.Append(bytes[i].ToString("x2"));
            }

            return thirdHash.ToString();
        }
    }

    private void ErrorCallback(PlayFabErrorCode errorCode, Action failedAction, Action restartAction, Action processError)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                failedAction();
            else
                restartAction();
        }
        else if (errorCode == PlayFabErrorCode.InternalServerError)
            ProcessSpecialError();
        else
        {
            if (processError != null)
                processError();
        }
    }

    private void FailedAction()
    {
        EntryCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel("Connectivity error. Please connect to strong internet");
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        EntryCore.ResetLoginPanel();
    }

    private void ProcessError(string error)
    {
        EntryCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(error);
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        EntryCore.ResetLoginPanel();
    }

    private void ProcessSpecialError()
    {
        EntryCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
        PlayFabClientAPI.ForgetAllCredentials();
        PlayerData.ResetPlayerData();
        EntryCore.ResetLoginPanel();
    }
    #endregion
}
