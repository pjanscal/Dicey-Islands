using UnityEngine;

public class SoundEffectOfButton : MonoBehaviour
{
    [Header("sfx")]
    [SerializeField] private AudioSource onclickSfx;

    public void Click()
    {
        onclickSfx.Play();
    }
}
