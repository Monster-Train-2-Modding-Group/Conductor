using Conductor.Triggers;
using ShinyShoe.Logging;
using System.Collections;

namespace Conductor.StatusEffects
{
    class StatusEffectGrowthState : StatusEffectState
    {
        int? stacksChangedAmount = null;
        StatusEffectGrowthInactiveState? inactiveState;

        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            if (inputTriggerParams.triggerType == CharacterTriggers.OnGrowthGained || inputTriggerParams.triggerType == CharacterTriggers.OnGrowthLost)
            {
                return true;
            }

            return false;
        }

        protected override IEnumerator OnTriggered(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            var character = GetAssociatedCharacter();

            if (stacksChangedAmount == null || character == null)
                yield break;

            if (stacksChangedAmount > 0)
            {
                yield return character!.BuffMaxHP(MaxHPIncreaseValue(stacksChangedAmount.Value), triggerOnHeal: false, heal: false);
            }
            else if (stacksChangedAmount < 0)
            {
                yield return character!.DebuffMaxHP(MaxHPIncreaseValue(-stacksChangedAmount.Value), floor: 0, decreaseHp: false);
            }
            stacksChangedAmount = null;
        }

        public override void OnStacksAdded(CharacterState character, int numStacksAdded, CharacterState.AddStatusEffectParams addStatusEffectParams, ICoreGameManagers coreGameManagers)
        {
            if (numStacksAdded > 0)
            {
                stacksChangedAmount = numStacksAdded;
            }
            if (inactiveState == null)
            {
                character.AddStatusEffect("conductor_growth_inactive", 1);
                inactiveState = character.GetStatusEffect("conductor_growth_inactive") as StatusEffectGrowthInactiveState;
            }
        }

        private int MaxHPIncreaseValue(int stacks)
        {
            return GetMagnitudePerStack() * stacks;
        }

        public override void OnStacksRemoved(CharacterState character, int numStacksRemoved, ICoreGameManagers coreGameManagers)
        {   
            if (numStacksRemoved > 0)
            {
                stacksChangedAmount = -numStacksRemoved;
            }
            
            if (character.GetStatusEffectStacks(GetStatusId()) <= 0)
            {
                // Shouldn't happen.
                if (inactiveState == null)
                {
                    character.AddStatusEffect("conductor_growth_inactive", 1);
                    inactiveState = character.GetStatusEffect("conductor_growth_inactive") as StatusEffectGrowthInactiveState;
                }
                inactiveState!.SetLastGrowthStacksRemoved(numStacksRemoved);
            }
        }

        public override int GetEffectMagnitude(int stacks = 1)
        {
            return MaxHPIncreaseValue(stacks);
        }

        public override int GetMagnitudePerStack()
        {
            return GetParamInt() + relicManager.GetModifiedStatusMagnitudePerStack(GetStatusId(), GetAssociatedCharacter().GetTeamType());
        }
    }

    // Hidden status effect to fix bug with cards like BrainTransfer in which all stacks of growth are removed.
    // The maxhp doesn't get removed in this case because the StatusEffectStates.OnTriggered doesn't get to fire.
    // So instead we do it from a hidden status that is given on the first stack of growth.
    class StatusEffectGrowthInactiveState : StatusEffectState
    {
        int? finalStacksRemoved;

        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            if (inputTriggerParams.triggerType == CharacterTriggers.OnGrowthLost && finalStacksRemoved != null)
                return true;
                
            return false;
        }

        public void SetLastGrowthStacksRemoved(int numStacks)
        {
            finalStacksRemoved = numStacks;
        }

        protected override IEnumerator OnTriggered(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            var character = GetAssociatedCharacter();
            yield return character.DebuffMaxHP(finalStacksRemoved!.Value, floor: 0, decreaseHp: false);
            character.RemoveStatusEffect(GetStatusId(), 1);
        }
    }
}
