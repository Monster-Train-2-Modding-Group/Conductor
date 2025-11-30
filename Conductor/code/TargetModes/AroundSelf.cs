using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target mode that targets self and the units in front and behind self. Only meaningful when played from an effect in a CharacterTrigger.
    /// </summary>
    class AroundSelf : CharacterTargetSelector
    {
        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;

        public override void FilterTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, IReadOnlyList<CharacterState> allValidTargets, List<CharacterState> chosenTargets)
        {
            CharacterState? self = data.selfTarget;

            if (self == null)
            {
                Plugin.Logger.LogWarning($"{GetType().Name} is not used for an effect within a CharacterTrigger. There is no selfTarget for this TargetMode to work.");
                return;
            }

            int index = allValidTargets.IndexOf(self);

            // Not within a CharacterTrigger or wrong setup (i.e. self is monster, and the target mode only specified heroes)
            if (index == -1)
                return;

            chosenTargets.Add(self!);

            if (index > 0 && !self!.IsInFrontOfRoom())
                chosenTargets.Add(allValidTargets[index - 1]);

            if (index < allValidTargets.Count - 1 && !self!.IsInBackOfRoom())
                chosenTargets.Add(allValidTargets[index + 1]);
        }
    }
}
