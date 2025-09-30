namespace Conductor.Triggers
{
    public static class CharacterTriggers
    {
        // TODO pass parameters to these triggers and document them.
        public static CharacterTriggerData.Trigger Vengeance;
        public static CharacterTriggerData.Trigger Junk;
        public static CharacterTriggerData.Trigger Encounter;
        public static CharacterTriggerData.Trigger Penance;
        public static CharacterTriggerData.Trigger Accursed;
        public static CharacterTriggerData.Trigger Evoke;

        // The following a Silent event triggers, if using set hideVisualAndIgnoreSilence on all CharacterTriggers.

        /// <summary>
        /// Silent event trigger that fires when a buff is applied to the unit.
        /// 
        /// Parameters:
        ///   paramString: statusId
        ///   paramInt: Number of distinct buffs on unit.
        ///   paramInt2: total number of stacks of statusId
        /// </summary>
        public static CharacterTriggerData.Trigger OnBuffed;
        /// <summary>
        /// Silent event trigger that fires when a debuff is applied to the unit.
        /// 
        /// Parameters:
        ///   paramString: statusId
        ///   paramInt: Number of distinct debuffs on unit.
        ///   paramInt2: total number of stacks of statusId
        /// </summary>
        public static CharacterTriggerData.Trigger OnDebuffed;
    }
}
