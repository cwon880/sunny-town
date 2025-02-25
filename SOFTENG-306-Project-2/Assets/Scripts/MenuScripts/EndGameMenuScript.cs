﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SunnyTown
{
    /// <summary>
    /// The EndGameMenuScript is a simple script used for the End Game scene. The user has the
    /// option to replay the game, return to the main menu, and quit the game.
    /// </summary>
    public class EndGameMenuScript : MonoBehaviour
    {

        public GameObject endGameView;

        void Awake()
        {
            var gameScore = endGameView.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>();
            var gameOutcome = endGameView.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            var gameBackground = endGameView.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            AchievementsManager.Instance.IsAchievementMade();
            gameOutcome.SetText(CardManager.Instance.GameWon ? "Game Won" : "Game lost");
            if (CardManager.Instance.GameWon)
            {
                gameOutcome.SetText("Game Won");
                gameBackground.color = new Color32(0, 255, 0, 130);
            }
            else
            {
                gameOutcome.SetText("Game Lost");
                gameBackground.color = new Color32(255, 0, 0, 130);
            }

            int finalScore = MetricManager.Instance.GetScore();
            int highScoreRank = AchievementsManager.UpdateHighScores(finalScore);
            if (highScoreRank != -1)
            {
                gameScore.SetText("High Score Rank: " + highScoreRank + " with " + finalScore);
            }
            else
            {
                gameScore.SetText("Final Score: " + finalScore);
            }
        }

        /// <summary>
        /// Sets the currently displayed screen to be the main menu
        /// </summary>
        public void NavigateToMainMenu()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 2);
        }

        public void QuitApplication()
        {
            Application.Quit();
        }

        /// <summary>
        /// Starts the game again from the beginning of the WorldScene.
        /// </summary>
        public void ReplayGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }
}
