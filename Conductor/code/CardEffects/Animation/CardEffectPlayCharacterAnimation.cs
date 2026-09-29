

using ShinyShoe;
using System.Collections;

namespace Conductor.CardEffects
{
    /// <summary>
    /// CardEffect that allows you to play a specific animation.
    /// 
    /// This CardEffect should only be used as an effect of a card.
    /// Do not use within a CharacterTrigger as in that case that natively supports playing an animation based of anim_to_play when the effect fires.
    /// 
    /// Note that Hover and Talk enum values should not be used as they will immediately get cancelled when the card is played.
    /// 
    /// Note if this is used to play a looping animation it will not persist if the user undoes the turn for that use case instead switch the model
    ///   with CardEffectSwitchCharacterAnimationModel and have that be the new idle animation.
    /// 
    /// Also supports playing a Global sound effect via param_str
    /// </summary>
    public class CardEffectPlayCharacterAnimation : CardEffectBase
    {
        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var saveManager = coreGameManagers.GetSaveManager();
            if (saveManager.PreviewMode || saveManager.GetActiveGameSpeed() == SaveManager.GameSpeed.Instant)
                yield break;

            string sfxCue = cardEffectState.GetParamStr();

            if (sfxCue.IsNullOrEmpty())
                sysManagers.GetSoundManager().PlaySfx(sfxCue);

            foreach (var character in cardEffectParams.targets)
            {
                character.GetCharacterUI().DoExecuteActionAnimation(cardEffectState.GetSourceCardEffectData().GetAnimToPlay());
            }
        }

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamStr.GetFieldName()] = new PropDescription("Sound effect to play"),
                ["animToPlay"] = new PropDescription("Animation to play")
            };
        }
    }
}
