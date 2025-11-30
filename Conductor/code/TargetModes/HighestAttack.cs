using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target Mode that targets the unit with Highest Attack stat on the floor.
    /// If multiple exists the front-most unit is selected.
    /// </summary>
    class HighestAttack : CharacterTargetSelector
    {
        public override bool TargetsRoom => true;
        public override bool ResolvesToSingleTarget => true;

        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;

        public override void FilterTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, IReadOnlyList<CharacterState> allValidTargets, List<CharacterState> chosenTargets)
        {
            CharacterState? strongest = null;
            for (int i = 0; i < allValidTargets.Count; i++)
            {
                var current = allValidTargets[i];
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
