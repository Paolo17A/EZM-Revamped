using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using PlayFab.ClientModels;
using PlayFab;
using System;

public class PurchaseCharacterController : MonoBehaviour
{
    #region VARIABLES
    //=========================================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private ShopCore ShopCore;
    [SerializeField] private CharacterData ThisCharacterData;
    [field: SerializeField] public Button PurchaseBtn { get; set; }

    [Header("CHARACTER STATS")]
    [SerializeField] private TextMeshProUGUI StrengthTMP;
    [SerializeField] private TextMeshProUGUI SpeedTMP;
    [SerializeField] private TextMeshProUGUI StaminaTMP;

    [Header("PLAYFAB VARIABLES")]
    [ReadOnly] private GetUserDataRequest getUserData;
    [ReadOnly] private StartPurchaseRequest startPurchase;
    [ReadOnly] private PayForPurchaseRequest payForPurchase;
    [ReadOnly] private ConfirmPurchaseRequest confirmPurchase;
    [ReadOnly] private ConsumeItemRequest consumeItem;
    [ReadOnly] private int failedCallbackCounter;
    //=========================================================================================================
    #endregion

    private void Awake()
    {
        getUserData = new GetUserDataRequest();
        startPurchase = new StartPurchaseRequest();
        startPurchase.Items = new List<ItemPurchaseRequest>();
        payForPurchase = new PayForPurchaseRequest();
        confirmPurchase = new ConfirmPurchaseRequest();
        consumeItem = new ConsumeItemRequest();
    }

    private void Start()
    {
        StrengthTMP.text = ThisCharacterData.strength.ToString();
        SpeedTMP.text = ThisCharacterData.speed.ToString();
        StaminaTMP.text = ThisCharacterData.stamina.ToString();
    }

    public void PurchaseThisCharacter()
    {
        if(GameManager.Instance.DebugMode)
        {
            if (MayPurchaseCharacter())
            {
                if (PlayerData.EZCoin >= ThisCharacterData.price)
                {
                    for (int i = 0; i < PlayerData.OwnedCharacters.Count; i++)
                    {
                        if (PlayerData.OwnedCharacters[i].BaseCharacterData == null)
                        {
                            PlayerData.OwnedCharacters[i].CharacterInstanceID = "newlyPurchasedCharacter " + i;
                            PlayerData.OwnedCharacters[i].BaseCharacterData = ThisCharacterData;
                            PlayerData.OwnedCharacters[i].CharacterCurrentRole = CharacterInstanceData.Roles.MINER;
                            PlayerData.OwnedCharacters[i].CharacterCurrentState = CharacterInstanceData.States.INVENTORY;
                            PlayerData.OwnedCharacters[i].CharacterCurrentStamina = ThisCharacterData.stamina;
                            break;
                        }
                    }
                    ShopCore.OwnedCharactersCount++;
                    PlayerData.EZCoin -= ThisCharacterData.price;
                    ShopCore.UpdateEZCoinDisplay();
                    ShopCore.CheckCharacterPurchasability();
                    ShopCore.CheckAutopilotPurchasability();
                }
                else
                    GameManager.Instance.DisplayErrorPanel("You do not have enough EZCoins to purchase this character");
            }
            else
                GameManager.Instance.DisplayErrorPanel("You may only own " + ShopCore.OwnedCharactersCount + " characters with a " + PlayerData.SubscriptionLevel + " subscription");
        }
        else
        {
            if(MayPurchaseCharacter())
            {
                ShopCore.DisplayLoadingPanel();
                PlayFabClientAPI.GetUserData(getUserData,
                    resultCallback =>
                    {
                        failedCallbackCounter = 0;
                        if (resultCallback.Data.ContainsKey("LUID") && resultCallback.Data["LUID"].Value == PlayerData.LUID)
                            StartPurchasePlayFab();
                        else
                            GameManager.Instance.DisplayDualLoginErrorPanel();
                    },
                    errorCallback =>
                    {
                        ErrorCallback(errorCallback.Error,
                            PurchaseThisCharacter,
                            () => ProcessError(errorCallback.ErrorMessage));
                    });
            }
            else
                GameManager.Instance.DisplayErrorPanel("You may only own " + ShopCore.OwnedCharactersCount + " own characters with a " + PlayerData.SubscriptionLevel + " subscription");
        }
    }

