using ShinyShoe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Conductor.CardTraits
{
    /// <summary>
    /// Card Trait that when the card is played adds a specific card to the hand, temporarily or permanently.
    /// Unlike CardEffectAddBattle|RunCard the amount of cards generated can be potentially scaled based on a TrackedValue.
    /// To use scaling params be sure to set param_use_scaling_params to true.
    ///   
    /// Example json:
    /// "traits": [
    ///   {
    ///     "id": "AddBlightsToRandomInDeckBasedOnTimePlayed",
    ///     "name": {
    ///       "id": "@CardTraitScalingAddCard",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "param_card": "SelfPurgeBlight",
    ///     "param_tracked_value": "times_played",
    ///     "param_entry_duration": "this_battle",
    ///     "param_float": 1.0,
    ///     "param_int_2": 7,
    ///     "param_use_scaling_params": true
    ///   }
    /// ]
    /// </summary>
    public sealed class CardTraitScalingAddCard : CardTraitState
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardTraitFieldNames.paramCardData.GetFieldName()] = new PropDescription("(Required) Card To Give"),
                [CardTraitFieldNames.ParamFloat.GetFieldName()] = new PropDescription("Num Cards Multiplier if ParamUseScalingParams is set"),
                [CardTraitFieldNames.ParamInt.GetFieldName()] = new PropDescription("Num Cards Added"),
                [CardTraitFieldNames.ParamInt2.GetFieldName()] = new PropDescription("Target Card Pile", "", typeof(CardPile)),
                [CardTraitFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("Optional Upgrade"),
                [CardTraitFieldNames.ParamBool.GetFieldName()] = new PropDescription("Enable ParamInt3 Bitfield processing"),
                [CardTraitFieldNames.ParamUseScalingParams.GetFieldName()] = new PropDescription("Enable Scaling Add Card Mode"),
                [CardTraitFieldNames.ParamInt3.GetFieldName()] = new PropDescription("Bitfield for other parameters",
                "bit 1: Permanently add card to the deck (default not set)." +
                "bit 2: IgnoreTempModifiersFromSource. Don't copy temporarily modifiers from the source card played (default set)" +
                "bit 3: CopyModifiersFromSource. Copy upgrades from the source card played. (default set)" +
                "bit 4: UpgradeIsPermanent. If ParamCardUpgradeData is provided makes the upgrade parameter apply to the card permanently"),
            };  
        }

        public override IEnumerator OnCardPlayed(CardState card, ICoreGameManagers coreGameManagers)
        {
            bool permanent = false;
            bool ignoreTempUpgrades = true;
            bool copyModifiersFromSource = true;
            bool upgradeIsPermanent = false;

            if (GetParamBool())
            {
                int paramInt3 = GetParamInt3();
                permanent = (paramInt3 & 1) != 0;
                ignoreTempUpgrades = (paramInt3 & 2) != 0;
                copyModifiersFromSource = (paramInt3 & 4) != 0;
                upgradeIsPermanent = (paramInt3 & 8) != 0;
            }

            int maxCount = GetAdditionalCards(coreGameManagers.GetCardStatistics());
            CardManager cardManager = coreGameManagers.GetCardManager();
            SaveManager saveManager = coreGameManagers.GetSaveManager();
            CardData? cardData = GetParamCardData();
            if (cardData == null)
            {
                Plugin.Logger.LogWarning($"{this.GetType().Name} not setup correctly. Missing param_card.");
                yield break;
            }
            int count = maxCount;
            int maxAdd = cardManager.GetMaxHandSize() - cardManager.GetNumCardsInHand();
            if (maxAdd > 0)
            {
                count = Mathf.Min(count, maxAdd);
            }
            CardManager.AddCardUpgradingInfo? addCardUpgradingInfo = new();
            var cardUpgradeData = GetCardUpgradeDataParam();
            if (cardUpgradeData != null)
            {
                addCardUpgradingInfo.upgradeDatas.Add(cardUpgradeData);
            }
            addCardUpgradingInfo.tempCardUpgrade = !upgradeIsPermanent;
            addCardUpgradingInfo.upgradingCardSource = GetCard();
            addCardUpgradingInfo.ignoreTempUpgrades = ignoreTempUpgrades;
            addCardUpgradingInfo.copyModifiersFromCard = copyModifiersFromSource ? GetCard() : null;
            CardPile cardPile = (CardPile) GetParamInt2();
            for (int i = 0; i < count; i++)
            {
                cardManager.AddNewCard(cardData, cardPile, fromRelic: false, permanent: permanent, addCardUpgradingInfo: addCardUpgradingInfo);
            }

            yield break;
        }

        private int GetAdditionalCards(CardStatistics cardStatistics)
        {
            if (!GetUseScalingParams())
            {
                return GetParamInt();
            }

            CardStatistics.StatValueData statValueData = new()
            {
                cardState = GetCard(),
                trackedValue = GetParamTrackedValue(),
                entryDuration = GetParamEntryDuration(),
                cardTypeTarget = GetParamCardType(),
                paramSubtype = GetParamSubtype(),
                forPreviewText = false
            };
            int statValue = cardStatistics.GetStatValue(statValueData);
            return (int)(GetParamFloat() * statValue + GetParamInt());
        }
    }
}
