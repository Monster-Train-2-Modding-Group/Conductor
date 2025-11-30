using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target mode that targets the character with the most health in the room.
    /// If multiple exists the front-most unit is selected.
    /// </summary>
    class Strongest : CharacterTargetSelector
    {
        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;
        public override bool ResolvesToSingleTarget => true;

        public override bool TargetsRoom => true;

        public override void FilterTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, IReadOnlyList<CharacterState> allValidTargets, List<CharacterState> chosenTargets)
        {
            CharacterState? strongest = null;
            for (int i = 0; i < allValidTargets.Count; i++)
            {
                var current = allValidTargets[i];
                if (strongest == null || current.GetHP() > strongest.GetHP())
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
