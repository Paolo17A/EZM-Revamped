using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestCore : MonoBehaviour
{
    //=======================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private LobbyCore LobbyCore;

    [Header("UI ELEMENTS")]
    [SerializeField] private Slider QuestProgressSlider;
    [SerializeField] private Button DailyLoginBtn;
    [SerializeField] private Button DailyClaimBtn;
    [SerializeField] private TextMeshProUGUI MinsPlayedTMP;
    [SerializeField] private TextMeshProUGUI CoinsGainedTMP;
    //=======================================================================

    public void InitializeQuestData()
    {
        if(GameManager.Instance.DebugMode)
        {
            QuestProgressSlider.value = 0;

            if(PlayerData.DailyLogin == 0)
                DailyLoginBtn.interactable = true;
            else
            {
                DailyLoginBtn.interactable = false;
                QuestProgressSlider.value += 0.2f;
            }

            if (PlayerData.SocMedShared > 0)
                QuestProgressSlider.value += 0.2f;

            if (PlayerData.AdsWatched > 0)
                QuestProgressSlider.value += 0.2f;

            MinsPlayedTMP.text = PlayerData.MinsPlayed + "/30";
            if (PlayerData.MinsPlayed >= 30)
                QuestProgressSlider.value += 0.2f;

            CoinsGainedTMP.text = PlayerData.CoinsGained + "/100";
            if (PlayerData.CoinsGained >= 100)
                QuestProgressSlider.value += 0.2f;

            ProcessDailyClaimButton();
        }
    }

    public void DailyLogIn()
    {
        if(GameManager.Instance.DebugMode)
        {
            if(PlayerData.DailyLogin == 0)
            {
                PlayerData.DailyLogin++;
                QuestProgressSlider.value += 0.2f;
                ProcessDailyClaimButton();
                Debug.Log("You have logged in for the day");
            }
            else
            {
                Debug.Log("You have already claimed daily logged in");
            }
            DailyLoginBtn.interactable = false;
        }
        else
        {

        }
    }

    public void ShareToSocMed()
    {
        new NativeShare().SetText("Start playing EZMoneyPH!").SetUrl("https://marketplace.optibit.tech/home/customer/dashboard")
            .SetCallback((result, shareTarget) => ProcessShareResult(result))
            .Share();
    }

    private void ProcessShareResult(NativeShare.ShareResult result)
    {
        if (result == NativeShare.ShareResult.Shared)
        {
            PlayerData.SocMedShared++;
            if (GameManager.Instance.DebugMode)
            {
                if(PlayerData.SocMedShared == 1)
                {
                    QuestProgressSlider.value += 0.2f;
                    ProcessDailyClaimButton();
                }
            }
            else
            {

            }
        }
    }

    public void ClaimDailyQuest()
    {
        if(GameManager.Instance.DebugMode)
        {
            if (QuestProgressSlider.value == 1)
            {
                if (PlayerData.DailyClaimed == 0)
                {
                    PlayerData.DailyClaimed++;
                    PlayerData.EZGem++;
                    LobbyCore.EZGemTMP.text = PlayerData.EZGem.ToString("n0");
                }
                else
                    GameManager.Instance.DisplayErrorPanel("You have already claimed today's reward");
                DailyClaimBtn.interactable = false;
            }
            else
                GameManager.Instance.DisplayErrorPanel("You have not yet completed all the quests");
        }
    }

    private void ProcessDailyClaimButton()
    {
        if (QuestProgressSlider.value == 1)
            DailyClaimBtn.interactable = true;
        else
            DailyClaimBtn.interactable = false;
    }
}
