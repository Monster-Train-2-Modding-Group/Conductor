using Conductor.Interfaces;
using HarmonyLib;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(CardState), nameof(CardState.GetDiscardEffectWhenPlayed))]
    internal class TraitOverrideDiscardEffectPatch
    {
        public static void Postfix(CardState __instance, RelicManager relicManager, Type suppressTraitOnDiscard, ref HandUI.DiscardEffect __result)
        {
            foreach (var trait in __instance.GetTraitStates())
            {
                if (trait is ICardTraitOverrideDiscardEffect overideDiscardTrait)
                {
                    __result = overideDiscardTrait.GetDiscardEffectWhenPlayed(relicManager, suppressTraitOnDiscard, __result);
                }
            }
        }
    }
}
