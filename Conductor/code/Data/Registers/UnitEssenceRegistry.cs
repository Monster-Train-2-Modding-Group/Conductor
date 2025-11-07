using System;
using System.Collections.Generic;
using System.Text;
using TrainworksReloaded.Core.Interfaces;

namespace Conductor.Data.Registers
{
    public class UnitEssenceRegistry
    {
        internal IDictionary<CharacterData, CardUpgradeData> Essences = new Dictionary<CharacterData, CardUpgradeData>();

        public void Register(CharacterData character, CardUpgradeData upgrade, bool force = false)
        {
            Plugin.Logger.LogInfo($"Register Essence for {character.name} - Upgrade: {upgrade.name}");
            if (!Essences.ContainsKey(character))
            {
                Essences.Add(character, upgrade);
                return;
            }
            else if (force)
            {
                Plugin.Logger.LogDebug($"Overwriting essence for {character.name} - Upgrade: {upgrade.name}");
                Essences[character] = upgrade;
            }
        }

        public CardUpgradeData? GetEssence(CharacterData character)
        {
            return Essences.GetValueOrDefault(character);
        }
    }
}
