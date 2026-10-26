using ShinyShoe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CharacterState;

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
    public sealed class RelicEffectClearStatusEndOfTurn : RelicEffectBase, ITurnPhaseEndOfTurnRelicEffect
    {
        private StatusEffectData.DisplayCategory displayCategory;
        private bool halveStatuses;
        private Team.Type sourceTeam;
        private VfxAtLoc? appliedVfx;
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
            using (GenericPools.Get<List<CharacterState>>(out var targets))
            {
                relicEffectParams.roomState.AddMovableCharactersToList(targets, sourceTeam);
                using (GenericPools.Get<CharacterState.RemoveStatusEffectParams>(out var removeStatusEffectParams))
                {
                    removeStatusEffectParams.sourceRelicState = SourceRelicState;
                    removeStatusEffectParams.sourceCardState = null;
                    removeStatusEffectParams.showNotification = true;
                    removeStatusEffectParams.fromEffectType = this.GetType();
                    foreach (var characterState in targets)
                    {
                        foreach (SubtypeData subtypeData in excludeCharacterSubtypes!)
                        {
                            if (characterState.GetHasSubtype(subtypeData))
                            {
                                continue;
                            }
                        }
                        RemoveStatusEffectsFromCharacter(characterState, removeStatusEffectParams);
                    }
                }
            }
            yield break;
        }

        private void RemoveStatusEffectsFromCharacter(CharacterState characterState, RemoveStatusEffectParams removeStatusEffectParams)
        {
            using (GenericPools.Get<List<CharacterState.StatusEffectStack>>(out var statusEffectStacks))
            {
                characterState.GetStatusEffects(ref statusEffectStacks);
                foreach (var statusEffect in statusEffectStacks)
                {
                    if (statusEffect.State.GetDisplayCategory() == displayCategory)
                    {
                        int num = statusEffect.Count;
                        if (halveStatuses)
                        {
                            num = Mathf.CeilToInt((float)num / 2f);
                        }
                        characterState.RemoveStatusEffect(statusEffect.State.GetStatusId(), num, removeStatusEffectParams, allowModification: false);
                        characterState.GetCharacterUI().ShowEffectVFX(characterState, appliedVfx);
                    }
                }
            }
        }
    }
}
