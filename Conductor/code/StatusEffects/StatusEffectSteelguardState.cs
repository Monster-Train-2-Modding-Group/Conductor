using Conductor.Triggers;
using ShinyShoe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.StatusEffects
{
    public class StatusEffectSteelguardState : StatusEffectState 
    {
        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers) 
        {
            CharacterState character = inputTriggerParams.attacked;
            if (character == null)
                return false;
            if (DamageHelper.IsPiercingDamage(inputTriggerParams.damageSourceCard, inputTriggerParams.damageSourceRelic, coreGameManagers, inputTriggerParams.attacker, inputTriggerParams.damageType))
                return false;
            int armor = character.GetStatusEffectStacks("armor");
            if (inputTriggerParams.damage > GetParamInt() && armor >= GetParamInt()) 
            {
                outputTriggerParams.damage = GetParamInt();
                outputTriggerParams.damageBlocked = inputTriggerParams.damage - outputTriggerParams.damage;
                return true;
            }
            return false;
        }
        public override int GetTriggerOrder() 
        {
            return -1;
        }
        public override int GetEffectMagnitude(int stacks = 1)
        {
            return GetParamInt();
        }
    }
}
