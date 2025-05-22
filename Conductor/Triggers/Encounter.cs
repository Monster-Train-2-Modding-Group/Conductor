using HarmonyLib;
using System.Collections;

namespace Conductor.Triggers
{
    // Handles the case where a new encounterer unit is summoned to the room.
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.OnSpawn))]
    class EncounterOnSpawnPatch
    {
        private static readonly List<CharacterState> outCharacters = [];

        public static IEnumerator Postfix(IEnumerator enumerator, CharacterState __instance, CombatManager ___combatManager)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }

            __instance.AssertNotDestroyed();

            if (!__instance.HasEffectTrigger(CharacterTriggers.Encounter))
            {
                yield break;
            }

            outCharacters.Clear();
            RoomState room = __instance.GetSpawnPoint().GetRoomOwner()!;
            room.AddCharactersToList(outCharacters, Team.Type.Monsters);

            foreach (CharacterState character in outCharacters)
            {
                if (character == __instance)
                {
                    continue;
                }
                yield return ___combatManager.QueueAndRunTrigger(__instance, CharacterTriggers.Encounter, fireTriggersData: new CharacterState.FireTriggersData
                {
                    overrideTargetCharacter = character,
                });
            }
        }
    }

    // Handles the case where an encounterer unit is in the room and another unit is spawned.
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.OnOtherCharacterSpawned))]
    class EncounterSeeingMonsterPatch
    {
        public static IEnumerator Postfix(IEnumerator enumerator, CharacterState __instance, CharacterState otherCharacter, CombatManager ___combatManager)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }

            if (!__instance.HasEffectTrigger(CharacterTriggers.Encounter))
            {
                yield break;
            }
            if (otherCharacter == null || otherCharacter.IsDestroyed || otherCharacter.GetSpawnPoint() == null || __instance.GetSpawnPoint() == null || !(otherCharacter.GetSpawnPoint().GetRoomOwner() == __instance.GetSpawnPoint().GetRoomOwner()))
            {
                yield break;
            }
            yield return ___combatManager.QueueAndRunTrigger(__instance, CharacterTriggers.Encounter, fireTriggersData: new CharacterState.FireTriggersData
            {
                overrideTargetCharacter = otherCharacter,
            });
        }
    }

    // Handles the case where an encounterer unit is ascended/descended or another case where a non-encounterer moves onto the floor with an encounterer.
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


            foreach (var character in cardEffectParams.targets)
            {
                if (character == null)
                {
                    continue;
                }

                outCharacters.Clear();
                RoomState room = character.GetSpawnPoint().GetRoomOwner()!;
                room.AddCharactersToList(outCharacters, Team.Type.Monsters);

                // Handle non-encounterer moving onto a floor with an encounterer.
                foreach (CharacterState other in outCharacters)
                {
                    if (!other.HasEffectTrigger(CharacterTriggers.Encounter))
                    {
                        continue;
                    }

                    if (character == other)
                    {
                        continue;
                    }

                    yield return coreGameManagers.GetCombatManager().QueueAndRunTrigger(other, CharacterTriggers.Encounter, fireTriggersData: new CharacterState.FireTriggersData
                    {
                        overrideTargetCharacter = character,
                    });
                }

                // Handle encounterer moving onto a floor with an non-encounterer.
                if (character.HasEffectTrigger(CharacterTriggers.Encounter))
                {
                    foreach (CharacterState other in outCharacters)
                    {
                        if (character == other)
                        {
                            continue;
                        }

                        yield return coreGameManagers.GetCombatManager().QueueAndRunTrigger(character, CharacterTriggers.Encounter, fireTriggersData: new CharacterState.FireTriggersData
                        {
                            overrideTargetCharacter = other,
                        });
                    }
                }
            }
        }
    }
}
