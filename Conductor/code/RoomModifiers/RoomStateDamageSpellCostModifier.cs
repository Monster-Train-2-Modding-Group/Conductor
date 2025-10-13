namespace Conductor.RoomModifiers
{
    /// <summary>
    /// Simplified version of the Stock RoomStateDamageCostModifier with improvements.
    /// The original has extraneous code that was checking for a condition (A monster with a spawn effect that applies regen) that doesn't exist.
    /// This version checks for various Damaging card effects and can be extended across all mods, by mods with custom damage effects adding to 
    /// the DamagingCardEffects list.
    /// 
    /// The original only checked for CardEffectDamage.
    /// 
    /// Example Json
    /// "room_modifiers": [
    ///   {
    ///     "id": "ReduceDamageSpellsBy2Ember",
    ///     "name": {
    ///       "id": "@RoomStateDamageSpellCostModifier",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "param_int": 2
    ///   }
    /// ]
    /// </summary>
    public class RoomStateDamageSpellCostModifier : RoomStateCostModifierBase
    {
        public static List<Type> DamagingCardEffects =
        [
            typeof(CardEffectDamage),
            typeof(CardEffectDamageByPercentRemainingHealth),
            typeof(CardEffectDamageByUnitsKilled),
            typeof(CardEffectDamagePerHp),
            typeof(CardEffectDamagePerSourceAttack),
            typeof(CardEffectDamagePerTargetAttack),
            typeof(CardEffectDamageWithTrample),
            typeof(CardEffectHealAndDamageRelative),
        ];

        public override int GetModifiedCardPlayedCost(CardState cardState, CardStatistics cardStatistics)
        {
            return 0;
        }

        public override int GetModifiedCost(CardState cardState, CardStatistics cardStatistics, RelicManager monsterManager)
        {
            if (cardState.GetCardType() != CardType.Spell)
            {
                return 0;
            }

            foreach (CardEffectState effectState in cardState.GetEffectStates())
            {
                if (DamagingCardEffects.Contains(effectState.GetCardEffect().GetType()))
                {
                    return modifiedCost;
                }
            }
            foreach (CardTriggerEffectState effectTrigger in cardState.GetTriggers())
            {
                if (effectTrigger.GetTrigger() != CardTriggerType.OnCast)
                {
                    continue;
                }
                foreach (CardEffectState effectState in effectTrigger.GetCardEffectParams())
                {
                    if (DamagingCardEffects.Contains(effectState.GetCardEffect().GetType()))
                    {
                        return modifiedCost;
                    }
                }
            }
            return 0;
        }
    }
}
