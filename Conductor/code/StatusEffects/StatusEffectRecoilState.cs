using ShinyShoe;
using System.Collections;

namespace Conductor.StatusEffects
{
    class StatusEffectRecoilState : StatusEffectState
    {
        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            var attacker = inputTriggerParams.attacker;
            if (attacker != null && attacker.IsAlive)
            {
                return attacker.GetStatusEffectStacks(GetStatusId()) > 0;
            }
            return false;
        }

        protected override IEnumerator OnTriggered(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            var attacker = inputTriggerParams.attacker;
            var stacks = attacker.GetStatusEffectStacks(GetStatusId());
            CoreSignals.DamageAppliedPlaySound.Dispatch(Damage.Type.Spikes);
            yield return coreGameManagers.GetCombatManager().ApplyDamageToTarget(GetDamageAmount(stacks), attacker, new CombatManager.ApplyDamageToTargetParameters
            {
                damageType = Damage.Type.Default,
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
