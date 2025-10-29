## v0.2.3
- Changed overrideTargetCharacter for FollowUp and Vengeance to the character that was damaged which caused the trigger to fire. The attacking character can still be gotten by last_attacker_character, the damaged character can not. 

## v0.2.2
- Add API for Custom TrackedValues.
- Add TrackedValue BlightsAndScourgesInDeck

## v0.2.1
- Fixed Ambiguous method reference in Utilites.SetupCardEffectTooltips
- Added support for implementing TargetModes.
- Added 11 New TargetModes: (played_card, override_target_character, in_front_of_self, behind_self, around_self, strongest, highest_attack, lowest_attack, highest_attack_all_rooms, lowest_attack_all_rooms, highest_attack_excluding_self)
- Remove unused setter from IConstructStatusArmorModifier
- Add Resonance trigger