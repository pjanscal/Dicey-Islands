using UnityEngine;

//shortcuts
using AnimationBoolEvent = CharacterLoader.CharactersAnimationBooleanEvent;
using AnimationTriggerEvent = CharacterLoader.CharactersAnimationTriggerEvent;

public class CutSceneChampion : MonoBehaviour
{
    [SerializeField] private CharacterLoader character;

    public void StartRunning()
    {
        character.SetAnimationBool(AnimationBoolEvent.IsRunning, true);
        character.animator.Play("Running");
    }

    public void ActiveJump()
    {
        character.UseAnimation(AnimationTriggerEvent.ActiveJump);
    }
}
