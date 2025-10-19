using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.CardTraits
{
    /// <summary>
    /// CardTrait that modifies a TrackedValue on the card when played
    /// 
    /// Note that this CardTrait does not apply in preview mode, so if you have
    /// follow-up CardEffects they can not depend on the TrackedValue otherwise
    /// the preview result of the card being played will be incorrect.
    /// 
    /// Example json:
    /// "traits": [
    ///   {
    ///     "id": "IncreaseTrackedValue",
    ///     "name": {
    ///       "id": "@CardTraitModifyTrackedValue",
    ///       "mod_reference": "Conductor"
    ///     },
    ///     "param_tracked_value": {
    ///       "id": "@NewTrackedValue"
    ///     },
    ///     "param_int": 1
    ///   }
    /// ],
    /// "tracked_value_types": [
    ///   {
    ///     "id": "NewTrackedValue"
    ///   }
    /// ]
    /// </summary>
    public class CardTraitModifyTrackedValue : CardTraitState
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions {
                [CardTraitFieldNames.ParamTrackedValue.GetFieldName()] = new PropDescription("Tracked Value to modify."),
                [CardTraitFieldNames.ParamInt.GetFieldName()] = new PropDescription("Amount to modify Tracked Value by.")
            };
        }

        public override IEnumerator OnCardPlayed(CardState thisCard, ICoreGameManagers coreGameManagers)
        {
            if (coreGameManagers.GetSaveManager().PreviewMode)
                yield break;

            coreGameManagers.GetCardStatistics().IncrementStat(thisCard, GetParamTrackedValue(), GetParamInt());
        }
    }
}
