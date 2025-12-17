using Conductor.Extensions;
using HarmonyLib;
using ShinyShoe.Audio;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(SoundManager), "OnDamageApplied")]
    public class DamageSoundEffectsPatch
    {
        public static void Postfix(Damage.Type damageType, CoreAudioSystem ___audioSystem)
        {
            if (DamageExtensions.DamageTypeToCue.TryGetValue(damageType, out var cue))
            {
                ___audioSystem.PlaySfx(cue);
            }
        }
    }
}
