using Conductor.Data.Registers;

namespace Conductor.Extensions
{
    public static class EssenceExtensions
    {
        public static bool IsEssenceUpgrade(this CardUpgradeData upgrade)
        {
            if (UnitEssenceRegistry.Instance == null) return false;
            return UnitEssenceRegistry.Instance!.IsEssenceUpgrade(upgrade);
        }

        public static bool IsEssenceUpgrade(this CardUpgradeState upgradeState)
        {
            var upgradeData = upgradeState.GetSourceCardUpgradeData();
            if (upgradeData == null) return false;
            return upgradeData.IsEssenceUpgrade();
        }

        public static CharacterData? GetSourceEssenceCharacter(this CardUpgradeData upgrade)
        {
            if (UnitEssenceRegistry.Instance == null) return null;
            return UnitEssenceRegistry.Instance!.GetSourceEssenceCharacter(upgrade);
        }

        public static CharacterData? GetSourceEssenceCharacter(this CardUpgradeState upgradeState)
        {
            var upgradeData = upgradeState.GetSourceCardUpgradeData();
            return upgradeData?.GetSourceEssenceCharacter();
        }

        public static CardUpgradeData? GetEssence(this CharacterData character)
        {
            if (UnitEssenceRegistry.Instance == null) return null;
            return UnitEssenceRegistry.Instance!.GetEssence(character);
        }

        public static CardUpgradeData? GetEssence(this CharacterState characterState)
        {
            var characterData = characterState.GetSourceCharacterData();
            return characterData?.GetEssence();
        }
    }
}
