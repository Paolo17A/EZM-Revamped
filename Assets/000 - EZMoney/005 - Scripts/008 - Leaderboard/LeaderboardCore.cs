using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardCore : MonoBehaviour
{
    //=========================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private List<PlacementController> Placements;
    //=========================================================

    public void InitializeLeaderboard()
    {
        if(GameManager.Instance.DebugMode)
        {
            foreach(PlacementController placement in Placements)
                placement.gameObject.SetActive(false);

            Placements[0].gameObject.SetActive(true);
            Placements[0].NameTMP.text = PlayerData.DisplayName;
            Placements[0].GemTMP.text = PlayerData.LifetimeEZGem.ToString("n0");
        }
    }
}
