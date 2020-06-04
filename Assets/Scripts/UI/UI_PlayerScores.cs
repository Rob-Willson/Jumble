﻿using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class UI_PlayerScores : NetworkBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> allPlayerScoreTexts = new List<TextMeshProUGUI>();

    private void OnEnable()
    {
        NetworkManagerJumble.ClientJoinedServer += UpdatePlayerScores;
        PlayerScore.PlayerScoreChange += UpdatePlayerScores;
        PlayerScreenName.PlayerScreenNameChange += UpdatePlayerScores;
    }
    private void OnDisable()
    {
        NetworkManagerJumble.ClientJoinedServer -= UpdatePlayerScores;
        PlayerScore.PlayerScoreChange -= UpdatePlayerScores;
        PlayerScreenName.PlayerScreenNameChange -= UpdatePlayerScores;
    }

    private void UpdatePlayerScores()
    {
        for(int i = 0; i < allPlayerScoreTexts.Count; i++)
        {
            if(i >= PlayerManager.instance.allConnectedPlayers.Count)
            {
                allPlayerScoreTexts[i].transform.parent.gameObject.SetActive(false);
                continue;
            }

            PlayerInstance playerInstance = PlayerManager.instance.allConnectedPlayers[i];
            if(playerInstance == null)
            {
                Debug.Log("PlayerInstance could not be found on connection. Bug?");
                allPlayerScoreTexts[i].transform.parent.gameObject.SetActive(false);
                continue;
            }

            allPlayerScoreTexts[i].SetText(playerInstance.ScreenName.screenName + " :  " + playerInstance.Score.score);
            allPlayerScoreTexts[i].transform.parent.gameObject.SetActive(true);
        }
    }

}
