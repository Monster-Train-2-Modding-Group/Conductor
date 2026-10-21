namespace Conductor.Interfaces
{
    /// <summary>
    /// Interface to make a status effect able to modify the trigger fire count for any triggers.
    /// </summary>
    public interface IStatusEffectModifyTriggerFireCount
    {
        /// <summary>
        /// Gets additional trigger fire count.
        /// </summary>
        /// <param name="trigger">Trigger type.</param>
        /// <param name="triggerStateContext">The firing trigger state.</param>
        /// <returns>Number of additional times to fire the trigger.</returns>
        int GetTriggerFireCount(CharacterTriggerData.Trigger trigger, CharacterTriggerState? triggerStateContext);
    }
}
