using Conductor.Interfaces;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target Mode that sets targetCards to the card being played.
    /// Useful for bypassing the need for CardTriggerEffects for permanently scaling a card.
    /// </summary>
    public class PlayedCard : CardTargetSelector
    {
        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;

        public override void PreCollectTargets(CardEffectState effectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, bool isTesting)
        {            
            if (cardEffectParams?.playedCard == null)
            {
                Plugin.Logger.LogWarning("Used played_card target mode, but there wasn't a played card.");
                return;
            }

            cardEffectParams.targetCards?.Add(cardEffectParams.playedCard);
        }
    }
}
