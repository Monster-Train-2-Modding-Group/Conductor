using UnityEngine;

namespace Conductor.RoomModifiers
{
    /// <summary>
    /// A RoomModifier that shares the units attack with other units on the same team.
    /// 
    /// Parameters:
    ///   ParamInt: Flat damage boost to share.
    ///   ParamInt2: Attack share type. 0: Shares Buffed attack (base attack + attack from status effects) 1: Shares base attack only.
    ///   ParamFloat: Damage multiplier.
    ///   
    /// Example Json
    /// "room_modifiers": [
    ///   {
    ///     "id": "GiveAllies10MoreAttack",
    ///     "name": {
    ///       "id": "@RoomStateDamagePerAttackModifier",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "param_int": 10,
    ///     "param_float": 0
    ///   }
    /// ]
    /// </summary>
    public class RoomStateDamagePerAttackModifier : RoomStateModifierBase, IRoomStateDamageModifier
    {
        enum AttackShareType
        {
            BuffedAttack = 0,
            BaseAttack = 1,
        }

        CharacterState? owner;
        private float additionalDamageMultiplier;
        private int additionalDamage;
        private AttackShareType attackShareType;

        public override void Initialize(RoomModifierData roomModifierData, SaveManager saveManager)
        {
            base.Initialize(roomModifierData, saveManager);
            additionalDamage = roomModifierData.GetParamInt();
            attackShareType = (AttackShareType)roomModifierData.GetParamInt2();
            additionalDamageMultiplier = roomModifierData.GetParamFloat();
        }

        public override void OnRoomModifierAttachedToOwner(CharacterState ownerCharacterState)
        {
            owner = ownerCharacterState;
        }

        public int GetModifiedAttackDamage(Damage.Type damageType, CharacterState attackerState, bool requestingForCharacterStats, ICoreGameManagers coreGameManagers)
        {
            if (requestingForCharacterStats)
            {
                return GetDynamicInt(attackerState);
            }
            return 0;
        }

        public override int GetDynamicInt(CharacterState characterContext)
        {
            if (owner == null || characterContext == owner || characterContext.GetSpawnPoint() == null)
                return 0;

            if (characterContext.GetTeamType() == owner.GetTeamType())
            {
                return Mathf.FloorToInt(GetBaseDamage() * additionalDamageMultiplier) + additionalDamage;
            }

            return 0;
        }

        private int GetBaseDamage()
        {
            switch (attackShareType)
            {
                case AttackShareType.BaseAttack:
                    return owner?.GetUnbuffedAttackDamage() ?? 0;
                case AttackShareType.BuffedAttack:
                    return owner?.GetUnmodifiedAttackDamage() ?? 0;
            }
            return 0;
        }

        public int GetModifiedMagicPowerDamage(ICoreGameManagers coreGameManagers)
        {
            return 0;
        }
    }
}
