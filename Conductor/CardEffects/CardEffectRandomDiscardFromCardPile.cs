using ShinyShoe.Logging;
using System.Collections;
using UnityEngine;

namespace Conductor.CardEffects
{
    /// <summary>
    /// A generic version of CardEffectRandomDiscard. It's kinda like CardEffectRecursion except stuff goes in trash.
    /// Allows you to move cards from anywhere to the discard. If moving cards from the Consume or Eaten piles the cards
    /// are restored (which adds to deck), then drawn, then discarded.
    /// 
    /// WARNING if applying a permanent upgrade, You should not modify the CardUpgradeState
    /// via CardTraitState's OnApplyingCardUpgradeToUnit. Any modifications to the CardUpgrade
    /// are not able to be saved and will be lost if the game is reloaded. Only the original stats
    /// from the CardUpgradeData are used.
    /// 
    /// Params:
    ///   TargetCardType: (Required) Target card filter. Note default is Spell cards only. Set to CardType.Invalid to disable filtering.
    ///   TargetCharacterSubtype: If TargetCardType is Monster additional filtering based on subtype.The default is the None subtype which matches all.
    ///
    /// Example Json
    /// {
    ///   "id": "DiscardTwoMonstersFromDeck",
    ///   "name": {
    ///     "id": "CardEffectRandomDiscardFromCardPile",
    ///     "mod_reference": "Conductor"
    ///   },
    ///   "target_mode": "draw_pile",
    ///   "target_card_type": "monster",
    ///   "param_int":  2
    /// }
    /// </summary>
    public sealed class CardEffectRandomDiscardFromCardPile : CardEffectBase
    {
        public override bool CanPlayAfterBossDead => false;
        public override bool CanApplyInPreviewMode => false;

        // Small delay before discarding drawn cards.
        private static readonly float DelayBeforeDiscard = 0.1f;
        // Unfortunately these timings aren't specified in the AnimationTiming class.
        private static readonly float RestoreDelayBeforeDiscard = 0.55f;

        public static readonly List<TargetMode> HandTargets =
        [
            TargetMode.Hand,
            TargetMode.LastDrawnCard,
        ];
        private readonly List<CardState> targets = [];
        private readonly List<CardState> selected = [];

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("An Optional CardUpgrade to apply. Note that CardUpgradeMaskData filters can be applied to restrict the upgrade from hitting non matching cards."),
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("If true the CardUpgrade is applied permanently."),
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Num Cards To Discard"),
                [CardEffectFieldNames.UseIntRange.GetFieldName()] = new PropDescription("Use Range For Num Cards To Discard"),
                [CardEffectFieldNames.ParamMinInt.GetFieldName()] = new PropDescription("Min Num Cards To Discard"),
                [CardEffectFieldNames.ParamMaxInt.GetFieldName()] = new PropDescription("Max Num Cards To Discard"),
                [CardEffectFieldNames.ParamMultiplier.GetFieldName()] = new PropDescription("Range Multiplier")
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
            int count = cardEffectState.GetParamInt();
            if (cardEffectState.GetUseIntRange())
            {
                count = Mathf.FloorToInt(cardEffectState.GetParamMultiplier() * (float)RandomManager.Range(cardEffectState.GetParamMinInt(), cardEffectState.GetParamMaxInt(), RngId.BattleTest));
            }
            return targets.Count >= count;
        }

        public static bool TargetsHand(TargetMode targetMode)
        {
            return HandTargets.Contains(targetMode);
        }

        public HandUI.DrawSource GetDrawSource(TargetMode targetMode)
        {
            switch (targetMode)
            {
                case TargetMode.Discard:
                    return HandUI.DrawSource.Discard;
                // Consume/Eaten will go back to deck.
                case TargetMode.Exhaust:
                    return HandUI.DrawSource.Consume;
                case TargetMode.Eaten:
                    return HandUI.DrawSource.Eaten;
                default:
                    return HandUI.DrawSource.Deck;
            }
        }

