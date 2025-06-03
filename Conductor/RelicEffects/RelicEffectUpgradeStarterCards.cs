using Conductor.Interfaces;
using ShinyShoe.Logging;

namespace Conductor.RelicEffects
{
    /// <summary>
    /// A working implementation of "Wurmtooth" from Monster Train 1. That particular artifact does nothing and its effects were implemented as
    /// ClassData.enable_corruption. This RelicEffect handles upgrading starter cards. The other part of Wurmtooth can be done with ClassData.randomDraftEnhancerPool.
    /// 
    /// Example Json
    /// "relic_effects": [
    ///  {
    ///    "id": "FakeWurmkinsInfusedUpgrade",
    ///    "name": {
    ///      "id": "@RelicEffectUpgradeStarterCards",
    ///      "mod_reference": "Conductor"
    ///    },
    ///    "param_upgrade": "@AddInfused"
    ///  }
    /// ],
    /// "upgrades": [
    ///   {
    ///     "id": "AddInfused",
    ///     "trait_upgrades": [
    ///       "@Infused"
    ///     ]
    ///   }
    /// ],
    /// "traits": [
    ///   {
    ///     "id": "Infused",
    ///     "name": "CardTraitCorruptState"
    ///   }
    /// ]
    /// 
    /// </summary>
    class RelicEffectUpgradeStarterCards : RelicEffectBase, IPostStartOfRunRelicEffect
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions()
            {
                [RelicEffectFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("CardUpgrade to apply to starter cards.")
            };
        }

        private CardUpgradeData? cardUpgrade;
        public override void Initialize(RelicState relicState, RelicData relicData, RelicEffectData relicEffectData)
        {
            base.Initialize(relicState, relicData, relicEffectData);
            cardUpgrade = relicEffectData.GetParamCardUpgradeData();
            if (cardUpgrade == null)
            {
                Log.Warning(LogGroups.Gameplay, this.GetType().Name + ": Missing required field param_upgrade.");
            }
        }

        public bool IsStarterCard(CardState card, SaveManager saveManager)
        {
            AllGameData allGameData = saveManager.GetAllGameData();
            ClassData main = saveManager.GetMainClass();
            ClassData sub = saveManager.GetSubClass();
            return SaveManager.CheckIfCardShouldBeUpgradedByClassStarterUpgrade(allGameData.FindCardData(card.GetCardDataID()), main, sub);
        }

        public void ApplyEffect(RelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers)
        {
            if (cardUpgrade == null)
                return;

            foreach (CardState card in relicEffectParams.saveManager.GetDeckState())
            {
                // The starter Rarity is unused by the starter cards.
                if (IsStarterCard(card, coreGameManagers.GetSaveManager()))
                {
                    CardUpgradeState cardUpgradeState = new();
                    cardUpgradeState.Setup(cardUpgrade);
                    card.Upgrade(cardUpgradeState, relicEffectParams.saveManager, true);
                }
            }
        }
    }
}
