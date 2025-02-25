﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SunnyTown;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An AchievementsManager is responsible for listening to achievement events and notifying 
/// the appropriate views of these events. 
/// </summary>
public class AchievementsManager : MonoBehaviour
{
    public static AchievementsManager Instance { get; private set; }

    private Animator achievementNotificationAnimator;
    private Transform highScoreContainer;
    private Transform achievementsContainer;
    private Transform achievementsTemplate;
    private Transform highScoreTemplate;

    private const string NUMBER_OF_SCORES = "NumberOfScores";
    private const string NUMBER_OF_ACHIEVEMENTS = "NumberOfAchievements";
    private const string HIGH_SCORE = "HighScore";
    private const string PLAYER_NAME = "PlayerName";
    private const string ACHIEVEMENT_DATE = "AchievementDate";
    private const float POPUP_ANIMATION_TIME = 5f;
    private const int HIGH_SCORE_SIZE = 5;

    public static string playerName;
    private int envInARow;
    //private MetricManager metricManager;

    private AchievementsManager()
    {
        envInARow = 0;
    }

    /// <summary>
    /// Should be called to determine if the game is in an appropriate state to gain a new 
    /// achievement for the player. This will also update the views if an achievement has 
    /// been achieved. 
    /// </summary>
    public void IsAchievementMade()
    {
        HandleWinnerAchievement();
        HandleTreeHuggerAchievement();
		HandleHappyTownAchievement();
		HandleGoldDiggerAchievement();
		HandleCaptainPlanetAchievement();
		HandleAllRounderAchievement();
        HandleExclamationMarkClickAchievement();
    }

