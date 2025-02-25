using System.Collections.Generic;
using UnityEngine;

namespace SunnyTown
{
    /// <summary>
    /// A MinorCard represents a card that does not have any impact on the plot state.
    /// However they still impact the metrisc.
    /// </summary>
    public class MinorCard : Card
    {
        public MinorCard(string[] precedingDialogue, string name, string dialogue, List<Transition> options)
        {
            PrecedingDialogue = precedingDialogue;
            NPCName = name;
            Question = dialogue;
            Options = options;
        }

        public override void HandleDecision(int decisionIndex, string additionalState = "")
        {
            if (Options.Count >= decisionIndex + 1)
            {
                Options[decisionIndex].MetricsModifier.Modify();
                Feedback = Options[decisionIndex].Feedback;
                FeedbackNPCName = Options[decisionIndex].FeedbackNPCName;
                ShouldAnimate = Options[decisionIndex].HasAnimation;
                BuildingName = Options[decisionIndex].BuildingName;
            }
        }
    }
}