        public float GetDrawDelay(TargetMode targetMode, BalanceData.AnimationTimingData timings)
        {
            switch (targetMode)
            {
                case TargetMode.Discard:
                    return timings.cardDrawAnimationDuration;
                case TargetMode.Eaten:
                case TargetMode.Exhaust:

                    return timings.cardConsumeReturnAnimationDuration + timings.cardDrawAnimationDuration + RestoreDelayBeforeDiscard;
                default:
                    return timings.cardDrawAnimationDuration;
            }
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
            targets.Shuffle(RngId.Battle);
            int num = Math.Min(cardEffectState.GetIntInRange(), targets.Count);

            selected.Clear();
            for (int i = 0; i < num; i++)
            {
                selected.Add(targets[i]);
            }

            CardManager cardManager = coreGameManagers.GetCardManager();
            SaveManager saveManager = coreGameManagers.GetSaveManager();
            if (!TargetsHand(cardEffectState.GetTargetMode()))
            {
                for (int num3 = 0; num3 <= num - 1; num3++)
                {
                    var card = selected[num3];
                    cardManager.RestoreExhaustedOrEatenCard(card);
                    ApplyCardUpgrade(cardEffectState, card, coreGameManagers);
                    cardManager.DrawSpecificCard(card, false, GetDrawSource(cardEffectState.GetTargetMode()), sourceCardState, num3, num);
                }
                // Wait before doing the discard, so that the player sees the cards.
                yield return CoreUtil.WaitForSecondsOrBreak(GetDrawDelay(cardEffectState.GetTargetMode(), saveManager.GetAllGameData().GetBalanceData().GetAnimationTimingData()) + DelayBeforeDiscard);
            }
            else
            {
                foreach (var card in selected)
                {
                    ApplyCardUpgrade(cardEffectState, card, coreGameManagers);
                }
            }

            float effectDelay = saveManager.GetAllGameData().GetBalanceData().GetAnimationTimingData().cardEffectDiscardAnimationDelay;
            CardManager.DiscardCardParams discardCardParams = new();
            foreach (CardState item in selected)
            {
                discardCardParams.effectDelay = effectDelay;
                discardCardParams.discardCard = item;
                discardCardParams.triggeredByCard = true;
                discardCardParams.triggeredCard = sourceCardState;
                discardCardParams.wasPlayed = false;
                yield return cardManager.DiscardCard(discardCardParams);
            }
        }

        private void ApplyCardUpgrade(CardEffectState cardEffectState, CardState card, ICoreGameManagers coreGameManagers)
        {
            CardManager cardManager = coreGameManagers.GetCardManager();
            SaveManager saveManager = coreGameManagers.GetSaveManager();
            RelicManager relicManager = coreGameManagers.GetRelicManager();
            RoomManager roomManager = coreGameManagers.GetRoomManager();

            var cardUpgradeData = cardEffectState.GetParamCardUpgradeData();
            if (cardUpgradeData == null)
            {
                return;
            }
            foreach (CardUpgradeMaskData filter in cardUpgradeData.GetFilters())
            {
                if (!filter.FilterCard(card, relicManager))
                {
                    return;
                }
            }
            CardUpgradeState cardUpgradeState = new();
            cardUpgradeState.Setup(cardUpgradeData);

            foreach (CardTraitState traitState in cardEffectState.GetParentCardState().GetTraitStates())
            {
                if (!traitState.OnCardBeingUpgraded(card, cardEffectState.GetParentCardState(), cardUpgradeState, coreGameManagers))
                {
                    return;
                }
            }
            if (cardEffectState.GetParamBool())
            {
                // The additional steps seen below are handled in CardState.Upgrade.
                card.Upgrade(cardUpgradeState, saveManager);
            }
            else
            {
                card.GetTemporaryCardStateModifiers().AddUpgrade(cardUpgradeState);
                card.ReapplyMagicPowerScalingFromTraitsToAllExistingUpgrades(roomManager, cardManager, relicManager);
                card.UpdateCardBodyText();
            }
        }

    }
}
