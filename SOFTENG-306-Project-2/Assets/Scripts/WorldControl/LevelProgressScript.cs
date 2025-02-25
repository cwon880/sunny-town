﻿﻿using SunnyTown;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A LevelProgress Script is used on a game object to render a plot progress value 
/// within a Slider. The game object should be updated by other modules
/// to receive the progress updates.
/// </summary>
public class LevelProgressScript : MonoBehaviour
{
    [SerializeField]
    private Slider levelProgressBar;
    [SerializeField]
    private TextMeshProUGUI daysRemainingText;
    private LevelControl levelController;
    private const float LEVEL_ONE_CAP = 4f;
    private const float LEVEL_TWO_CAP = 9f;
    private const float LEVEL_THREE_CAP = 15f;

    private void Start()
    {
        levelController = GameObject.Find("LevelManager").GetComponent<LevelControl>();
    }

    /// <summary>
    /// Updates the progress displayed on the slider game object owning this script. 
    /// </summary>
    /// <param name="card">The plot card from which to render the plot progress</param>
    internal void UpdateValue(PlotCard card)
    {
        var cardIdString = Regex.Match(card.Id, @"\d+").Value;
        var cardIdNumber = float.Parse(cardIdString);
        Debug.Log("On plot card: " + cardIdNumber);
        float levelProgress;
        if (cardIdNumber <= LEVEL_ONE_CAP)
        {
            levelProgress = (cardIdNumber / LEVEL_ONE_CAP); 
            levelProgressBar.value =  levelProgress / 3;
        }
        else if ( cardIdNumber <= LEVEL_TWO_CAP)
        {
            levelProgress = cardIdNumber / LEVEL_TWO_CAP;
            levelProgressBar.value = levelProgress * 2/3;
        }
        else
        {
            levelProgress = cardIdNumber / LEVEL_THREE_CAP;
            levelProgressBar.value = levelProgress;
        }

        if (cardIdNumber == LEVEL_ONE_CAP || cardIdNumber == LEVEL_TWO_CAP)
        {
            levelController.NextLevel();
        }
    }
}
