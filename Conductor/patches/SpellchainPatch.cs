using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(CardTraitCopyOnPlay), nameof(CardTraitCopyOnPlay.GetCardText))]
    class SpellchainPatch
    {
        public static void Postfix(CardTraitCopyOnPlay __instance, ref string __result)
        {
            var paramInt = __instance.GetParamInt();
            if (paramInt <= 1)
                return;
            __result = $"{__result} <b>{paramInt}</b>";
        }
    }
}
