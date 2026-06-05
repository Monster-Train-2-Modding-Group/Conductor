using Conductor.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.CardEffects.Conditions
{
    /// <summary>
    /// Card effect that requires the character to be destroyed (died without any stacks of reanimate).
    /// Optimally this card effect should be placed in an Extinguish trigger
    /// The main use-case of this CardEffect is to stop processing effects if this one fails.
    /// 
    /// Test fails if:
    ///    There is no selfTarget (not within a CharacterTrigger).
    ///    The selfTarget is alive, or dead with stacks of reanimate.
    /// 
    /// Example Json.
    /// 
    /// "effects": [
    ///   {
    ///     "id": "RequireAtLeastTwo",
    ///     "name": {
    ///       "id": "@CardEffectRequireNumTargets",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "target_mode": "room",
    ///     "target_team": "monsters",
    ///     "param_bool": false
    ///   }
    /// ]
    /// </summary>
    public class CardEffectRequireTargetsDestroyed : CardEffectBase
    {

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.UseIntRange.GetFieldName()] = new PropDescription("Enable min and max testing"),
                [CardEffectFieldNames.ParamMinInt.GetFieldName()] = new PropDescription("The minimum targets that must be destroyed to pass the test."),
                [CardEffectFieldNames.ParamMaxInt.GetFieldName()] = new PropDescription("The maximum targets that must be destroyed to pass the test."),
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("ALL VS ANY. If true, any target must be destroyed, otherwise all targets specified must be destroyed."),
                [CardEffectFieldNames.ParamBool3.GetFieldName()] = new PropDescription("Inverse. If true then the test's outcome is inverted.")
            };
        }

        private int minimumDestroyed;
        private int maximumDestroyed;
        private bool enableMinMaxTest;
        private bool requireAnyDestroyed;
        private bool invertResult;

        public override void Setup(CardEffectState cardEffectState)
        {
            base.Setup(cardEffectState);
            enableMinMaxTest = cardEffectState.GetUseIntRange();
            minimumDestroyed = cardEffectState.GetParamMinInt();
            maximumDestroyed = cardEffectState.GetParamMaxInt();
            requireAnyDestroyed = cardEffectState.GetParamBool();
            invertResult = cardEffectState.GetParamBool3();
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            int count = 0;
            int total = cardEffectParams.targets.Count;
            foreach (var target in cardEffectParams.targets)
            {
                if (target.IsDeadAndUnrevivable)
                    count++;
            }
            bool flag1 = (enableMinMaxTest && count >= minimumDestroyed && count <= maximumDestroyed) || (!enableMinMaxTest);
            bool flag2 = count > 0 && (requireAnyDestroyed || (!requireAnyDestroyed && count == total));
            return invertResult ? !(flag1 && flag2) : (flag1 && flag2);
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            yield break;
        }
    }
}
