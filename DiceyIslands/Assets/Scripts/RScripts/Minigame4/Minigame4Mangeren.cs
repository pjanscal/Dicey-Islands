using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Minigame4Mangeren : MonoBehaviour
{
    public static Minigame4Mangeren instance;

    [HideInInspector] public bool isActive = false;
    [HideInInspector] public Dictionary<int, Player_Minigame4> plrScripts = new();
    private HashSet<int> plrsIngame = new();
    private int potatoTarget = 0; //0 is like no one work as 1 if that plr is dead
    private bool canGivePotato = false;

    public Transform camaraHolder;

    //configs
    private Vector2 potatoLifeTime = new Vector2(14, 20);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        GameMangeren.startMiniGame += Init;

        for (int plrId = 1; plrId <= LokaalConnecter.maxPlr; plrId++)
        {
            plrsIngame.Add(plrId);
        }
    }

    //init when it start
    public void Init()
    {
        print("start minigame4");
        isActive = true; //say u can use it

        Invoke("GiveRandomPlrPotato", 3f); //starting time soon make a countdown
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator PlayPotatoLifeTime()
    {
        float rngTime = UnityEngine.Random.Range(potatoLifeTime.x, potatoLifeTime.y);
        yield return new WaitForSeconds(rngTime);

        //kill the one with potato
        canGivePotato = false;
        plrsIngame.Remove(potatoTarget);
        print($"player{potatoTarget} is out of the game");

        GiveRandomPlrPotato();
    }

    void GivePlrPotato(int plrId)
    {
        //put the old potato inside the player;
        potatoTarget = plrId;
    }

    void PlayerCollideWithOtherPlayer(int owner, int target)
    {
        if (potatoTarget != owner || !canGivePotato) return; //then it wouold be useless the script

        GivePlrPotato(target);
    }

    void GiveRandomPlrPotato()
    {
        //get random player
        int rng = UnityEngine.Random.Range(1, plrsIngame.Count);
        int plrId = plrsIngame.ElementAt(rng);
        print($"player{plrId} have potato");

        //set it up
        canGivePotato = true;
        GivePlrPotato(plrId);

        //start it
        StartCoroutine(PlayPotatoLifeTime());
    }
}
