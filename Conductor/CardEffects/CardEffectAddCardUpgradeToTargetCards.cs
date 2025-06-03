using ShinyShoe.Logging;
using System.Collections;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Like CardEffectAddTempCardUpgradeToCardsInHand, but uses TargetMode to select which cards get upgraded.
    /// Further filtering can be done with the CardUpgrade's UpgradeMaskData. Additionally this CardEffect
    /// allows you to permanently upgrade the cards.
    /// 
    /// This card effect doesn't suffer the limitations of the afforementioned CardEffect, it can be used in
    /// Charactere's Trigger effects.
    /// 
    /// WARNING if applying a permanent upgrade, You should not modify the CardUpgradeState
    /// via CardTraitState's OnCardBeingUpgrade. Any modifications to the CardUpgrade
    /// are not able to be saved and will be lost if the game is reloaded. Only the original stats
    /// from the CardUpgradeData are used.
    /// 
    /// Parameters:
    ///   ParamCardUpgrade: CardUpgrade to apply. The filters in the CardUpgrade further filter from TargetCards.
    ///   ParamBool: False for temporary upgrade. True for permanent upgrade.
    ///   
    /// Example Json
    /// "effects": [
    ///   {
    ///     "id": "DiscardedMonstersCostOneLessAndGain20Attack",
    ///     "name": {
    ///       "id": "CardEffectAddCardUpgradeToTargetCards",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "target_mode": "discard",
    ///     "target_team": "none",
    ///     "param_bool": true,
    ///     "param_upgrade": "@MonstersCostLessAndAttackUp"
    ///   }
    /// ]
    /// "upgrades": [
    ///   {
    ///     "id": "CostLess",
    ///     "cost_reduction": 1,
    ///     "bonus_damage": 20,
    ///     "filters": [
    ///       {
    ///         "id": "@OnlyMonsterCards"
    ///       }
    ///     ]
    ///   }
    /// ],
    /// "upgrade_masks": [
    ///   {
    ///     "id": "OnlyMonsterCards",
    ///     "type": "monster"
    ///   }
    /// ],
    /// </summary>
    public sealed class CardEffectAddCardUpgradeToTargetCards : CardEffectBase
    {
        public override bool CanPlayAfterBossDead => false;
        public override bool CanApplyInPreviewMode => false;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("CardUpgrade to apply."),
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("False to apply the upgrade temporarily. True to apply it permanently."),
            };
        }

        public override void Setup(CardEffectState cardEffectState)
        {
            base.Setup(cardEffectState);
            // Sanity checks
            if (cardEffectState.GetParamCardUpgradeData() == null)
            {
                Log.Warning(LogGroups.Gameplay, this.GetType().Name + ": Missing required field param_upgrade.");
            }
            if (!cardEffectState.GetTargetMode().TargetModeIsACardPile() && cardEffectState.GetTargetMode() != TargetMode.Hand && cardEffectState.GetTargetMode() != TargetMode.LastDrawnCard)
            {
                Log.Warning(LogGroups.Gameplay, this.GetType().Name + ": TargetMode " + cardEffectState.GetTargetMode() + " is invalid for this CardEffect.");
            }
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return cardEffectParams.targetCards.Count > 0;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers systemManagers)
        {
            bool cardUpgraded = false;
            foreach (CardState item in cardEffectParams.targetCards)
            {
                // CardUpgradeMask filtering
                bool flag = false;
                foreach (CardUpgradeMaskData filter in cardEffectState.GetParamCardUpgradeData().GetFilters())
                {
                    if (!filter.FilterCard(item, coreGameManagers.GetRelicManager()))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    continue;
                }

                var sourceCard = cardEffectState.GetParentCardState() ?? cardEffectParams.selfTarget?.GetSpawnerCard();
                CardUpgradeState cardUpgradeState = new();
                cardUpgradeState.Setup(cardEffectState.GetParamCardUpgradeData(), false, cardEffectParams.isFromHiddenTrigger);

                // The card trait can reject the card upgrade too.
                // But this also can modify the card upgrade so make sure the upgrade is clean.
                flag = false;
                if (sourceCard != null)
                {
                    foreach (CardTraitState traitState in sourceCard.GetTraitStates())
                    {
                        if (!traitState.OnCardBeingUpgraded(item, sourceCard, cardUpgradeState, coreGameManagers))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    continue;
                }

                CardManager cardManager = coreGameManagers.GetCardManager();
                RoomManager roomManager = coreGameManagers.GetRoomManager();
                RelicManager relicManager = coreGameManagers.GetRelicManager();
                if (cardEffectState.GetParamBool())
                {
                    // The additional steps seen below are handled in CardState.Upgrade.
                    item.Upgrade(cardUpgradeState, coreGameManagers.GetSaveManager());
                }
                else
                {
                    item.GetTemporaryCardStateModifiers().AddUpgrade(cardUpgradeState);
                    item.ReapplyMagicPowerScalingFromTraitsToAllExistingUpgrades(roomManager, cardManager, relicManager);
                    item.UpdateCardBodyText();
                }

                if (coreGameManagers.GetCardManager().GetCardInHand(item) != null)
                {
                    coreGameManagers.GetCardManager().RefreshCardInHand(item, cleanupTweens: false);
                    coreGameManagers.GetCardManager().GetCardInHand(item).ShowEnhanceFX();
                }
                cardUpgraded = true;
            }
            if (cardUpgraded)
            {
                ShowPyreNotification(cardEffectState, coreGameManagers.GetSaveManager(), systemManagers.GetPopupNotificationManager());
            }
            yield break;
        }

        public override void GetTooltipsStatusList(CardEffectState cardEffectState, ref List<string> outStatusIdList)
        {
            CardEffectAddTempCardUpgradeToCardsInHand.GetTooltipsStatusList(cardEffectState.GetSourceCardEffectData(), ref outStatusIdList);
        }

        public override void CreateAdditionalTooltips(CardEffectState cardEffectState, TooltipContainer tooltipContainer, SaveManager saveManager)
        {
            CardUpgradeState cardUpgradeState = new();
            cardUpgradeState.Setup(cardEffectState.GetParamCardUpgradeData());
            tooltipContainer.AddTooltipsCardUpgrade(CardState.None, cardUpgradeState, saveManager);
            tooltipContainer.AddTooltipsUpgradedCardTraits(cardUpgradeState);
        }
    }
}
