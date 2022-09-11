using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class SwapCore : MonoBehaviour
{
    [SerializeField] private ProfileCore ProfileCore;
    [SerializeField] private PlayerData PlayerData;

    [Header("INPUT FIELDS")]
    [SerializeField] private VerticalLayoutGroup VirtualCurrencyContainer;
    [SerializeField] private TMP_InputField EZCoinTMP;
    [SerializeField] private TMP_InputField EZGemTMP;
    [SerializeField] private Button SwapBtn;

    [Header("DEBUGGER")]
    [SerializeField][ReadOnly] private bool willSwapEZGem;

    public void InterchangeInput()
    {
        EZGemTMP.text = "";
        EZCoinTMP.text = "";
        willSwapEZGem = !willSwapEZGem;

        if (willSwapEZGem)
        {
            EZGemTMP.interactable = true;
            EZCoinTMP.interactable = false;
            VirtualCurrencyContainer.reverseArrangement = true;
        }
        else
        {
            EZGemTMP.interactable = false;
            EZCoinTMP.interactable = true;
            VirtualCurrencyContainer.reverseArrangement = false;
        }
    }

    public void DisplayTakehomeEZCoin()
    {
        if (EZGemTMP.text == "")
        {
            EZCoinTMP.text = "";
            SwapBtn.interactable = false;
        }
        else
        {
            if (int.Parse(EZGemTMP.text) > PlayerData.EZGem)
            {
                EZGemTMP.text = "";
                GameManager.Instance.DisplayErrorPanel("Input must not exceed " + PlayerData.EZGem);
            }
            else if (int.Parse(EZGemTMP.text) < 1)
            {
                EZGemTMP.text = "";
                GameManager.Instance.DisplayErrorPanel("Input must be at least 1 EZGem");
            }
            else
            {
                EZCoinTMP.text = (int.Parse(EZGemTMP.text) * 95).ToString();
                SwapBtn.interactable = true;
            }
        }
    }

    public void DisplayTakehomeEZGem()
    {
        if (EZCoinTMP.text == "")
        {
            EZGemTMP.text = "";
            SwapBtn.interactable = false;
        }
        else
        {
            if (int.Parse(EZCoinTMP.text) > PlayerData.EZCoin)
            {
                EZCoinTMP.text = "";
                GameManager.Instance.DisplayErrorPanel("Input must not exceed " + PlayerData.EZCoin);
            }
            else if (int.Parse(EZCoinTMP.text) < 105)
            {
                EZCoinTMP.text = "";
                GameManager.Instance.DisplayErrorPanel("Input must be at least 105 EZCoin");
            }
            else
            {
                EZGemTMP.text = (int.Parse(EZCoinTMP.text) / 105).ToString();
                SwapBtn.interactable = true;
            }
        }
    }

    public void InputMaxValue()
    {
        if (willSwapEZGem)
        {
            EZGemTMP.text = PlayerData.EZGem.ToString();
            DisplayTakehomeEZCoin();
        }
        else
        {
            EZCoinTMP.text = PlayerData.EZCoin.ToString();
            DisplayTakehomeEZGem();
        }
    }

    public void SwapCurrencies()
    {
        if (!willSwapEZGem && int.Parse(EZCoinTMP.text) % 105 != 0)
            Debug.Log("You will have an excess of " + (int.Parse(EZCoinTMP.text) % 105) + "EZGems");
        else
            Debug.Log("You will have no excess EZCoins");


        if (GameManager.Instance.DebugMode)
        {
            if (willSwapEZGem)
            {
                PlayerData.EZGem -= int.Parse(EZGemTMP.text);
                PlayerData.EZCoin += int.Parse(EZCoinTMP.text);
            }
            else
            {
                PlayerData.EZGem += int.Parse(EZGemTMP.text);
                PlayerData.EZCoin -= int.Parse(EZCoinTMP.text) - (int.Parse(EZCoinTMP.text) % 105);
            }

            ProfileCore.EZCoinsTMP.text = PlayerData.EZCoin.ToString();
            ProfileCore.EZGemsTMP.text = PlayerData.EZGem.ToString();
            EZCoinTMP.text = "";
            EZGemTMP.text = "";
            SwapBtn.interactable = false;
        }
    }
}