    private void StartPurchasePlayFab()
    {
        startPurchase.CatalogVersion = "Consumables";
        startPurchase.Items.Clear();
        startPurchase.Items.Add(new ItemPurchaseRequest() { ItemId = "PurchaseAnimalStub", Quantity = 1 });
        PlayFabClientAPI.StartPurchase(startPurchase,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PayForPurchase(resultCallback.OrderId, resultCallback.PaymentOptions[0].ProviderName);
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    StartPurchasePlayFab,
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void PayForPurchase(string _orderID, string _providerName)
    {
        payForPurchase.Currency = "EC";
        payForPurchase.OrderId = _orderID;
        payForPurchase.ProviderName = _providerName;

        PlayFabClientAPI.PayForPurchase(payForPurchase,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                ConfirmPurchase(_orderID);
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => PayForPurchase(_orderID, _providerName),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void ConfirmPurchase(string _orderID)
    {
        confirmPurchase.OrderId = _orderID;

        PlayFabClientAPI.ConfirmPurchase(confirmPurchase,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                ConsumeAnimalStub(resultCallback.Items[0].ItemInstanceId);
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => ConfirmPurchase(_orderID),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void ConsumeAnimalStub(string stubID)
    {
        consumeItem.ItemInstanceId = stubID;
        consumeItem.ConsumeCount = 1;
        PlayFabClientAPI.ConsumeItem(consumeItem,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                GrantCharacterCloudscript();
            },
            errorCallback =>
            {
                ErrorCallback(errorCallback.Error,
                    () => ConsumeAnimalStub(stubID),
                    () => ProcessError(errorCallback.ErrorMessage));
            });
    }

    private void GrantCharacterCloudscript()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "GrantNewCharacter",
            FunctionParameter = new
            {
                AnimalID = ThisCharacterData.animalID,
                Strength = ThisCharacterData.strength,
                Speed = ThisCharacterData.speed,
                Stamina = ThisCharacterData.stamina
            },
            GeneratePlayStreamEvent = true
        },
        resultCallback =>
        {
            failedCallbackCounter = 0;
            //PlayerData.EZCoin -= 2000;
            ShopCore.GetUserInventoryPlayfab();
            ShopCore.ListAllCharactersPlayFab();
        },
        errorCallback =>
        {
            ErrorCallback(errorCallback.Error,
                GrantCharacterCloudscript,
                () => ProcessError(errorCallback.ErrorMessage));
        });
    }


    #region UTILITY
    private bool MayPurchaseCharacter()
    {
        if ((PlayerData.SubscriptionLevel == "PEARL" && ShopCore.OwnedCharactersCount >= 5) || 
            (PlayerData.SubscriptionLevel == "TOPAZ" && ShopCore.OwnedCharactersCount >= 10) ||
            (PlayerData.SubscriptionLevel == "SAPPHIRE" && ShopCore.OwnedCharactersCount >= 15) ||
            (PlayerData.SubscriptionLevel == "EMERALD" && ShopCore.OwnedCharactersCount >= 20) ||
            (PlayerData.SubscriptionLevel == "RUBY" && ShopCore.OwnedCharactersCount >= 25) ||
            (PlayerData.SubscriptionLevel == "DIAMOND" && ShopCore.OwnedCharactersCount >= 30))
            return false;
        else
            return true;
    }
    private void ErrorCallback(PlayFabErrorCode errorCode, Action restartAction, Action errorAction)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                ProcessError("Connectivity error. Please connect to strong internet");
            else
                restartAction();
        }
        else if (errorCode == PlayFabErrorCode.InternalServerError)
            ProcessSpecialError();
        else
            errorAction();
    }

    private void ProcessError(string errorMessage)
    {
       ShopCore.HideLoadingPanel();
        GameManager.Instance.DisplayErrorPanel(errorMessage);
    }

    private void ProcessSpecialError()
    {
        ShopCore.HideLoadingPanel();
        GameManager.Instance.DisplaySpecialErrorPanel("Server Error. Please restart the game");
    }
    #endregion
}
