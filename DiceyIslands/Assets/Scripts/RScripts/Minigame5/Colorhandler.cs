using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Colorhandler : MonoBehaviour
{
    public static Colorhandler instance;

    public RawImage[] mainColors;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;

    [SerializeField] private AudioClip playerReadySound;
    [SerializeField] private AudioClip allPlayersReadySound;
    [SerializeField] private AudioClip backgroundMusic;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SetRandomColor();

        // Start background music.
        if (musicAudioSource != null &&
            backgroundMusic != null)
        {
            musicAudioSource.clip =
                backgroundMusic;

            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
    }

    void SetRandomColor()
    {
        Color randomColor = Random.ColorHSV();

        randomColor.r = Mathf.Max(randomColor.r, 40f / 255f);
        randomColor.g = Mathf.Max(randomColor.g, 40f / 255f);
        randomColor.b = Mathf.Max(randomColor.b, 40f / 255f);

        foreach (RawImage mainColor in mainColors)
        {
            mainColor.color = randomColor;
        }

        Debug.Log($"Main color: {randomColor}");
    }

    public void PlayPlayerReadySound()
    {
        if (sfxAudioSource == null ||
            playerReadySound == null)
            return;

        sfxAudioSource.PlayOneShot(
            playerReadySound
        );
    }

    public void PlayAllPlayersReadySound()
    {
        if (sfxAudioSource == null ||
            allPlayersReadySound == null)
            return;

        sfxAudioSource.PlayOneShot(
            allPlayersReadySound
        );
    }
}