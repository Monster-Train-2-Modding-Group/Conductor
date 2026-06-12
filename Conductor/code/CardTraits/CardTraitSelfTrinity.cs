using System.Collections;

namespace Conductor.CardTraits
{
    /// <summary>
    /// Card Trait that gives a bonus effect to the card that activates every 3rd time the card is played.
    /// The Bonus effect is specified via param_upgrade, and added to the card via param_description.
    /// 
    /// Example json:
    /// "traits": [
    ///   {
    ///     "id": "Trinity",
    ///     "name": {
    ///       "id": "@CardTraitSelfTrinity",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "param_description": {
    ///       "english": "<b>Bonus</b>: 0[ember], <nobr>+<b>[upgrade.bonusdamage]</b> [magicpower]</nobr>, Apply [dazed] [upgrade.trigger0.effect0.status0.power]."
    ///     },
    ///     "param_upgrade": "@99CostReductionAddMagicPower"
    ///   }
    /// ]
    /// </summary>
    public class CardTraitSelfTrinity : CardTraitState
    {
        private CardUpgradeState? cachedTraitUpgradeState;
        private bool upgradeActive;
        private int playedOffset = 0;
        public override bool DisplayAsStateModifier => true;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardTraitFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("Temporary upgrade to apply for third cast."),
                [CardTraitFieldNames.ParamDescription.GetFieldName()] = new PropDescription("Description for bonus effect.")
            };
        }

        public override void OnTraitStateReset()
        {
            CardState card = GetCard();
            card?.RemoveUpgrade(GetCachedTraitUpgradeState(), card.GetCardStateModifiers());
            upgradeActive = false;
        }

        public override IEnumerator OnCardDiscarded(CardManager.DiscardCardParams discardCardParams, ICoreGameManagers coreGameManagers)
        {
            var associatedCard = GetCard();
            if (associatedCard == null)
                yield break;

            if (IsTrinity())
            {
                associatedCard.ApplyPermanentUpgrade(GetCachedTraitUpgradeState(), coreGameManagers.GetSaveManager(), ignoreUpgradeAnimation: true);
                upgradeActive = true;
            }
            else if (upgradeActive)
            {
                associatedCard.RemoveUpgrade(GetCachedTraitUpgradeState(), associatedCard.GetCardStateModifiers());
                upgradeActive = false;
            }
            associatedCard.ReapplyMagicPowerScalingFromTraitsToAllExistingUpgrades(coreGameManagers);
            associatedCard.UpdateCardBodyText();
            coreGameManagers.GetCardManager().RefreshCardInHand(associatedCard, cleanupTweens: false);
        }

        public bool IsTrinity()
        {
            return GetTrinityCount() == 2;
        }

        public int GetTrinityCount()
        {
            var associatedCard = GetCard();
            if (associatedCard == null)
                return 0;
            return (associatedCard.GetCurrentScenarioPlayCount() + playedOffset) % 3;
        }

        /// <summary>
        /// Force Trinity on the card.
        /// </summary>
        /// <returns></returns>
        public void ForceTrinity()
        {
            playedOffset = 2 - GetTrinityCount();
            var associatedCard = GetCard();
            if (associatedCard == null)
                return;
            associatedCard.ApplyPermanentUpgrade(GetCachedTraitUpgradeState(), AllGameManagers.Instance?.GetSaveManager(), ignoreUpgradeAnimation: true);
            upgradeActive = true;
        }

        public override string GetCardText()
        {
            var text = LocalizeTraitKey("CardTraitSelfTrinity_CardText");
            var associatedCard = GetCard();
            if (associatedCard == null)
                return string.Empty;
            return string.Format(text, GetTrinityCount() + 1);
        }

        public override string GetCardTooltipText()
        {
            int times = 2 - GetTrinityCount();
            if (IsTrinity())
                return "CardTraitSelfTrinity_TrinityCharged_TooltipText".Localize();
            else
                return string.Format("CardTraitSelfTrinity_TooltipText".Localize(), times, GetParamInt());
        }

        public override string GetCardSupplementalText()
        {
            CardState card = GetCard();
            string text = GetParamDescription();
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            text = text.Localize(new CardEffectLocalizationContext(this, null, card));
            if (upgradeActive)
            {
                text = CardTextHelper.FormatTextAsUpgrade(text, isTempModifier: true, useUpgradeHighlightTextTags: true);
            }
            return text;
        }

        public override void CreateAdditionalTooltips(TooltipContainer tooltipContainer)
        {
            if (!upgradeActive)
            {
                SaveManager? saveManager = AllGameManagers.Instance.OrNull()?.GetSaveManager();
                CardUpgradeState cardUpgradeState = GetCachedTraitUpgradeState();
                tooltipContainer.AddTooltipsCardUpgrade(CardState.None, cardUpgradeState, saveManager);
                tooltipContainer.AddTooltipsUpgradedCardTraits(cardUpgradeState);
            }
        }

        private CardUpgradeState GetCachedTraitUpgradeState()
        {
            if (cachedTraitUpgradeState == null)
            {
                cachedTraitUpgradeState = new CardUpgradeState();
                cachedTraitUpgradeState.Setup(GetCardTraitData().GetCardUpgradeDataParam());
                cachedTraitUpgradeState.SetAvoidClobberingExistingTraits(value: true);
            }
            return cachedTraitUpgradeState;
        }

    }
}
