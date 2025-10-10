using ShinyShoe.Logging;

namespace Conductor.StatusEffects
{
    /// <summary>
    /// Example status effect that makes the damage always be one.
    /// </summary>
    class StatusEffectIntangibleState : StatusEffectState
    {
        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            if (inputTriggerParams.attacked == null)
            {
                Plugin.Logger.LogError("StatusEffectIntangibleState.OnPreAttacked() attacked character should not be null!");
                return false;
            }
            if (inputTriggerParams.damage > 0)
            {
                outputTriggerParams.damageBlocked = inputTriggerParams.damage - 1;
                outputTriggerParams.damage = 1;

                return true;
            }
            return false;
        }
    }
}
