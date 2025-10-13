using Conductor.Interfaces;
using HarmonyLib;

namespace Conductor.Patches
{
    /// <summary>
    /// Implementation of IPostStartOfRunRelicEffect. THis is a patch that runs these relic effects after all others have ran.
    /// </summary>
    [HarmonyPatch(typeof(RelicManager), nameof(RelicManager.ApplyStartOfRunRelicEffects))]
    class ApplyStartOfRunRelicEffectsPatch
    {
        public static void Postfix(SaveManager ___saveManager, RelicEffectParams ___relicEffectParams, AllGameManagers ___allGameManagers)
        {
            foreach (RelicState currentRelic in ___saveManager.GetAllRelics())
            {
                foreach (IRelicEffect effect in currentRelic.GetEffects())
                {
                    if (effect is IPostStartOfRunRelicEffect postStartOfRunRelicEffect)
                    {
                        postStartOfRunRelicEffect.ApplyEffect(___relicEffectParams, ___allGameManagers.GetCoreManagers());
                    }
                }
            }
        }
    }
}
