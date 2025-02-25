using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SunnyTown
{

    /// <summary>
    /// A CardManager maintains the state of cards and makes calls to the DialogueManager
    /// to render the appropriates Cards.
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        private static float WAITING_FOR_FEEDBACK_DURATION = 0.1f;

        // TODO: will need to change this later depending on playtesting
        private float waitingForEventsDuration = 2.5f;
        private const int MINOR_CARDS_PER_PLOT_CARD = 1;
        private int START_CAMPAIGN_CARD_NUMBER = 6;
        private float waitingForFeedbackDuration = WAITING_FOR_FEEDBACK_DURATION;
        private const string FINAL_LEVEL_ID = "s15";

        public static CardManager Instance { get; private set; }
        public GameObject spawnHandlerObject;
        private CardFactory cardFactory;
        private DialogueManager dialogueManager;
        private MetricManager metricManager;
        private DialogueMapper dialogueMapper;
        private SpawnHandler animationHandler;
        private LevelProgressScript levelProgress;
        private SimpleDialogue endGameDialogue;
        private int cardCount = 0;
        private TownAudioClipSwitch townAudioClipSwitch;


        public bool GameWon { get; private set; } = false;
        public bool GameLost { get; private set; } = false;
        public GameState CurrentGameState { get; private set; } = GameState.GameStarting;

        private HashSet<Card> storyCardsTravelled = new HashSet<Card>();
        public Dictionary<string, string> PastTokens = new Dictionary<string, string>();
        private Card currentCard;
        private float timeRemainingInCurrentState = float.PositiveInfinity;
        private bool hadCampaign = false;

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
            townAudioClipSwitch = GameObject.Find("TownAudioClipSwitch").GetComponent<TownAudioClipSwitch>();
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

        /// <summary>
        /// A GameState represents the state of the game and cards. GameState should be used 
        /// to determine whether or not the game is in an appropriate state to perform some action.
        /// </summary>
        public enum GameState
        {

            GameStarting,
            GamePaused,
            WaitingForEvents,
            SelectingPlotDecision,
            SelectingMinorDecision,
            WaitingForFeedback,
            ViewingFeedback,
            GameEnding,
            WeatherEvent
        }

        /// <summary>
        /// Sets the current state of the game, changing the state of the game 
        /// updates the view with the new state.
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
                    DisplayAnyCard();
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
                case GameState.GamePaused:
                    timeRemainingInCurrentState = float.PositiveInfinity;
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
                    DisplayWeatherCard();
                    break;
            }
        }

        private void Update()
        {
            timeRemainingInCurrentState -= Time.deltaTime;
            if (timeRemainingInCurrentState <= 0)
                MoveToNextState();
        }

        /// <summary>
        /// Moves the Game state to the next state depending on the current state. 
        /// This method sshould not contain any logic updating the view. The SetState
        /// method is used to perform this instead.
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
            if (GameWon || GameLost)
            {
                SetState(GameState.GameEnding);
            }
            else
            {
                SetState(GameState.SelectingPlotDecision);
            }
        }

        private void TransitionFromSelectingDecision()
        {
            if (string.IsNullOrEmpty(currentCard.Feedback))
            {
                metricManager.RenderMetrics();
                AchievementsManager.Instance.IsAchievementMade();
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
            var endGameImage = GameObject.Find("EndGameImage").GetComponent<Image>();
            GameObject.Find("MetricPanel").SetActive(false);
            GameObject.Find("LevelProgressPanel").SetActive(false);
            GameObject.Find("PauseButton").SetActive(false);
            if (GameWon)
            {
                townAudioClipSwitch.PlayWinScreenMusic();
                SimpleDialogue endGameDialogue = LastCardDialogue.CreateFinalDialogue(PastTokens);
                Sprite sprite;
                if (MetricManager.Instance.ScoreLow())
                {
                    sprite = Resources.Load<Sprite>("Sprites/BadWinCutscene");
                }
                else
                {
                    //TODO: change to new image when finished art
                    sprite = Resources.Load<Sprite>("Sprites/GoodWinCutscene");
                }
                endGameImage.sprite = sprite;
                this.endGameDialogue = endGameDialogue;
                StartCoroutine(FadeInCutScene(endGameImage));
                dialogueManager.StartCutsceneDialogue(
                    this.endGameDialogue.Statements,
                    () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
            }
            else if (GameLost)
            {
                townAudioClipSwitch.PlayLoseScreenMusic();
                Sprite sprite = Resources.Load<Sprite>("Sprites/LoseCutscene");
                endGameImage.sprite = sprite;
                StartCoroutine(FadeInCutScene(endGameImage));
                dialogueManager.StartCutsceneDialogue(
                    this.endGameDialogue.Statements,
                    () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1));
            }
        }

        private static IEnumerator FadeInCutScene(Image image)
        {
            float elapsedTime = 0.0f;
            Color c = image.color;
            while (elapsedTime < 1)
            {
                yield return null;
                elapsedTime += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsedTime / 1);
                image.color = c;
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
            if (!(currentCard is SliderCard) && PastTokens.ContainsKey(currentCard.Options[decisionValue].AdditionalState))
            {
                Debug.Log("addition state added: " + PastTokens[currentCard.Options[decisionValue].AdditionalState]);
                currentCard.HandleDecision(decisionValue, PastTokens[currentCard.Options[decisionValue].AdditionalState]);
            }
            else
            {
                currentCard.HandleDecision(decisionValue);
            }

            storyCardsTravelled.Add(currentCard);

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
                GameWon = true;
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
                waitingForFeedbackDuration = WAITING_FOR_FEEDBACK_DURATION;
            }

            if (currentCard is PlotCard)
            {
                levelProgress.UpdateValue((PlotCard)currentCard);
            }
            MoveToNextState();
        }

        /// <summary>
        /// Shows the feedback card to the user based on their decision
        /// </summary>
        private void ShowFeedback()
        {
            metricManager.RenderMetrics();
            AchievementsManager.Instance.IsAchievementMade();
            dialogueManager.StartExplanatoryDialogue(dialogueMapper.FeedbackToDialogue(currentCard.Feedback, currentCard.FeedbackNPCName), MoveToNextState);
        }

        /// <summary>
        /// Displays a Plot Card if there is one
        /// </summary>
        private void DisplayAnyCard()
        {
            int currentStateInt = int.Parse(Regex.Match(cardFactory.CurrentPlotCard.Id, @"\d+").Value);
            if (!hadCampaign && currentStateInt == START_CAMPAIGN_CARD_NUMBER)
            {
                hadCampaign = true;
                GameObject.Find("CampaignManager").GetComponent<CampaignManager>().StartCampaignDialogue();
            }
            else
            {
                currentCard = cardCount++ % MINOR_CARDS_PER_PLOT_CARD == 0 ? cardFactory.GetNewCard("story") : cardFactory.GetNewCard("minor");
                if (currentCard is SliderCard)
                {
                    dialogueManager.StartSliderOptionDialogue(dialogueMapper.SliderCardToSliderOptionDialogue((SliderCard)currentCard), HandleOptionPressed);
                }
                else
                {
                    dialogueManager.StartBinaryOptionDialogue(dialogueMapper.CardToOptionDialogue(currentCard), HandleOptionPressed);
                }
            }

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
                    dialogueManager.StartBinaryOptionDialogue(dialogueMapper.CardToOptionDialogue(currentCard), HandleOptionPressed);
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
                if (((PlotCard)currentCard).Id == FINAL_LEVEL_ID)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called when in WeatherEvent state, it displays a weather card to the player and makes a call to 
        /// decrease player health after player clicks continue;
        /// </summary>
        private void DisplayWeatherCard()
        {
            String weatherEvent = "";
            switch (WeatherController.Instance.currentEvent)
            {
                case WeatherController.ClimateEvent.AcidRain:
                    weatherEvent = "acid rain";
                    break;

                case WeatherController.ClimateEvent.Hurricane:
                    weatherEvent = "hurricane";
                    break;

                case WeatherController.ClimateEvent.Smog:
                    weatherEvent = "smog";
                    break;

                case WeatherController.ClimateEvent.WildFire:
                    weatherEvent = "wildfire";
                    break;
            }
            string statement = "Your town has been struck by " + weatherEvent + "! Try raise your environment health to avoid more disasters";
            string[] statements = { statement };
            Action displayWeatherInfo = () =>
            {
                WeatherController.Instance.StopAnim();
                Debug.Log("Clicked continue on weather event");
                SetState(GameState.WaitingForEvents);
                //TODO: balance numbers on event
                MetricsModifier modifier = new MetricsModifier(-4, -3, 0);
                modifier.Modify();
                metricManager.RenderMetrics();
                WeatherController.Instance.probability = 0;
                Debug.Log("new prob " + WeatherController.Instance.probability);
            };

            // minor card should be displayed upon the callback to the mail message
            dialogueManager.StartExplanatoryDialogue(new SimpleDialogue(statements, weatherEvent), displayWeatherInfo);

        }

        /// <summary>
        /// Sets the next plot card to be the start of level two.
        /// </summary>
        /// <returns>The card which transfers to level two</returns>
        public Card SetLevelTwo()
        {
            var card = (PlotCard) cardFactory.GetLevelTwoCard();
            card.NextStateId = "s5";
            SetCheatcodePath();
            return card;
        }

        /// <summary>
        /// Sets the next plot card to be the start of level three.
        /// </summary>
        /// <returns>The card which transfers to level three</returns>
        public Card SetLevelThree()
        {
            var card = (PlotCard) cardFactory.GetLevelThreeCard();
            card.NextStateId = "s10EV";
            SetCheatcodePath();
            return card;
        }

        /// <summary>
        /// Sets the decision-tree as a set of default ideal decisions
        /// </summary>
        private void SetCheatcodePath()
        {
            PastTokens.Clear();
            PastTokens.Add("investment", "EV");
            PastTokens.Add("arvio", "no");
        }


    }
}