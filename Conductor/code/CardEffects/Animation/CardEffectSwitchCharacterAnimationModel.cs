using HarmonyLib;
using System.Collections;
using System.Reflection;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Card effect that switches a Character's ModelVariation
    /// 
    /// This is a highly specific effect so to prevent unexpected results the characters this effect targets must be whitelisted as not all characters will have a model variation
    /// Currently the only Vanilla unit is FunGuy and that is specifically controlled by the CharacterUI class.
    /// 
    /// This card effect bypasses that and allows you to easily swap to a different defined animation set on the character.
    /// </summary>
    public class CardEffectSwitchCharacterAnimationModel : CardEffectBase
    {
        readonly FieldInfo CharacterUI_DesiredModelIndex_Field = AccessTools.Field(typeof(CharacterUI), "desiredModelIndex");
        readonly HashSet<CharacterData> allowedCharacters = [];

        public override void Setup(CardEffectState cardEffectState)
        {
            base.Setup(cardEffectState);
            foreach (CharacterData characterData in cardEffectState.GetParamCharacterDataPool())
            {
                allowedCharacters.Add(characterData);
            }
        }
        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            if (coreGameManagers.GetSaveManager().PreviewMode)
                yield break;

            foreach (var character in cardEffectParams.targets)
            {
                if (!allowedCharacters.Contains(character.GetSourceCharacterData()))
                {
                    continue;
                }
                var charUI = character.GetCharacterUI();
                CharacterUI_DesiredModelIndex_Field.SetValue(charUI, cardEffectState.GetParamInt());
            }
        }

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamCharacterDataPool.GetFieldName()] = new PropDescription("Character Whitelist"),
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Model ID to switch to")
            };
        }
    }
}
