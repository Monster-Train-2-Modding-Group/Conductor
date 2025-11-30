using Conductor.TargetModes;
using Conductor.Interfaces;
using TrainworksReloaded.Base.Enums;

namespace Conductor.Extensions
{
    public static class TargetModeExtensions
    {
        internal static readonly Dictionary<TargetMode, ITargetSelector> TargetModeDictionary = [];

        /// <summary>
        /// Sets the implementation of TargetMode, without patching the relevant functions.
        /// </summary>
        /// <param name="targetMode">Must be a custom TargetMode from Trainworks</param>
        /// <param name="targetSelectorClass">Class implementing CharacterTargetSelector for TargetModes that target unit, CardTargetSelector otherwise.</param>
        /// <returns>The TargetMode</returns>
        public static TargetMode SetTargetModeSelector(this TargetMode targetMode, ITargetSelector targetSelectorClass)
        {
            if (IsVanillaTargetMode(targetMode)) return targetMode;
            if (IsInvalidTargetModeClass(targetSelectorClass)) return targetMode;
            TargetModeDictionary.Add(targetMode, targetSelectorClass);
            return targetMode;
        }

        private static bool IsVanillaTargetMode(TargetMode targetMode)
        {
            if ((byte)targetMode <= (from byte x in Enum.GetValues(typeof(TargetMode)).AsQueryable() select x).Max())
            {
                Plugin.Logger.LogError($"Attempt to redefine vanilla target mode {targetMode.ToString()}, you probably didn't mean to do this?");
                return true;
            }
            return false;
        }

        private static bool IsInvalidTargetModeClass(ITargetSelector targetSelectorClass)
        {
            if (targetSelectorClass is CharacterTargetSelector || targetSelectorClass is CardTargetSelector)
            {
                return false;
            }
            Plugin.Logger.LogError($"targetSelectorClass does not inherit from CharacterTargetSelector or CardTargetSelector {targetSelectorClass.GetType()}");
            return true;
        }

        internal static bool IsCardTargetingMode(this TargetMode mode)
        {
            var selector = TargetModeDictionary.GetValueOrDefault(mode);
            return selector != null && selector is CardTargetSelector;
        }

        internal static bool IsCharacterTargetingMode(this TargetMode mode)
        {
            var selector = TargetModeDictionary.GetValueOrDefault(mode);
            return selector != null && selector is CharacterTargetSelector;
        }

        internal static ITargetSelector? GetTargetSelector(this TargetMode targetMode)
        {
            return TargetModeDictionary.GetValueOrDefault(targetMode);
        }

        internal static void PreCollectTargets(this TargetMode mode, CardEffectState effectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, bool isTesting)
        {
            var impl = TargetModeDictionary.GetValueOrDefault(mode);
            if (impl == null)
                return;
            impl.PreCollectTargets(effectState, cardEffectParams, coreGameManagers, isTesting);
        }

        internal static void CollectTargetCards(this TargetMode mode, TargetHelper.CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CardState> targetCards)
        {
            var impl = TargetModeDictionary.GetValueOrDefault(mode);
            if (impl == null || impl is not CardTargetSelector cardTargetSelector)
                return;

            cardTargetSelector.CollectTargetCards(data, coreGameManagers, targetCards);
        }
    }
}