    private void HandleTreeHuggerAchievement()
    {
        if (MetricManager.Instance.EnvHealth >= MetricManager.Instance.PrevEnvHealth)
        {
            envInARow++;
        }
        else
        {
            envInARow = 0;
        }

        if (envInARow == 5)
        {
            if (!IsAchievementAlreadyEarned("Tree Hugger"))
            {
                PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "Tree Hugger");
                PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
                PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
                DisplayAchievementNotification("Tree Hugger");
            }
        }
    }

    private void HandleExclamationMarkClickAchievement()
    {
        if (ExclamationDispatcher.Instance.clickCount == 5 && !IsAchievementAlreadyEarned("Crowd Pleaser 1"))
        {
            PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "Crowd Pleaser 1");
            PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
            PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
            DisplayAchievementNotification("Crowd Pleaser 1");
        } else if (ExclamationDispatcher.Instance.clickCount == 10 && !IsAchievementAlreadyEarned("Crowd Pleaser 2"))
        {
            PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "Crowd Pleaser 2");
            PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
            PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
            DisplayAchievementNotification("Crowd Pleaser 2");
        } else if (ExclamationDispatcher.Instance.clickCount == 15 && !IsAchievementAlreadyEarned("Crowd Pleaser 3"))
        {
            PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "Crowd Pleaser 3");
            PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
            PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
            DisplayAchievementNotification("Crowd Pleaser 3");
        }
        else
        {
            return;
        }
    }
    private void HandleWinnerAchievement()
    {
        if (CardManager.Instance.GameWon && !IsAchievementAlreadyEarned("Winner"))
        {
            PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "Winner");
            PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
            PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
            DisplayAchievementNotification("Winner");

            if (MetricManager.Instance.PopHappiness < 30 && MetricManager.Instance.Gold < 30 &&
                MetricManager.Instance.EnvHealth < 30 && !IsAchievementAlreadyEarned("Clutch Gamer"))
            {
                PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "Clutch Gamer");
                PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
                PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
                DisplayAchievementNotification("Clutch Gamer");
            }
        }
    }

	private void HandleHappyTownAchievement()
	{
        if (MetricManager.Instance.PopHappiness == 100 && !IsAchievementAlreadyEarned("Happy Town"))
        {
            PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "Happy Town");
            PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
            PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
            DisplayAchievementNotification("Happy Town");
        }
	}

    private void HandleGoldDiggerAchievement()
    {
        if (MetricManager.Instance.Gold == 100 && !IsAchievementAlreadyEarned("Gold Digger"))
        {
            PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "Gold Digger");
            PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
            PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
            DisplayAchievementNotification("Gold Digger");
        }
    }

    private void HandleCaptainPlanetAchievement()
    {
        if (MetricManager.Instance.EnvHealth == 100 && !IsAchievementAlreadyEarned("Captain Planet"))
        {
            PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "Captain Planet");
            PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
            PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
            DisplayAchievementNotification("Captain Planet");
        }
    }

    private void HandleAllRounderAchievement()
    {
        if (MetricManager.Instance.EnvHealth >= 75 && MetricManager.Instance.Gold >= 75 && MetricManager.Instance.PopHappiness >= 75 &&
            !IsAchievementAlreadyEarned("All Rounder"))
        {
            PlayerPrefs.SetString("achievement" + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), "All Rounder");
            PlayerPrefs.SetString(ACHIEVEMENT_DATE + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS), DateTime.Today.ToShortDateString());
            PlayerPrefs.SetInt(NUMBER_OF_ACHIEVEMENTS, PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + 1);
            DisplayAchievementNotification("All Rounder");
        }
    }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Checks if the new score is within the high score list, if it is, then add it to the list.
    /// </summary>
    /// <param name="newScore">Score to be checked to add to the high score list</param>
    /// <returns>Rank of the new high score, -1 if the score is too low to be a high score</returns>
    public static int UpdateHighScores(int newScore)
    {
        var numberOfScores = PlayerPrefs.GetInt(NUMBER_OF_SCORES);
        for (int i = 1; i <= numberOfScores; i++)
        {
            if (newScore > PlayerPrefs.GetInt(HIGH_SCORE + i))
            {
                LoadHighScoreIntoIndex(newScore, i);
                return i;
            }
        }

        if (numberOfScores < HIGH_SCORE_SIZE)
        {
            LoadHighScoreIntoIndex(newScore, numberOfScores + 1);
            return numberOfScores + 1;
        }

        return -1;
    }

    private static void LoadHighScoreIntoIndex(int newScore, int index)
    {
        //move the positions of all scores below the current one down
        for (int i = PlayerPrefs.GetInt(NUMBER_OF_SCORES); i >= index; i--)
        {
            PlayerPrefs.SetInt(HIGH_SCORE + (i + 1), PlayerPrefs.GetInt(HIGH_SCORE + i));
            PlayerPrefs.SetString(PLAYER_NAME + (i + 1), PlayerPrefs.GetString(PLAYER_NAME + i));
        }

        //add in the new score in the indexed position
        PlayerPrefs.SetInt(HIGH_SCORE + index, newScore);
        PlayerPrefs.SetString(PLAYER_NAME + index, playerName);
        PlayerPrefs.SetInt(NUMBER_OF_SCORES, PlayerPrefs.GetInt(NUMBER_OF_SCORES) + 1);

        //if the number of high scores stored are now greater than the maximum, delete the lowest of the high scores
        if (PlayerPrefs.GetInt(NUMBER_OF_SCORES) > HIGH_SCORE_SIZE)
        {
            PlayerPrefs.DeleteKey(HIGH_SCORE + (HIGH_SCORE_SIZE + 1));
            PlayerPrefs.DeleteKey(PLAYER_NAME + (HIGH_SCORE_SIZE + 1));
            PlayerPrefs.SetInt(NUMBER_OF_SCORES, HIGH_SCORE_SIZE);
        }
    }

    /// <summary>
    /// Returns a list of highscore entry to be displayed or inspected.
    /// </summary>
    /// <returns>The list of saved HighScoreEntry objects</returns>
    public List<HighScoreEntry> GetHighScores()
    {
        var highScores = new List<HighScoreEntry>();
        for (var i = 1; i <= PlayerPrefs.GetInt(NUMBER_OF_SCORES); i++)
        {
            var hse = new HighScoreEntry(
                PlayerPrefs.GetInt(HIGH_SCORE + i),
                PlayerPrefs.GetString(PLAYER_NAME + i),
                i);
            highScores.Add(hse);
        }

        return highScores;
    }

    /// <summary>
    /// Updates the AchievementsMenu view to display all high scores for this players instance.
    /// This should only be called from the MainMenuScene or a scene with an AchievementsMenu.
    /// High Scores are stored in player prefs.
    /// </summary>
    public void DisplayHighScores()
    {
        var achievementsView = GameObject.Find("AchievementsMenu");
        highScoreContainer = achievementsView.transform.GetChild(1).GetChild(4).GetComponent<Transform>();
        highScoreTemplate = highScoreContainer.Find("HighScoreTemplate");
        highScoreTemplate.gameObject.SetActive(false);

        float templateHeight = 23f;
        var highScoreList = GetHighScores();
        for (int i = 0; i < highScoreList.Count; i++)
        {
            var entryTransform = Instantiate(highScoreTemplate, highScoreContainer);
            var entryRectTransform = entryTransform.GetComponent<RectTransform>();
            var highScore = entryRectTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var playerName = entryRectTransform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var hsrank = entryRectTransform.GetChild(2).GetComponent<TextMeshProUGUI>();
            var hse = highScoreList.ElementAt(i);
            highScore.SetText(hse.getHighScore().ToString());
            playerName.SetText(hse.getPlayerName());
            hsrank.SetText(hse.getRank().ToString());
            entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * i);
            entryTransform.gameObject.SetActive(true);
        }
    }

    private bool IsAchievementAlreadyEarned(string achievementName)
    {
        for (int i = 0; i < PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS); i++)
        {
            if (PlayerPrefs.GetString("achievement" + i).Equals(achievementName))
            {
                return true;
            }
        }

        return false;
    }

    private Achievement GetAchievementByIndex(int i)
    {
        var achievementName = PlayerPrefs.GetString("achievement" + i);
        foreach (Achievement achievement in Reader.Instance.AllAchievements)
        {
            if (achievement.name.Equals(achievementName))
            {
                achievement.dateEarned = PlayerPrefs.GetString(ACHIEVEMENT_DATE + i);
                return achievement;
            }
        }
        return null;
    }

    /// <summary>
    /// Displays an achievement popup notification with sound. The name and sprite displayed
    /// corresponds to the given achievement name.
    /// </summary>
    /// <param name="achievementName">The achievement to display</param>
    public void DisplayAchievementNotification(string achievementName)
    {
        foreach (Achievement a in Reader.Instance.AllAchievements)
        {
            if (a.name.Equals(achievementName))
            {
                var achievementNotificationView = GameObject.Find("AchievementNotification");
                if (achievementNotificationView != null)
                {
                    achievementNotificationView.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = a.name;
                    achievementNotificationView.transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + a.imageUrl);
                    achievementNotificationAnimator = achievementNotificationView.GetComponent<Animator>();
                    achievementNotificationAnimator.SetBool("IsVisible", true);
                    SFXAudioManager.Instance.PlayAchievementSound();
                    StartCoroutine(WaitForDownAnimation());
                }

            }
        }
    }
    
    private IEnumerator WaitForDownAnimation()
    {
        yield return new WaitForSeconds(POPUP_ANIMATION_TIME);
        achievementNotificationAnimator.SetBool("IsVisible", false);
    }

    /// <summary>
    /// Displays the list of achievements achieved by the player in the achievements menu. 
    /// This should be used in classes with an AchivementsMenu.
    /// The achievements retrieved are from playerprefs.
    /// </summary>
    public void DisplayAchievementsMenu()
    {
        var achievementsView = GameObject.Find("AchievementsMenu");
        achievementsContainer = achievementsView.transform.GetChild(2).GetChild(2).GetChild(0).GetChild(0).GetComponent<Transform>();
        var achievementsCompleted = achievementsView.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
        achievementsCompleted.SetText("Achievements Unlocked: " + PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS) + "/" +
                                      Reader.Instance.AllAchievements.Count);

        achievementsTemplate = achievementsContainer.Find("AchievementsTemplate");
        achievementsTemplate.gameObject.SetActive(false);
        float templateHeight = 34f;
        for (int i = 0; i < PlayerPrefs.GetInt(NUMBER_OF_ACHIEVEMENTS); i++)
        {
            var entryTransform = Instantiate(achievementsTemplate, achievementsContainer);
            var entryRectTransform = entryTransform.GetComponent<RectTransform>();
            var badge = entryRectTransform.GetChild(0).GetComponent<Image>();
            var description = entryRectTransform.GetChild(1).GetComponent<TextMeshProUGUI>();
            var date = entryRectTransform.GetChild(2).GetComponent<TextMeshProUGUI>();
            Achievement achievement = GetAchievementByIndex(i);
            description.SetText(achievement.name + " - " + achievement.description);
            date.SetText("Date achieved: " + achievement.dateEarned);
            badge.sprite = Resources.Load<Sprite>("Sprites/" + achievement.imageUrl);
            entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * i);
            entryTransform.gameObject.SetActive(true);
        }
    }

    public class HighScoreEntry
    {
        internal HighScoreEntry(int highScore, string playername, int rank)
        {
            this.highScore = highScore;
            this.playername = playername;
            this.rank = rank;
        }

        private int highScore;
        private string playername;
        private int rank;

        internal int getHighScore()
        {
            return this.highScore;
        }

        internal string getPlayerName()
        {
            return this.playername;
        }

        internal int getRank()
        {
            return this.rank;
        }
    }
}
