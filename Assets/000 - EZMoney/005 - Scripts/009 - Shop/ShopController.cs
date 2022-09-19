using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;

public class ShopController : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.SceneController.AddActionLoadinList(ShopCore.InitializeShopScene());
        if (!GameManager.Instance.DebugMode)
            GameManager.Instance.SceneController.AddActionLoadinList(ShopCore.GetUserInventory());
        GameManager.Instance.SceneController.ActionPass = true;
        ShopCore.onShopStateChange += ShopStateChange;
    }

    private void OnDisable()
    {
        ShopCore.onShopStateChange -= ShopStateChange;
    }

    private void Awake()
    {
        ShopCore.getUserData = new GetUserDataRequest();
        ShopCore.getUserInventory = new GetUserInventoryRequest();
    }

    private void ShopStateChange(object sender, EventArgs e)
    {
        if (ShopCore.CurrentShopState == ShopCore.ShopStates.CHARACTERS)
            ShopCore.ShowCharactersPanel();
        else if (ShopCore.CurrentShopState == ShopCore.ShopStates.AUTOPILOT)
            ShopCore.ShowAutoPanel();
    }

    [SerializeField] private ShopCore ShopCore;

    public void ShopStateToIndex(int state)
    {
        switch(state)
        {
            case (int)ShopCore.ShopStates.CHARACTERS:
                ShopCore.CurrentShopState = ShopCore.ShopStates.CHARACTERS;
                break;
            case (int)ShopCore.ShopStates.AUTOPILOT:
                ShopCore.CurrentShopState = ShopCore.ShopStates.AUTOPILOT;
                break;
        }
    }
}
