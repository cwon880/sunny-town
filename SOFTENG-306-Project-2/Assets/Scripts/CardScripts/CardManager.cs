﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SunnyTown
{

    /// <summary>
    /// A CardManager handles the state of cards and makes calls to the DialogueManager
    /// to render the appropriates Cards
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        [SerializeField]
        private float waitingForEventsDuration = 15f;
        [SerializeField]
        private float waitingForFeedbackDuration = 0.5f;

        public static CardManager Instance { get; private set; }
        public GameObject spawnHandlerObject;

        private CardFactory cardFactory;
        private DialogueManager dialogueManager;
        private MetricManager metricManager;
        private DialogueMapper dialogueMapper;
        private SpawnHandler animationHandler;
        private LevelProgressScript levelProgress;
        private SimpleDialogue endGameDialogue;

        public bool LevelWon { get; private set; } = false;
        public bool GameLost { get; private set; } = false;
        public bool EndOfDay { get; set; } = false;

        public GameState CurrentGameState { get; private set; } = GameState.GameStarting;
        public Dictionary<string, string> PastTokens = new Dictionary<string, string>();
        private Card currentCard;
        private float timeRemainingInCurrentState = float.PositiveInfinity;

        // Start is called before the first frame update
        void Start()
        {
            cardFactory = new CardFactory();
            currentCard = cardFactory.CurrentPlotCard;
            dialogueManager = DialogueManager.Instance;
            metricManager = MetricManager.Instance;
            dialogueMapper = new DialogueMapper();
            animationHandler = spawnHandlerObject.GetComponent<SpawnHandler>();
            levelProgress = GameObject.Find("LevelProgress").GetComponent<LevelProgressScript>();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public enum GameState
        {
            GameStarting,
            WaitingForEvents,
            SelectingPlotDecision,
            SelectingMinorDecision,
            WaitingForFeedback,
            ViewingFeedback,
            DayEnding,
            GameEnding,
            WeatherEvent
        }

        /// <summary>
        /// Sets the current state of the game, this method also updates the view with the new state.
        /// </summary>
        /// <param name="state">The state to set the game to</param>
        public void SetState(GameState state)
        {
            Debug.Log("Setting State: " + state);
            CurrentGameState = state;
            WeatherController.Instance.CheckGameStatus();
            switch (CurrentGameState)
            {
                case GameState.SelectingPlotDecision:
                    timeRemainingInCurrentState = float.PositiveInfinity;
                    DisplayPlotCard();
                    break;
                case GameState.SelectingMinorDecision:
                    timeRemainingInCurrentState = float.PositiveInfinity;
                    DisplayMinorDecisionCard();
                    break;
                case GameState.ViewingFeedback:
                    timeRemainingInCurrentState = float.PositiveInfinity;
                    ShowFeedback();
                    break;
                case GameState.WaitingForFeedback:
                    timeRemainingInCurrentState = waitingForFeedbackDuration;
                    break;
                case GameState.WaitingForEvents:
                    timeRemainingInCurrentState = waitingForEventsDuration;
                    break;
                case GameState.GameEnding:
                    timeRemainingInCurrentState = float.PositiveInfinity;
                    EndGame();
                    break;
                case GameState.WeatherEvent:
                    timeRemainingInCurrentState = float.PositiveInfinity;
                    Debug.Log("displying weather card rn");
                    DisplayWeatherCard();
                    break;
                case GameState.DayEnding:
                    timeRemainingInCurrentState = float.PositiveInfinity;
                    EndDay();
                    break;
            }
        }

        private void EndDay()
        {
            var clock = GameObject.Find("Clock").GetComponent<Clock>();
            Action resetDay = () =>
            {
                levelProgress.UpdateValue(cardFactory.PlotCardsRemaining, cardFactory.TotalPlotCardsInLevel);
                SetState(GameState.WaitingForEvents);
                EndOfDay = false;
                clock.ResetDay();
            };

            dialogueManager.StartExplanatoryDialogue(new SimpleDialogue(new string[1] { "End of Day" }, "Advisory Board"), resetDay);
        }

        private void Update()
        {
            if (EndOfDay && CurrentGameState == GameState.WaitingForEvents)
            {
                SetState(GameState.DayEnding);
            }

            timeRemainingInCurrentState -= Time.deltaTime;
            if (timeRemainingInCurrentState <= 0)
                MoveToNextState();
        }

        /// <summary>
        /// Moves the Game state to a new state depending on the current state and 
        /// other values, this should not contain any logic updating the view. That 
        /// should be in the SetState method instead.
        /// </summary>
        private void MoveToNextState()
        {
            switch (CurrentGameState)
            {
                case GameState.GameStarting:
                    SetState(GameState.WaitingForEvents);
                    break;
                case GameState.WaitingForEvents:
                    TransitionFromWaitingForEvents();
                    break;
                case GameState.SelectingMinorDecision:
                case GameState.SelectingPlotDecision:
                    TransitionFromSelectingDecision();
                    break;
                case GameState.WaitingForFeedback:
                    SetState(GameState.ViewingFeedback);
                    break;
                case GameState.ViewingFeedback:
                    SetState(GameState.WaitingForEvents);
                    break;
                case GameState.WeatherEvent:
                    SetState(GameState.WaitingForEvents);
                    break;
            }
        }

        private void TransitionFromWaitingForEvents()
        {
            if (LevelWon || GameLost)
            {
                SetState(GameState.GameEnding);
            }
        }

        private void TransitionFromSelectingDecision()
        {
            if (string.IsNullOrEmpty(currentCard.Feedback))
            {
                metricManager.RenderMetrics();
                SetState(GameState.WaitingForEvents);
            }
            else
            {
                SetState(GameState.WaitingForFeedback);
            }

        }

        /// <summary>
        /// Ends the game by displaying dialogue and switching scenes
        /// </summary>
        private void EndGame()
        {
            if (LevelWon)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else if (GameLost)
            {
                dialogueManager.StartExplanatoryDialogue(
                    this.endGameDialogue,
                    () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
            }
        }

        /// <summary>
        /// Queues the dialogue to be played to the user when the current card 
        /// dissappears
        /// </summary>
        /// <param name="endGameDialogue">The dialogue to be played to the user</param>
        public void QueueGameLost(SimpleDialogue endGameDialogue)
        {
            GameLost = true;
            waitingForEventsDuration = 0f;
            this.endGameDialogue = endGameDialogue;
        }

        /// <summary>
        /// Displays a minor card to the user, ensuring not to interrupt any already being
        /// viewed
        /// </summary>
        /// <param name="button">To be destroyed</param>
        public void QueueMinorCard()
        {
            if (CurrentGameState.Equals(GameState.WaitingForEvents))
            {
                Debug.Log("Successfully selected minor card from world!");
                SetState(GameState.SelectingMinorDecision);
                //show exposition dialouge 
            }
            else
            {
                Debug.Log("Not in approriate game state to display minor card");
                // TODO: DisplayWarningDialogue();
            }
            AchievementsManager.Instance.IsAchievementMade();
        }

        /// <summary>
        /// Begins displaying cards to the user
        /// </summary>
        public void StartDisplayingCards()
        {
            MoveToNextState();
        }

        /// <summary>
        /// Handles when a user makes a decision for a card
        /// </summary>
        /// <param name="decisionValue">The value chosen by the user</param>
        private void HandleOptionPressed(int decisionValue)
        {
            // TODO: Handle this more elegantly, maybe move into different methods
            if (!(currentCard is SliderCard) && PastTokens.ContainsKey(currentCard.Options[decisionValue].AdditionalState))
            {
                Debug.Log("addition state added: " + PastTokens[currentCard.Options[decisionValue].AdditionalState]);
                currentCard.HandleDecision(decisionValue, PastTokens[currentCard.Options[decisionValue].AdditionalState]);
            }
            else
            {
                currentCard.HandleDecision(decisionValue);
            }

            if (!(currentCard is SliderCard))
            {
                string key = currentCard.Options[decisionValue].TokenKey;
                string value = currentCard.Options[decisionValue].TokenValue;
                if (!key.Equals(""))
                {
                    PastTokens.Add(key, value);
                }
            }


            if (IsFinalCard(currentCard))
            {
                LevelWon = true;
                waitingForEventsDuration = 0f;
            }

            if (currentCard.ShouldAnimate)
            {
                waitingForFeedbackDuration = 3f;
                dialogueManager.ShowAnimationProgress(waitingForFeedbackDuration);
                animationHandler.PlayAnimation(currentCard.BuildingName, waitingForFeedbackDuration);
                SFXAudioManager.Instance.PlayConstructionSound();
            }
            else
            {
                waitingForFeedbackDuration = 0.5f;
            }
            MoveToNextState();
        }

        /// <summary>
        /// Shows the feedback card to the user based on their decision
        /// </summary>
        private void ShowFeedback()
        {
            metricManager.RenderMetrics();
            dialogueManager.StartExplanatoryDialogue(dialogueMapper.FeedbackToDialogue(currentCard.Feedback, currentCard.FeedbackNPCName), MoveToNextState);
        }

        /// <summary>
        /// Displays a Plot Card if there is one
        /// </summary>
        private void DisplayPlotCard()
        {
            currentCard = cardFactory.GetNewCard("story");
            dialogueManager.StartBinaryOptionDialogue(dialogueMapper.PlotCardToBinaryOptionDialogue((PlotCard)currentCard), HandleOptionPressed);
        }

        /// <summary>
        /// Displays a minor decision card to the user with a mail message appearing before
        /// </summary>
        private void DisplayMinorDecisionCard()
        {
            string[] statements = { "A new message has been addressed to you at the town hall !" };
            Action displayMinorCard = () =>
            {
                currentCard = cardFactory.GetNewCard("minor");
                if (currentCard is MinorCard)
                {
                    dialogueManager.StartBinaryOptionDialogue(dialogueMapper.MinorCardToBinaryOptionDialogue((MinorCard)currentCard), HandleOptionPressed);
                }
                else
                {
                    dialogueManager.StartSliderOptionDialogue(dialogueMapper.SliderCardToSliderOptionDialogue((SliderCard)currentCard), HandleOptionPressed);
                }
            };

            // minor card should be displayed upon the callback to the mail message
            dialogueManager.StartExplanatoryDialogue(new SimpleDialogue(statements, "You have mail"), displayMinorCard);
        }

        /// <summary>
        /// Returns whether or not the Card is the final card
        /// </summary>
        /// <param name="currentCard">The card to assert finality of</param>
        /// <returns>True if the current card is a final card of the story tree, otherwise false</returns>
        private bool IsFinalCard(Card currentCard)
        {
            // Game is ended on story cards with no transitions

            if (currentCard is PlotCard)
            {
                if (((PlotCard)currentCard).NextStateId == null)
                {
                    return true;
                }
            }
            return false;
        }

        private void DisplayWeatherCard()
        {
            string[] statements = { "Your town has been struck by a weather event!" };
            Action displayWeatherInfo = () =>
            {
                WeatherController.Instance.StopAnim();
                Debug.Log("Clicked continue on weather event");
                SetState(GameState.WaitingForEvents);
                MetricsModifier modifier = new MetricsModifier(-10,-20,0);
                modifier.Modify();
                metricManager.RenderMetrics();
            };

            // minor card should be displayed upon the callback to the mail message
            dialogueManager.StartExplanatoryDialogue(new SimpleDialogue(statements, "Weather event"), displayWeatherInfo);

        }

    }
}