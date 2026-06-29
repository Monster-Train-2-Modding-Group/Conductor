## v0.5.8
- Added ability for mods to have save data.
- Added PlayerResource class
- Added RoomModifierDamagePerAttackModifier

## v0.5.7
- SimpleTrackedValueHandler is now preview aware, now has a preview state and current state.
- AbstractTrackedValueHandler has functions that get called when Previews are enabled/disabled.
- Updated localizations.

## v0.5.6
- Add Utilities.MarkEffectAsADamageEffect
- Add Utilities.MarkEffectAsHealEffect
- Add Utilities.MarkEffectAsStatusGivingEffect

## v0.5.5
- Added Absolve (triggers when a Blight/Scourge is purged)
- Added Vanish (triggers when an Ephemeral card is played or discarded)
- Added Furnish (triggers when a Room is played)
- Added missing Recoil icons.
- Added TriggerOnCardPurged event.

## v0.5.4
- Added Recoil (character takes damage equal to stacks after attacking)
- Added Trinity (genericized version from MT2 modding tutorials)

## v0.5.3
- AbstractTrackedValueHandler now has access to all of the managers.
- Add CardEffectRequireTargetsDestroyed.
- Fox Scaling CardTraits now respect the param_card parameter.
- Fix CardEffectShuffleUnits triggering shift multiple times
- Steelguard removed (Developers made Steelguard an actual status effect, which inactivated Conductor's version).

## v0.5.2
- Steelguard can now be referenced without a mod_reference. Use "steelguard".
- Steelguard now sets damageBlocked.
- Fix steelguard localizations and param substitution.

## v0.5.1
- Fix mistakenly included file
- Ability to create status effects that change the target mode, see IChangeAttackingTargetModeStatusEffect.
- Add fixes for Dormant to work properly.

## v0.5.0
- Fixes for Destiny of the Railforged 2.1
- Add actual steelguard status effect (steelguard is not a true status effect in base game.)
- Add sprite for unused Steelmight status effect.

## v0.4.1
- Remove debug logging.
- Remove Mobilize Trigger code (It is the same as Rally now).
- Internal touchups.

## v0.4.0
- Fixes for Destiny of the Railforged.

## v0.3.2
- Add Binder trigger.
- Add ability to associate a sound cue with a custom Damage.Type

## v0.3.1
- Just more logging.

## v0.3.0
- Breaking change: Conductor.Interfaces.ITrackedValueHandler is private, users should switch to inheriting Conductor.TrackedValues.AbstractTrackedValueHandler
- Breaking change: Conductor.Interfaces.CharacterTargetSelector and Conductor.Interfaces.CardTargetSelector moved to Conductor.TargetModes namespace.
- Ability to create ClassMechanicHUDs.

## v0.2.9
- Ability to alias or combine triggers.
- Added CardEffectEnchant and AfterSpawnBetterEnchant triggers
- Added Brambles status effect
- Added Utilities.AddCardEffectDisplay for a CardEffect to replace a trigger icon.
- Allowed AllowTriggerToFirePreCharacterTriggerStatus to be called on Vanilla Triggers.

## v0.2.8
- Fix Heroic status dealing double the effects damage.

## v0.2.7
- Fix on_shift not being present as an identifier
- Fix on_shift not triggering on normal enemy ascending.

## v0.2.6
- Fix Utilities.SetupTraitTooltips assembly is optional.

## v0.2.5
- Just a rebuild against MonsterTrain2 v1.3.1

## v0.2.4
- TargetModes support for Abyssal Prism. Adds ResolvesToSingleTarget to CharacterTargetSelector.
- Essence processing and storage.
- new Status effect Heroic. Allied units have +1 attack for each stack.
- new Status effect Growth. Unit has +1 max health per stack.
- Expose abandoned Piercing status.
- OnShift StatusEffectTriggerStage
- Rounded tooltip edges for tooltip status effect icons

## v0.2.3
- Changed overrideTargetCharacter for FollowUp and Vengeance to the character that was damaged which caused the trigger to fire. The attacking character can still be gotten by last_attacker_character, the damaged character can not. 

## v0.2.2
- Add API for Custom TrackedValues.
- Add TrackedValue BlightsAndScourgesInDeck

## v0.2.1
- Fixed Ambiguous method reference in Utilities.SetupCardEffectTooltips
- Added support for implementing TargetModes.
- Added 11 New TargetModes: (played_card, override_target_character, in_front_of_self, behind_self, around_self, strongest, highest_attack, lowest_attack, highest_attack_all_rooms, lowest_attack_all_rooms, highest_attack_excluding_self)
- Remove unused setter from IConstructStatusArmorModifier
- Add Resonance trigger