using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target Mode that targets the unit with Lowest Attack stat on the floor.
    /// If multiple exists the front-most unit is selected.
    /// </summary>
    class LowestAttack : CharacterTargetSelector
    {
        public override bool TargetsRoom => true;
        public override bool ResolvesToSingleTarget => true;

        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;

        public override void FilterTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, IReadOnlyList<CharacterState> allValidTargets, List<CharacterState> chosenTargets)
        {
            CharacterState? weakest = null;
            for (int i = 0; i < allValidTargets.Count; i++)
            {
                var current = allValidTargets[i];
                if (weakest == null || current.GetAttackDamage() < weakest.GetAttackDamage())
                {
                    weakest = current;
                }
            }
            if (weakest != null)
            {
                chosenTargets.Add(weakest);
            }
        }
    }
}
