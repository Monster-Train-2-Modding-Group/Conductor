namespace Conductor
{
    /// <summary>
    /// Library flags mods can set to enable behaviour
    /// </summary>
    public static class Flags
    {
        internal static bool AttunedAlwaysShowParam = false;
        internal static bool SpellchainAlwaysShowParam = false;
        internal static bool ShardStatusTooltipTextIsGeneric = false;

        /// <summary>
        /// Enables showing the default Attuned parameter. That is Attuned -> Attuned 5
        /// Note that if paramInt is set to anything other than 0 or 5 the param will always display on the keyword.
        /// </summary>
        public static void EnableAttunedParamCardText()
        {
            AttunedAlwaysShowParam = true;
        }

        /// <summary>
        /// Enables showing the default Spellchain parameter. That is Spellchain -> Spellchain 1
        /// Note that if paramInt is set to anything other than 0, will always display on the keyword.
        /// </summary>
        public static void EnableSpellchainParamCardText()
        {
            SpellchainAlwaysShowParam = true;
        }

        /// <summary>
        /// Replaces the tooltip for Shard with that of Soul. Which removes the mention of Solgard the Martyr in the text.
        /// </summary>
        public static void EnableGenericShardTooltipText()
        {
            ShardStatusTooltipTextIsGeneric = true;
        }
    }
}
