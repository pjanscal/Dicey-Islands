using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Minigame4Mangeren : MonoBehaviour
{
    public static Minigame4Mangeren instance;

    [SerializeField] private GameObject potato;
    [SerializeField] private ParticleSystem bombVfx;

    [HideInInspector] public bool isActive = false;
    [HideInInspector] public HashSet<(int, int)> plrHittedPlr = new(); //here go all the collision check that is going to happend so a void don't happend at the exact same time
    [HideInInspector] public Dictionary<int, PlayerMinigame4> plrScripts = new();
    [HideInInspector] public HashSet<int> plrsIngame = new();
    private HashSet<int> plrsImunitty = new(); //plrs that can't get the potato
    [HideInInspector] public int potatoTarget = 0; //0 is like no one work as 1 if that plr is dead
    private bool canGivePotato = false;
    private bool canGivePotatoDebounce = true;
    private List<int> plrsPlaces = new(); //0 = last one 3 = first one //list have .indexOf so i don't have to find when debugging
    public Transform camaraHolder;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource tickingAudioSource;

    [SerializeField] private AudioClip playerTaggedSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip tickingSound;

    [Header("Ticking Settings")]
    [SerializeField] private float tickingStartPitch = 0.8f;
    [SerializeField] private float tickingEndPitch = 2.0f;

    //configs
    private Vector2 potatoLifeTime = new Vector2(10, 19);
    private Vector3 potatoOffet = new Vector3(0, 2, 0); //offset of being inside a player
    const float timeBeforeGivingARandomBomb = 3f;
    const float potatoRotateSpeed = .8f;
    const float potatoGivingCooldown = .2f;
    const float plrImmunityDur = 1f; //time before the player can get the potato again

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;

        for (int plrId = 1; plrId <= LokaalConnecter.maxPlr; plrId++)
        {
            plrsIngame.Add(plrId);
        }

        StartCoroutine(PlayerHitDectetorWaitList());
    }

    void Start()
    {
        GameMangeren.startMiniGame += Init;

        if (musicAudioSource != null &&
            backgroundMusic != null)
        {
            musicAudioSource.clip = backgroundMusic;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
    }

    #region Audio
    private void PlayTaggedSound()
    {
        if (sfxAudioSource == null ||
            playerTaggedSound == null)
            return;

        sfxAudioSource.PlayOneShot(
            playerTaggedSound
        );
    }

    private void PlayExplosionSound()
    {
        if (sfxAudioSource == null ||
            explosionSound == null)
            return;

        sfxAudioSource.PlayOneShot(
            explosionSound
        );
    }
    #endregion

    //init when it start
    public void Init()
    {
        isActive = true; //say u can use it

        Invoke("GiveRandomPlrPotato", timeBeforeGivingARandomBomb); //starting time
        potato.transform.DOLocalRotate(new Vector3(potato.transform.localEulerAngles.x, 360, potato.transform.localEulerAngles.z), 1 / potatoRotateSpeed, RotateMode.FastBeyond360)
        .SetEase(Ease.Linear).SetLoops(-1); //settings
    }

    //wait for the potato to explode and do things when it explode
    IEnumerator PlayPotatoLifeTime()
    {
        float rngTime = UnityEngine.Random.Range(potatoLifeTime.x, potatoLifeTime.y);
        bool exploded = false;

        // Start ticking.
        if (tickingAudioSource != null &&
            tickingSound != null)
        {
            tickingAudioSource.clip =
                tickingSound;

            tickingAudioSource.loop = true;

            tickingAudioSource.pitch =
                tickingStartPitch;

            tickingAudioSource.Play();
        }

        // Count toward the explosion.
        DOTween.To(() => tickingAudioSource.pitch, //get the values
         x => tickingAudioSource.pitch = x, //function while it do it
         tickingEndPitch, rngTime)
         .OnComplete(() => exploded = true);
        yield return new WaitUntil(() => exploded);

        // Explosion reached.
        if (tickingAudioSource != null)
        {
            tickingAudioSource.Stop();
            tickingAudioSource.pitch =
                tickingStartPitch;
        }

        PlayExplosionSound();

        //Kill the player holding the potato.
        int target = potatoTarget;
        canGivePotato = false;

        plrsIngame.Remove(target);
        plrsPlaces.Add(target);
        bombVfx.transform.position = plrScripts[target].transform.position;
        bombVfx.Play();
        potato.SetActive(false);
        potato.transform.SetParent(transform);
        plrScripts[target].gameObject.SetActive(false); //soon make it better

        print($"player{potatoTarget} is out of the game");

        //check or there is only 1 player left
        if (plrsIngame.Count == 1)
        {
            plrsPlaces.Add(plrsIngame.ElementAt(0));
            EndGame();
            yield break;
        }

        yield return new WaitForSeconds(timeBeforeGivingARandomBomb);

        GiveRandomPlrPotato();
    }

    //the game ended show result and send him back to board
    void EndGame()
    {
        //check or this is not testing
        if (MatchData.Instance == null) {Debug.LogError("there is no mathData Script"); return;}

        MatchData.Instance.playerOrderNumbers.Clear();
        plrsPlaces.Reverse();
        foreach (int plrId in plrsPlaces) //go from first to last
        {
            MatchData.Instance.playerOrderNumbers.Add(plrId); //save it to the matchData Mangeren

            Debug.Log($"MINIGAME RESULT - Place {1 + plrsPlaces.IndexOf(plrId)}: Player {plrId}");    
        }

        GameMangeren.MinigameWinner(plrsPlaces);
    }

    #region Give Potato Logic
    //give the plr a potato
    void GivePlrPotato(int plrId)
    {
        Debug.LogWarning($"plr{plrId} have the new potato");
        
        //put the old potato inside the player;
        potatoTarget = plrId;
        potato.transform.SetParent(plrScripts[plrId].transform);
        potato.transform.localPosition = potatoOffet;
        plrScripts[plrId].movementType = PlayerMinigame4.MovementType.potatoRunning;
    }

    //wait for a plr to hit someone and send a message to playercollidewithotherplayer void
    IEnumerator PlayerHitDectetorWaitList()
    {
        while (true)
        {
            //when contine of going again it is gone
            if (plrHittedPlr.Count >= 1) plrHittedPlr.Remove(plrHittedPlr.ElementAt(0)); //delete the first one

            yield return new WaitUntil(() => plrHittedPlr.Count >= 1); //wait until one is added

            var (owner, target) = plrHittedPlr.ElementAt(0); //get the info of the owner who hitted a plr
            PlayerCollideWithOtherPlayer(owner, target);
        }
    }

    //init when player touch a other player
    void PlayerCollideWithOtherPlayer(int owner, int target)
    {
        if (potatoTarget != owner && potatoTarget != target || !canGivePotato || !canGivePotatoDebounce) return; //make sure one of them have it
        
        int oldPotatoTarget = potatoTarget != owner? target : owner; //get the owner of the potato
        target = potatoTarget != target? target : owner; //if the target have potato then make the owner the target
        if (plrsImunitty.Contains(target)) return; //check or it have not imunitty

        canGivePotatoDebounce = false;
        StartCoroutine(WaitToGivePotato());
        StartCoroutine(TargetImunitty());

        IEnumerator WaitToGivePotato()
        {
            yield return new WaitForSeconds(potatoGivingCooldown);

            canGivePotatoDebounce = true;
        }

        IEnumerator TargetImunitty()
        {
            //give him immunity
            plrsImunitty.Add(oldPotatoTarget);
            plrScripts[oldPotatoTarget].movementType = PlayerMinigame4.MovementType.Running;

            //wait
            yield return new WaitForSeconds(plrImmunityDur);

            //delete it
            plrScripts[oldPotatoTarget].movementType = PlayerMinigame4.MovementType.walking;
            plrsImunitty.Remove(oldPotatoTarget);
        }

        plrHittedPlr.Clear();
        GivePlrPotato(target);

        // Potato successfully transferred.
        PlayTaggedSound();
    }

    //give a random player a potato
    void GiveRandomPlrPotato()
    {
        //get random player
        int rng = UnityEngine.Random.Range(0, plrsIngame.Count - 1);
        int plrId = plrsIngame.ElementAt(rng);
        print($"player{plrId} have potato");

        //set it up
        canGivePotato = true;
        GivePlrPotato(plrId);
        potato.transform.localPosition = potatoOffet;
        potato.SetActive(true);

        //start it
        StartCoroutine(PlayPotatoLifeTime());
    }
    #endregion
}
