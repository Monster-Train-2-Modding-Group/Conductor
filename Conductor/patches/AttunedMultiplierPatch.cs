using HarmonyLib;

namespace Conductor.Patches
{
    /// <summary>
    /// Attuned's multiplier comes from the trait's paramInt. The game's own data may leave it at 0,
    /// which keeps the hard-coded 5, so vanilla cards are unchanged apart from showing the number.
    /// </summary>
    [HarmonyPatch(typeof(CardTraitStrongerMagicPower))]
    class AttunedMultiplierPatch
    {
        private const int DefaultMultiplier = 5;

        private static int Multiplier(CardTraitStrongerMagicPower trait)
        {
            var paramInt = trait.GetParamInt();
            return paramInt > 0 ? paramInt : DefaultMultiplier;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(CardTraitStrongerMagicPower.GetMagicPowerMultiplier))]
        public static void GetMagicPowerMultiplier(CardTraitStrongerMagicPower __instance, ref int __result)
        {
            __result = Multiplier(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(nameof(CardTraitStrongerMagicPower.GetCardText))]
        public static void GetCardText(CardTraitStrongerMagicPower __instance, ref string __result)
        {
            int multiplier = Multiplier(__instance);
            if (multiplier == DefaultMultiplier && !Flags.AttunedAlwaysShowParam)
                return;
            __result = $"{__result} <b>{multiplier}</b>";
        }

        [HarmonyPostfix, HarmonyPatch(nameof(CardTraitStrongerMagicPower.GetCardTooltipText))]
        public static void GetCardTooltipText(CardTraitStrongerMagicPower __instance, ref string __result)
        {
            __result = string.Format("CardTraitStrongMagicPower_TooltipText".Localize(), Multiplier(__instance));
        }
    }
}
