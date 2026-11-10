using ShinyShoe;
using System.Collections;

namespace Conductor.StatusEffects
{
    public sealed class StatusEffectSparklesState : StatusEffectState, IDamageStatusEffect
    {
        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            if (inputTriggerParams.associatedCharacter != null && inputTriggerParams.associatedCharacter.IsAlive && inputTriggerParams.associatedCharacter.GetStatusEffectStacks(GetStatusId()) > 0)
            {
                return inputTriggerParams.damageSourceCard?.GetCardType() == CardType.Spell;
            }
            return false;
        }

        protected override IEnumerator OnTriggered(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            //CoreSignals.DamageAppliedPlaySound.Dispatch(Damage.Type.Poison);
            yield return coreGameManagers.GetCombatManager().ApplyDamageToTarget(GetDamageAmount(), GetAssociatedCharacter(), new CombatManager.ApplyDamageToTargetParameters
            {
                damageType = Damage.Type.Poison,
                affectedVfx = GetSourceStatusEffectData()?.GetOnAffectedVFX(),
                relicState = inputTriggerParams.suppressingRelic
            });
        }

        public override int GetEffectMagnitude(int stacks)
        {
            return GetDamageAmount();
        }

        private int GetDamageAmount()
        {
            var stacks = GetAssociatedCharacter().GetStatusEffectStacks(GetStatusId());
            return (GetParamInt() + relicManager.GetModifiedStatusMagnitudePerStack(GetStatusId(), GetAssociatedCharacter().GetTeamType())) * stacks;
        }
    }
}
