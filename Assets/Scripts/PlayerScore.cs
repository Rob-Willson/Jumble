﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerScore : NetworkBehaviour, IReset
{
    public static Action<Player, int> PlayerScoreChange;

    [SerializeField] private int score;
    [SerializeField] private List<int> scoreLog;

    public void Reset ()
    {
        score = 0;
        scoreLog.Clear();
    }

    [Server]
    public void AddToScore (Player myPlayer, int amount)
    {
        Debug.Log("PlayerScore.AddToScore");

        score += amount;
        scoreLog.Add(amount);

        PlayerScoreChange?.Invoke(myPlayer, score);
    }

}