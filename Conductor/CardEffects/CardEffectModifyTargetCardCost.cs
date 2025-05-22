using ShinyShoe.Logging;
using System.Collections;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Generic version of CardEffectModifyCardCost. The original version was hardcoded to target the Hand.
    /// 
    /// The test fails if no cards would get upgraded by this effect.
    /// 
    /// Params:
    ///    ParamInt: Cost modification. Positive will increase. Negative decreases.
    ///    TargetCardType: Cards type to restrict target cards to. Defaults to Spell Cards. Set to CardType.Invalid to disable this filter.
    ///
    /// Example Json
    /// {
    ///   "id": "MakeSpellsInDrawPileMoreExpensive",
    ///   "name": "CardEffectModifyTargetCardCost",
    ///   "target_mode": "draw_pile",
    ///   "target_card_type": "spell",
    ///   "param_int":  1
    /// }
    /// </summary>
    public sealed class CardEffectModifyTargetCardCost : CardEffectBase
    {
        public override bool CanApplyInPreviewMode => false;
        private List<CardState> targets = [];

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Increase Cost Amount", "If negative, decreases cost"),
            };
        }


        public override void Setup(CardEffectState cardEffectState)
        {
            base.Setup(cardEffectState);
            // Sanity checks
            if (!cardEffectState.GetTargetMode().TargetModeIsACardPile() && cardEffectState.GetTargetMode() != TargetMode.Hand && cardEffectState.GetTargetMode() != TargetMode.LastDrawnCard)
            {
                Log.Warning(LogGroups.Gameplay, this.GetType().Name + ": TargetMode " + cardEffectState.GetTargetMode() + " is invalid for this CardEffect.");
            }
        }
        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            var sourceCardState = cardEffectState.GetParentCardState() ?? cardEffectParams.cardTriggeredCharacter?.GetSpawnerCard() ?? cardEffectParams.selfTarget?.GetSpawnerCard();
            FilterTargetCards(cardEffectParams.targetCards, cardEffectState.GetTargetCardType(), sourceCardState, cardEffectState.GetTargetCharacterSubtype());
            return targets.Count > 0;
        }

        private void FilterTargetCards(List<CardState> allCards, CardType filterToCardType, CardState? playedCard, SubtypeData subtypeData)
        {
            targets.Clear();
            foreach (CardState allCard in allCards)
            {
                bool flag = true;
                CharacterData? spawnCharacterData = allCard.GetSpawnCharacterData();
                if (allCard == playedCard)
                {
                    continue;
                }
                if (spawnCharacterData != null && subtypeData != null && !subtypeData.IsNone)
                {
                    flag = spawnCharacterData.GetSubtypes()?.Contains(subtypeData) ?? false;
                }
                if (flag && filterToCardType != CardType.Invalid)
                {
                    flag = allCard.GetCardType() == filterToCardType;
                }
                if (flag)
                {
                    targets.Add(allCard);
                }
            }
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers systemManagers)
        {
            var sourceCardState = cardEffectState.GetParentCardState() ?? cardEffectParams.cardTriggeredCharacter?.GetSpawnerCard() ?? cardEffectParams.selfTarget?.GetSpawnerCard();
            FilterTargetCards(cardEffectParams.targetCards, cardEffectState.GetTargetCardType(), sourceCardState, cardEffectState.GetTargetCharacterSubtype());
            foreach (var item in targets)
            {
                item.GetTemporaryCardStateModifiers().IncrementAdditionalCost(cardEffectState.GetParamInt());
                item.GetTemporaryCardStateModifiers().IncrementAdditionalXCost(cardEffectState.GetParamInt());
            }
            yield break;
        }
    }
}
