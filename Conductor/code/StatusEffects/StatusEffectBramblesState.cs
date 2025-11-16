using ShinyShoe;
using System.Collections;

namespace Conductor.StatusEffects
{
    /// <summary>
    /// Example status effect that makes the damage always be one.
    /// </summary>
    class StatusEffectBramblesState : StatusEffectState
    {
        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            return GetAssociatedCharacter().IsAlive;
        }

        protected override IEnumerator OnTriggered(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            var associatedCharacter = GetAssociatedCharacter();
            var stacks = associatedCharacter.GetStatusEffectStacks(GetStatusId());
            CoreSignals.DamageAppliedPlaySound.Dispatch(Damage.Type.Spikes);
            yield return coreGameManagers.GetCombatManager().ApplyDamageToTarget(GetDamageAmount(stacks), associatedCharacter, new CombatManager.ApplyDamageToTargetParameters
            {
                damageType = Damage.Type.Spikes,
                affectedVfx = GetSourceStatusEffectData()?.GetOnAffectedVFX(),
                relicState = inputTriggerParams.suppressingRelic
            });
        }

        public override int GetEffectMagnitude(int stacks = 1)
        {
            return GetDamageAmount(stacks);
        }

        private int GetDamageAmount(int stacks)
        {
            return (GetParamInt() + relicManager.GetModifiedStatusMagnitudePerStack(GetStatusId(), GetAssociatedCharacter().GetTeamType())) * stacks;
        }
    }
}
