using ShinyShoe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Conductor.RelicEffects
{
    /// <summary>
    /// A relic effect that clears status effects of a certain category at the end of the turn.
    /// 
    /// Example Json
    /// "relic_effects": [
    ///  {
    ///    "id": "ClearUnitDebuffsAtEndOfTurn",
    ///    "name": {
    ///      "id": "@RelicEffectClearStatusEndOfTurn",
    ///      "mod_reference": "Conductor"
    ///    },
    ///    "param_int": 1,
    ///    "source_team": "monsters"
    ///  }
    /// ]
    /// </summary>
    public sealed class RelicEffectClearStatusEndOfTurn : RelicEffectBase, ITurnPhaseEndOfTurnRelicEffect, IRelicEffect
    {
        private StatusEffectData.DisplayCategory displayCategory;
        private bool halveStatuses;
        private Team.Type sourceTeam;
        private VfxAtLoc? appliedVfx;
        private List<CharacterState> targets = [];
        private List<CharacterState.StatusEffectStack> statusEffectStacks = [];
        private SubtypeData[]? excludeCharacterSubtypes;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [RelicEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("StatusEffectData.DisplayCategory Enum"),
                [RelicEffectFieldNames.ParamSourceTeam.GetFieldName()] = new PropDescription("Target Team"),
                [RelicEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("Halve statuses instead of removing all"),
                [RelicEffectFieldNames.ParamExcludeCharacterSubtypes.GetFieldName()] = new PropDescription("Excluded Character Subtypes")
            };
        }

        public override void Initialize(RelicState relicState, RelicData relicData, RelicEffectData relicEffectData)
        {
            base.Initialize(relicState, relicData, relicEffectData);
            displayCategory = (StatusEffectData.DisplayCategory) relicEffectData.GetParamInt();
            halveStatuses = relicEffectData.GetParamBool();
            sourceTeam = relicEffectData.GetParamSourceTeam();
            appliedVfx = relicEffectData.GetAppliedVfx();
            excludeCharacterSubtypes = relicEffectData.GetParamExcludeCharacterSubtypes() ?? [];
        }

        public bool TestEffectEndOfTurn(EndOfTurnRelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers)
        {
            return true;
        }

        public IEnumerator ApplyEffectEndOfTurn(EndOfTurnRelicEffectParams relicEffectParams, ICoreGameManagers coreGameManagers)
        {
            targets.Clear();
            relicEffectParams.roomState.AddMovableCharactersToList(targets, sourceTeam);
            CharacterState.RemoveStatusEffectParams removeStatusEffectParams;
            using (GenericPools.Get<CharacterState.RemoveStatusEffectParams>(out removeStatusEffectParams))
            {
                removeStatusEffectParams.sourceRelicState = SourceRelicState;
                removeStatusEffectParams.sourceCardState = null;
                removeStatusEffectParams.showNotification = true;
                removeStatusEffectParams.fromEffectType = this.GetType();
                foreach (var characterState in targets)
                {
                    bool flag = false;
                    foreach (SubtypeData subtypeData in excludeCharacterSubtypes!)
                    {
                        if (characterState.GetHasSubtype(subtypeData))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        continue;
                    }

                    characterState.GetStatusEffects(ref statusEffectStacks);
                    for (int j = 0; j < statusEffectStacks.Count; j++)
                    {
                        if (statusEffectStacks[j].State.GetDisplayCategory() == displayCategory)
                        {
                            int num = statusEffectStacks[j].Count;
                            if (halveStatuses)
                            {
                                num = Mathf.CeilToInt((float)num / 2f);
                            }
                            characterState.RemoveStatusEffect(statusEffectStacks[j].State.GetStatusId(), num, removeStatusEffectParams, allowModification: false);
                            characterState.GetCharacterUI().ShowEffectVFX(characterState, appliedVfx);
                        }
                    }
                    statusEffectStacks.Clear();
                }
            }
            yield break;
        }
    }
}
