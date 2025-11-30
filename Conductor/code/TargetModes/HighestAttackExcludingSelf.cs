using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target Mode that targets the unit with Highest Attack stat in the room, but it can't be itself. Only makes sense for card effects that are attached to a CharacterTrigger.
    /// If multiple exists the front-most unit is selected.
    /// </summary>
    class HighestAttackExcludingSelf : CharacterTargetSelector
    {
        public override bool TargetsRoom => true;
        public override bool ResolvesToSingleTarget => true;

        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;

        public override void FilterTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, IReadOnlyList<CharacterState> allValidTargets, List<CharacterState> chosenTargets)
        {
            CharacterState? self = data.selfTarget;
            CharacterState? strongest = null;
            foreach (var current in allValidTargets) 
            {
                if (current == self)
                {
                    continue;
                }
                if (strongest == null || current.GetAttackDamage() > strongest.GetAttackDamage())
                {
                    strongest = current;
                }
            }
            if (strongest != null)
            {
                chosenTargets.Add(strongest);
            }
        }
    }
}
