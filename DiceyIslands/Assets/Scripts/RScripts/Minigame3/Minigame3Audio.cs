using UnityEngine;

public class Minigame3Audio : MonoBehaviour
{
    public static Minigame3Audio instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip daggerThrowSound;
    [SerializeField] private AudioClip playerEliminatedSound;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (musicAudioSource != null &&
            backgroundMusic != null)
        {
            musicAudioSource.clip =
                backgroundMusic;

            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
    }

    public void PlayDaggerThrowSound()
    {
        if (sfxAudioSource == null ||
            daggerThrowSound == null)
            return;

        sfxAudioSource.PlayOneShot(
            daggerThrowSound
        );
    }

    public void PlayPlayerEliminatedSound()
    {
        if (sfxAudioSource == null ||
            playerEliminatedSound == null)
            return;

        sfxAudioSource.PlayOneShot(
            playerEliminatedSound
        );
    }
}