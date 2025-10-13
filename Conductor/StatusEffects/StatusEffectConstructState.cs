using System.Collections;

namespace Conductor.StatusEffects
{
    public interface IConstructStatusArmorModifier : IRelicEffect
    {
        RelicState SourceRelicState { get; }
    }

    public sealed class StatusEffectConstructState : StatusEffectState
    {
        public override int GetTriggerOrder()
        {
            return 1;
        }

        public override bool TestTrigger(StatusEffectState.InputTriggerParams inputTriggerParams, StatusEffectState.OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            CombatManager combatManager = coreGameManagers.GetCombatManager();
            return (!(combatManager != null) || combatManager.GetTurnCount() != 0) && inputTriggerParams.associatedCharacter.GetStatusEffectStacks(base.GetStatusId()) > 0;
        }

        protected override IEnumerator OnTriggered(StatusEffectState.InputTriggerParams inputTriggerParams, StatusEffectState.OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            CombatManager combatManager = coreGameManagers.GetCombatManager();
            if (combatManager != null && combatManager.GetTurnCount() == 0)
            {
                yield break;
            }
            int statusEffectStacks = inputTriggerParams.associatedCharacter.GetStatusEffectStacks(base.GetStatusId());
            if (statusEffectStacks <= 0)
            {
                yield break;
            }
            var constructAmount = this.GetConstructAmount(statusEffectStacks);
            inputTriggerParams.associatedCharacter.BuffDamage(constructAmount, null, true);

            var effect = this.relicManager.GetRelicEffect<IConstructStatusArmorModifier>();
            if (effect != null)
            {
                inputTriggerParams.associatedCharacter.AddStatusEffect("armor", constructAmount, new CharacterState.AddStatusEffectParams
                {
                    sourceRelicState = effect.SourceRelicState
                }, null, true, false);
            }
            yield return inputTriggerParams.associatedCharacter.BuffMaxHP(constructAmount, true, null);
        }

        public override int GetEffectMagnitude(int stacks = 1)
        {
            return this.GetConstructAmount(stacks);
        }

        private int GetConstructAmount(int stacks)
        {
            return (base.GetParamInt() + this.relicManager.GetModifiedStatusMagnitudePerStack(GetStatusId(), base.GetAssociatedCharacter().GetTeamType())) * stacks;
        }
    }
}