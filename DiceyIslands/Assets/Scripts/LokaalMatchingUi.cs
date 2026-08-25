using System.Collections.Generic;
using UnityEngine;

public class LokaalMatchingUi : MonoBehaviour
{
    //everything about matchUi logic function is in here
    //reason why a new one because we need a public void with Ui and other ui logic i will see
    //fixed i might do make the slots here

    public static LokaalMatchingUi instance; //let other scripts use this

    //ui image that can be used
    public Sprite nothingEnabledUi;
    public Sprite controllerUi;
    public Sprite devUi;

    private Canvas canvas;

    public enum ConnectionTypes
    {
        Join, //when a controller join
        JoinDev, //with keyboard what is impossible To DO in buildgame
        Leave, //when a controller leave
        Dissconnect, //when midGame a controller DissConnect
    }

    void Awake()
    {
        instance = this; //connect the instance to this script and it will never dupe so no fail system needed
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject); //let it switch scene's
        canvas = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //enable or disable the ui
    public void SwitchVisible(bool state)
    {
        canvas.enabled = state; //so the script can stil run if u turn of canvas
    }

    //when smth happend where the ui need to be fix it is the function for it
    public void ChangeOutputUi(int plrId, ConnectionTypes connectionType)
    {
        print($"plr{plrId} connected: {connectionType}");
        LokaalMatchSlot slot = LokaalConnecter.allMatchingSlots[plrId];

        //connect it to the right function *ik why not public void all of them
        if (connectionType == ConnectionTypes.Join) Join(slot);
        else if (connectionType == ConnectionTypes.JoinDev) JoinDev(slot);
        else if (connectionType == ConnectionTypes.Leave) Leave(slot);
        else if (connectionType == ConnectionTypes.Dissconnect) DissConnect(slot);

    }

    void Join(LokaalMatchSlot slot)
    {
        slot.SwitchImage(controllerUi);
        slot.SwitchColor(true);
    }

    void JoinDev(LokaalMatchSlot slot)
    {
        slot.SwitchImage(devUi);
        slot.SwitchColor(true);
    }

    void Leave(LokaalMatchSlot slot)
    {
        slot.SwitchImage(nothingEnabledUi);
        slot.SwitchColor(false);
    }

    void DissConnect(LokaalMatchSlot slot)
    {
        if (LokaalConnecter.canConnect)
        {
            Leave(slot);
            return;
        }
    }
}
