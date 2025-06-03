using ShinyShoe.Logging;
using System.Collections;

namespace Conductor.StatusEffects
{
    /// <summary>
    /// Pretty much this is a combination of Lifesteal (for when to trigger) and Rage (for buffing attack). 
    /// </summary>
    class StatusEffectSmirkState : StatusEffectState
    {
        public const string StatusId = "conductor_statuseffect_smirk";

        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            if (inputTriggerParams.attacked == null)
            {
                return false;
            }
            if (inputTriggerParams.attacker == null)
            {
                return false;
            }
            if (inputTriggerParams.attacker.PreviewMode)
            {
                return false;
            }
            outputTriggerParams.damage = inputTriggerParams.damage;
            if (inputTriggerParams.damage > 0)
            {
                if (inputTriggerParams.damageType == Damage.Type.DirectAttack)
                {
                    return true;
                }
                if (inputTriggerParams.damageSourceCard != null && inputTriggerParams.damageSourceCard.IsUnitAbility())
                {
                    return true;
                }
            }
            return false;
        }

        protected override IEnumerator OnTriggered(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            yield break;
        }

        private int DamageValue(int stacks)
        {
            return GetMagnitudePerStack() * stacks;
        }

        public override void OnStacksAdded(CharacterState character, int numStacksAdded, ICoreGameManagers coreGameManagers)
        {
            if (character == null)
            {
                Log.Debug(LogGroups.Gameplay, "StatusEffectSmirkState.OnStackAdded() could not add buff because a NULL character was provided.");
            }
            else if (numStacksAdded > 0)
            {
                character.BuffDamage(DamageValue(numStacksAdded), null, fromStatusEffect: true);
            }
        }

        public override void OnStacksRemoved(CharacterState character, int numStacksRemoved, ICoreGameManagers coreGameManagers)
        {
            if (character == null)
            {
                Log.Debug(LogGroups.Gameplay, "StatusEffectSmirkState.OnStackRemoved() could not remove buff because a NULL character was provided.");
                return;
            }
            if (numStacksRemoved > 0)
            {
                character.DebuffDamage(DamageValue(numStacksRemoved), null, fromStatusEffect: true);
            }
        }

        public override int GetEffectMagnitude(int stacks = 1)
        {
            return DamageValue(stacks);
        }

        public override int GetMagnitudePerStack()
        {
            return GetParamInt() + relicManager.GetModifiedStatusMagnitudePerStack(StatusId, GetAssociatedCharacter().GetTeamType());
        }
    }
}
