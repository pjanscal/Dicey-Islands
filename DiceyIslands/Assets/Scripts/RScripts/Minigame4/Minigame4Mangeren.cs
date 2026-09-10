using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Minigame4Mangeren : MonoBehaviour
{
    public static Minigame4Mangeren instance;

    [SerializeField] private GameObject potato;

    [HideInInspector] public bool isActive = false;
    [HideInInspector] public HashSet<(int, int)> plrHittedPlr = new(); //here go all the collision check that is going to happend so a void don't happend at the exact same time
    [HideInInspector] public Dictionary<int, Player_Minigame4> plrScripts = new();
    private HashSet<int> plrsIngame = new();
    private HashSet<int> plrsImunitty = new(); //plrs that can't get the potato
    private int potatoTarget = 0; //0 is like no one work as 1 if that plr is dead
    private bool canGivePotato = false;
    private bool canGivePotatoDebounce = true;
    private List<int> plrsPlaces = new(); //0 = last one 3 = first one //list have .indexOf so i don't have to find when debugging

    public Transform camaraHolder;

    //configs
    private Vector2 potatoLifeTime = new Vector2(10, 19);
    private Vector3 potatoOffet = new Vector3(0, 3, 0); //offset of being inside a player
    const float potatoRotateSpeed = .8f;
    const float potatoGivingDebounceDur = .2f;
    const float durBeforeGivingPlrThePotatoBack = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        GameMangeren.startMiniGame += Init;

        for (int plrId = 1; plrId <= LokaalConnecter.maxPlr; plrId++)
        {
            plrsIngame.Add(plrId);
        }

        StartCoroutine(PlayerHitDectetorWaitList());
    }

    //init when it start
    public void Init()
    {
        print("start minigame4");
        isActive = true; //say u can use it

        Invoke("GiveRandomPlrPotato", 3f); //starting time soon make a countdown
        potato.transform.DOLocalRotate(new Vector3(0, 360, 0), 1 / potatoRotateSpeed, RotateMode.FastBeyond360)
        .SetEase(Ease.Linear).SetLoops(-1); //settings
    }

    //wait for the potato to explode and do things when it explode
    IEnumerator PlayPotatoLifeTime()
    {
        float rngTime = UnityEngine.Random.Range(potatoLifeTime.x, potatoLifeTime.y);
        yield return new WaitForSeconds(rngTime);

        //kill the one with potato
        int target = potatoTarget;
        canGivePotato = false;
        plrsIngame.Remove(target);
        plrsPlaces.Add(target);

        potato.SetActive(false);
        potato.transform.SetParent(transform);
        plrScripts[target].gameObject.SetActive(false); //soon make it better

        print($"player{potatoTarget} is out of the game");

        //check or new round u only one left
        if (plrsIngame.Count == 1)
        {
            plrsPlaces.Add(plrsIngame.ElementAt(0));
            EndGame();
            yield break;
        }

        yield return new WaitForSeconds(3); //debug

        GiveRandomPlrPotato();
    }

    //the game ended show result and send him back to board
    void EndGame()
    {
        //check or this is not testing
        if (MatchData.Instance == null) {Debug.LogError("there is no mathData Script"); return;}

        MatchData.Instance.playerOrderNumbers.Clear();
        foreach (int plrId in plrsPlaces) //go from last to first
        {
            MatchData.Instance.playerOrderNumbers.Add(plrId); //save it to the matchData Mangeren

            Debug.Log($"MINIGAME RESULT - Place {4 - plrsPlaces.IndexOf(plrId)}: Player {plrId}");    
        }

        //go back
        MatchData.Instance.returningFromMinigame = true;
        GameMangeren.SwitchScene("BoardTestScene"); //beta so we can have it in a mangeren we all can get
    }

    //give the plr a potato
    void GivePlrPotato(int plrId)
    {
        Debug.LogWarning($"plr{plrId} have the new potato");
        
        //put the old potato inside the player;
        potatoTarget = plrId;
        potato.transform.SetParent(plrScripts[plrId].transform);
        potato.transform.localPosition = potatoOffet;
        plrScripts[plrId].movementType = Player_Minigame4.MovementType.potatoRunning;
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
            yield return new WaitForSeconds(potatoGivingDebounceDur);

            canGivePotatoDebounce = true;
        }

        IEnumerator TargetImunitty()
        {
            //give him immunity
            plrsImunitty.Add(oldPotatoTarget);
            plrScripts[oldPotatoTarget].movementType = Player_Minigame4.MovementType.Running;

            //wait
            yield return new WaitForSeconds(durBeforeGivingPlrThePotatoBack);

            //delete it
            plrScripts[oldPotatoTarget].movementType = Player_Minigame4.MovementType.walking;
            plrsImunitty.Remove(oldPotatoTarget);
        }

        plrHittedPlr.Clear();
        GivePlrPotato(target);
    }

    //give a random player a potato
    void GiveRandomPlrPotato()
    {
        //get random player
        int rng = UnityEngine.Random.Range(1, plrsIngame.Count);
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
    
}
