using UnityEngine;

namespace SunnyTown
{
    /// <summary>
    /// A MetricsModifier object is used in a Transition object and is responsible for changing
    /// the Metrics of the MetricsManager singleton when .Modify() is called.
    /// </summary>
    public class MetricsModifier
    {
        private int EnvHealthModifier { get; set; }
        private int PopHappinessModifier { get; set; }
        private int GoldModifier { get; set; }

        public MetricsModifier(int popHappinessModifier, int goldModifier, int envHealthModifier)
        {
            this.PopHappinessModifier = popHappinessModifier;
            this.GoldModifier = goldModifier;
            this.EnvHealthModifier = envHealthModifier;

        }

        public void Modify()
        {
            Debug.Log("Modifying metrics: pop: " + PopHappinessModifier + " gold: " + GoldModifier + " envHealth: " + EnvHealthModifier);
            MetricManager.Instance.UpdateGold(GoldModifier);
            MetricManager.Instance.UpdateEnvHealth(EnvHealthModifier);
            MetricManager.Instance.UpdatePopHappiness(PopHappinessModifier);
        }
    }
}