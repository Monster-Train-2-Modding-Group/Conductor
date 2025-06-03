namespace Conductor.RoomModifiers
{
    /// <summary>
    /// Simplified version of the Stock RoomStateHealCostModifier with improvements.
    /// The original has extraneous code that was checking for a condition (A monster with a spawn effect that applies regen) that doesn't exist.
    /// This version checks for various Healing card effects and can be extended across all mods, by mods with custom heal effects that don't
    /// inherit from CardEffectHeal adding to the OtherHealingCardEffects list.
    /// 
    /// The original only checked for CardEffectHeal (and subclasses).
    /// 
    /// "room_modifiers": [
    ///   {
    ///     "id": "ReduceHealSpellsBy1Ember",
    ///     "name": {
    ///       "id": "@RoomStateHealSpellCostModifier",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "param_int": 1
    ///   }
    /// ]
    /// </summary>
    public class RoomStateHealSpellCostModifier : RoomStateCostModifierBase
    {
        // TODO discover all of the new Healing Card Effects that don't inherit from CardEffectHeal.
        public static List<Type> OtherHealingCardEffects = [];
        // CardEffectHealPyrePerEmberCost is ommitted intentionally. It doesn't inherit from CardEffectHeal. (Holystone implementation).

        public override int GetModifiedCardPlayedCost(CardState cardState, CardStatistics cardStatistics)
        {
            return 0;
        }

        public override int GetModifiedCost(CardState cardState, CardStatistics cardStatistics, RelicManager relicManager)
        {
            if (cardState.GetCardType() != CardType.Spell)
                return 0;

            foreach (CardEffectState effectState in cardState.GetEffectStates())
            {
                if (typeof(CardEffectHeal).IsAssignableFrom(effectState.GetCardEffect().GetType()))
                {
                    return modifiedCost;
                }
                if (OtherHealingCardEffects.Contains(effectState.GetCardEffect().GetType()))
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
                    if (typeof(CardEffectHeal).IsAssignableFrom(effectState.GetCardEffect().GetType()))
                    {
                        return modifiedCost;
                    }
                    if (OtherHealingCardEffects.Contains(effectState.GetCardEffect().GetType()))
                    {
                        return modifiedCost;
                    }
                }
            }
            return 0;
        }
    }
}