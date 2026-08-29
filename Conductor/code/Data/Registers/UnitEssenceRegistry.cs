using System;
using System.Collections.Generic;
using System.Text;
using TrainworksReloaded.Core.Interfaces;

namespace Conductor.Data.Registers
{
    /// <summary>
    /// Class to store Unit Essences.
    /// 
    /// Essences are added via JSON see schemas/essences.json
    /// Essences are a 1:1 mapping of a CharacterData <=> CardUpgradeData.
    /// Do not reuse CardUpgradeData for multiple Character's Essence data.
    /// </summary>
    public class UnitEssenceRegistry
    {
        internal IDictionary<CharacterData, CardUpgradeData> Essences = new Dictionary<CharacterData, CardUpgradeData>();
        internal IDictionary<CardUpgradeData, CharacterData> ReverseEssences = new Dictionary<CardUpgradeData, CharacterData>();

        internal void Register(CharacterData character, CardUpgradeData upgrade, bool force = false)
        {
            Plugin.Logger.LogInfo($"Register Essence for {character.name} - Upgrade: {upgrade.name}");
            if (!Essences.ContainsKey(character))
            {
                Essences.Add(character, upgrade);
                if (ReverseEssences.ContainsKey(upgrade))
                {
                    Plugin.Logger.LogWarning($"Upgrade already registered as an essence for {ReverseEssences[upgrade].name}. Attempted to add it as an essence for {character.name}. Upgrades declared as essences must be unique.");
                    return;
                }
                ReverseEssences.Add(upgrade, character);
            }
            else if (force)
            {
                Plugin.Logger.LogDebug($"Overwriting essence for {character.name} - Upgrade: {upgrade.name}");
                Essences[character] = upgrade;
                ReverseEssences[upgrade] = character;
            }
        }

        /// <summary>
        /// Gets the Essence for a character.
        /// </summary>
        /// <param name="character">Character to get essence for.</param>
        /// <returns>CardUpgradeData for the essence or null if none exists.</returns>
        public CardUpgradeData? GetEssence(CharacterData character)
        {
            return Essences.GetValueOrDefault(character);
        }

        /// <summary>
        /// Given an essence gets the source character for it.
        /// </summary>
        /// <param name="upgrade">Upgrade that is an essence</param>
        /// <returns>Null if not an essence or the source CharacterData.</returns>
        public CharacterData? GetSourceEssenceCharacter(CardUpgradeData upgrade)
        {
            return ReverseEssences.GetValueOrDefault(upgrade);
        }

        /// <summary>
        /// Is the CardUpgradeData registered as an essence.
        /// </summary>
        /// <param name="upgrade">CardUpgradeData</param>
        /// <returns>true if it was registered as an essence.</returns>
        public bool IsEssenceUpgrade(CardUpgradeData upgrade)
        {
            return ReverseEssences.ContainsKey(upgrade);
        }
    }
}
