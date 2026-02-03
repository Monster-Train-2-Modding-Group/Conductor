using Conductor.Extensions;
using Conductor.Triggers;
using HarmonyLib;
using ShinyShoe;
using System.Collections;
using UnityEngine.TextCore.Text;

namespace Conductor.Patches
{
    // Handles the case where a new encounterer unit is summoned to the room.
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.OnSpawn))]
    class EncounterOnSpawnPatch
    {
        public static IEnumerator Postfix(IEnumerator enumerator, CharacterState __instance, CombatManager ___combatManager)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }

            __instance.AssertNotDestroyed();

            // Trigger Encounter for newly spawned character with overrideTarget being each character in room.
            List<CharacterState> outCharacters;
            using (GenericPools.GetList(out outCharacters))
            {
                RoomState? room = __instance.GetCurrentRoom(true);
                if (room == null)
                {
                    Plugin.Logger.LogError($"Character {__instance} not found in a room or does not exist?");
                    yield break;
                }
                room.AddCharactersToList(outCharacters, __instance.GetTeamType());

                foreach (CharacterState character in outCharacters)
                {
                    if (character == __instance)
                    {
                        continue;
                    }
                    ___combatManager.QueueTrigger(__instance, CharacterTriggers.Encounter, fireTriggersData: new CharacterState.FireTriggersData
                    {
                        paramInt = outCharacters.Count,
                        overrideTargetCharacter = character,
                    });
                }
            }
            yield return ___combatManager.RunTriggerQueue();
        }
    }

    // Handles the case where an encounterer unit is in the room and another unit is spawned.
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.OnOtherCharacterSpawned))]
    class EncounterSeeingMonsterPatch
    {
        public static IEnumerator Postfix(IEnumerator enumerator, CharacterState __instance, CharacterState otherCharacter, CombatManager ___combatManager, AllGameManagers ___allGameManagers)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }

            if (otherCharacter == null || otherCharacter.IsDestroyed || otherCharacter.GetSpawnPoint() == null || __instance.GetSpawnPoint() == null || !(otherCharacter.GetSpawnPoint().GetRoomOwner() == __instance.GetSpawnPoint().GetRoomOwner()))
            {
                yield break;
            }

            int numCharacters = __instance.GetCurrentRoom()?.GetNumCharacters(__instance.GetTeamType()) ?? 0;

            var data = new TriggerOnAnotherSpawnParams
            {
                Character = __instance,
                SpawnedCharacter = otherCharacter,
                Room = __instance.GetCurrentRoom(),
                RoomIndex = __instance.GetCurrentRoomIndex(),
                CoreGameManagers = ___allGameManagers.GetCoreManagers(),
            };

            foreach (var trigger_func in CharacterTriggerExtensions.TriggersOnAnotherSpawn)
            {
                if (trigger_func.Value(data, out var queueTriggerParams))
                {
                    ___combatManager.QueueCustomTrigger(__instance, trigger_func.Key, queueTriggerParams);
                }
            }    

            // Encounter has already been triggered for otherCharacter/__instance in OnSpawned. so trigger the opposite case
            ___combatManager.QueueTrigger(__instance, CharacterTriggers.Encounter, fireTriggersData: new CharacterState.FireTriggersData
            {
                paramInt = numCharacters,
                overrideTargetCharacter = otherCharacter,
            });

            yield return ___combatManager.RunTriggerQueue();
        }
    }

    // Handles the case where an encounterer unit is ascended/descended or another case where a non-encounterer moves onto the floor with an encounterer.
    // No need to handle Flight (teleportation) as those card effects call this one.
    [HarmonyPatch(typeof(CardEffectBump), nameof(CardEffectBump.Bump))]
    class CardEffectBumpEncounterPatch
    {
        private static readonly List<CharacterState> outCharacters = [];

        public static IEnumerator Postfix(IEnumerator enumerator, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }

            CombatManager combatManager = coreGameManagers.GetCombatManager();

            foreach (var character in cardEffectParams.targets)
            {
                RoomState? room = character?.GetCurrentRoom(true);
                if (character == null || room == null)
                {
                    Plugin.Logger.LogError($"Character {character} not found in a room or does not exist?");
                    continue;
                }
                // Handle non-encounterer moving onto a floor with an encounterer.
                List<CharacterState> outCharacters;
                using (GenericPools.GetList(out outCharacters))
                {
                    room.AddCharactersToList(outCharacters, Team.Type.Monsters);
                    foreach (CharacterState other in outCharacters)
                    {
                        if (character == other)
                        {
                            continue;
                        }
                        combatManager.QueueTrigger(other, CharacterTriggers.Encounter, fireTriggersData: new CharacterState.FireTriggersData
                        {
                            paramInt = outCharacters.Count,
                            paramInt2 = 1,
                            overrideTargetCharacter = character,
                        });
                        combatManager.QueueTrigger(character, CharacterTriggers.Encounter, fireTriggersData: new CharacterState.FireTriggersData
                        {
                             paramInt = outCharacters.Count,
                             paramInt2 = 1,
                             overrideTargetCharacter = other,
                        });
                    }
                }
            }
            yield return combatManager.RunTriggerQueue();
        }
    }
}
