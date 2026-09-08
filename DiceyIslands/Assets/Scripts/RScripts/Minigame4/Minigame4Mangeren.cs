using UnityEngine;

public class Minigame4Mangeren : MonoBehaviour
{
    public static Minigame4Mangeren instance;

    public bool isActive = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        GameMangeren.startMiniGame += Init;
    }

    //init when it start
    public void Init()
    {
        print("start minigame4");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
