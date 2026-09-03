using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LokaalMatchingUi : MonoBehaviour
{
    //everything about matchUi logic function is in here
    //reason why a new one because we need a public void with Ui and other ui logic i will see
    //fixed i might do make the slots here

    public static LokaalMatchingUi instance; //let other scripts use this
    
    //All Gui
    [Header("GUI")]
    //[SerializeField] private CanvasGroup dissconnectGui;
    [SerializeField] private CanvasGroup charSelectGui;
    [SerializeField] private CanvasGroup cpuSelectGui;

    //ui image that can be used
    [Header("Image's")]
    public Sprite nothingEnabledUi;
    public Sprite controllerUi;
    public Sprite devUi;
    public Sprite cpuUi;
    public Sprite dissconnectedControl; //it is for if it somehowDissconnected but i just have it while it useless :skull
    
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

        //beta
        for (int charId = 0; charId < GameMangeren.charsData.Length; charId++)
        {
            LokaalConnecter.charLeft.Add(charId);
        }
    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
            TryForceLoadingLokaalMatch();
        #endif

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            Debug.Log($"Gamepads: {Gamepad.all.Count}");

            foreach (Gamepad gamepad in Gamepad.all)
            {
                Debug.Log($"Gamepad: {gamepad.name} | ID: {gamepad.deviceId} | Enabled: {gamepad.enabled}");
            }
        }

        //here the rule when they can't join
        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing) return;
        
        //return if it reconnecting the dissconnected ones
        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.reConnecting && LokaalConnecter.currentPlr == GameMangeren.plrInGame) return;

        //connectcontrolls
        TryConnectingControllers();

        //keyboard Testing connector
        #if UNITY_EDITOR
            TryConnectingKeyboard();
        #endif
    }

    //enable or disable the ui
    public void SwitchVisible(bool state, bool isCharSelect)
    {
        if (state)
        {
            if (isCharSelect) EnableGui(charSelectGui, true);
            //else EnableGui(dissconnectGui, true);
        }
        else
        {
            EnableGui(charSelectGui, false);
            //EnableGui(dissconnectGui, false);
        }

        canvas.enabled = state; //so the script can stil run if u turn of canvas
    }

    public void SwitchToCpuGui(bool state)
    {
        if (state)
        {
            EnableGui(cpuSelectGui, true);
            EnableGui(charSelectGui, false);
        }
        else
        {
            EnableGui(charSelectGui, true);
            EnableGui(cpuSelectGui, false);
        }
    }

    void EnableGui(CanvasGroup Gui, bool state)
    {
        Gui.alpha = state? 1 : 0;
        Gui.blocksRaycasts = state;
        Gui.interactable = state;
    }

    //go exit it
    public void ExitLokaal()
    {
        //gameManger get it soon with the return to menu
        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.reConnecting)
        {
            Debug.LogError("return to menu and reset everything need to be make");
        }
        else
        {
            LokaalConnecter.SwitchMatchMaking(false);
            LokaalConnecter.outOfMatchMaking(false);

            LokaalConnecter.ResetLokaal();
        }
    }

    //when smth happend where the ui need to be fix it is the function for it
    public void ChangeOutputUi(int plrId, ConnectionTypes connectionType)
    {
        //Debug.LogWarning($"plr{plrId} connected: {connectionType}");
        LokaalMatchSlot disConnectSlot = LokaalConnecter.allMatchingSlots[plrId];
        LokaalCharSelectSlot charSelectSlot = LokaalConnecter.allCharacterSlots[plrId];

        //connect it to the right function *ik why not public void all of them
        if (connectionType == ConnectionTypes.Join) Join(charSelectSlot, disConnectSlot);
        else if (connectionType == ConnectionTypes.JoinDev) JoinDev(charSelectSlot, disConnectSlot);
        else if (connectionType == ConnectionTypes.Leave) Leave(charSelectSlot, disConnectSlot);
        else if (connectionType == ConnectionTypes.Dissconnect) DissConnect(disConnectSlot);

    }

    void Join(LokaalCharSelectSlot charSelectSlot, LokaalMatchSlot disConnectSlot)
    {
        //Ready up the Dissconnect screen so fix it when but i alr make it so it is for testing
        disConnectSlot.SwitchImage(controllerUi);
        disConnectSlot.SwitchColor(true);

        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.matchConnect) return;

        charSelectSlot.SwitchState(LokaalConnecter.characterSelectState.Choosing);
    }

    void JoinDev(LokaalCharSelectSlot charSelectSlot, LokaalMatchSlot disConnectSlot)
    {
        //Ready up the Dissconnect screen so fix it when but i alr make it so it is for testing
        disConnectSlot.SwitchImage(devUi);
        disConnectSlot.SwitchColor(true);

        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.matchConnect) return;

        charSelectSlot.SwitchState(LokaalConnecter.characterSelectState.Choosing);
    }

    void Leave(LokaalCharSelectSlot charSelectSlot, LokaalMatchSlot disConnectSlot)
    {
        //Ready up the Dissconnect screen so fix it when but i alr make it so it is for testing
        disConnectSlot.SwitchImage(nothingEnabledUi);
        disConnectSlot.SwitchColor(false);
        //disConnectSlot.SwitchReadyUpMark(false);

        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.matchConnect) return;

        charSelectSlot.SwitchState(LokaalConnecter.characterSelectState.Connecting);
    }

    void DissConnect(LokaalMatchSlot disConnectSlot)
    {
        disConnectSlot.SwitchImage(dissconnectedControl);
    }

    #if UNITY_EDITOR
    void TryConnectingKeyboard()
    {
        #if UNITY_EDITOR
            //get ur keyboard
            Keyboard keyboard = Keyboard.current;

            //check if it not being use again 
            if (!ControllIsFree()) return; //in foreach statement else when added it won't count that

            //check if it press join button
            if (!keyboard[Key.P].wasPressedThisFrame) return;

            //loop until having a avible keyboardId
            int keyboardId = 1;
            for (; keyboardId <= LokaalConnecter.maxKeyboardTester; keyboardId++)
            {
                if (!LokaalConnecter.alrUsedKeyboardId.Contains(keyboardId)) break;
            }

            if (keyboardId > LokaalConnecter.maxKeyboardTester) return;

            //connect it
            LokaalConnecter.ConnectKeyboard(keyboard, keyboardId);
        #endif
    }

    void TryForceLoadingLokaalMatch()
    {  
        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.nothing) return;

        if (Keyboard.current[Key.LeftBracket].wasPressedThisFrame)
        {
            LokaalConnecter.SwitchMatchMaking(true);
        }
    }

    #endif

    void TryConnectingControllers()
    {
        //get all coneccted controlls
        foreach(Gamepad gamepad in Gamepad.all)
        {
            //check or it is not alr connected
            if (ControllAlrBeingUsed(gamepad)) continue;

            //check if it not being use again 
            if (!ControllIsFree()) return; //in foreach statement else when added it won't count that

            //check if it press join button
            if (!gamepad.buttonSouth.wasPressedThisFrame) continue;

            //connect it
            LokaalConnecter.ConnectController(gamepad);
        }
    }

    //useless thx to controllAlrbeingUSed 
    //find or there is a slot free
    bool ControllIsFree()
    {
        //get all avible controller
        foreach (LokaalConnecter.PlayerController plrData in LokaalConnecter.plrsController.Values)
        {
            if (!plrData.occuplied) return true;
        }

        return false;
    }

    //check or a controller is not being use twice
    bool ControllAlrBeingUsed(Gamepad gamepad)
    {
        //look or a id is inside the list
        return LokaalConnecter.alrUsedControllers.Contains(gamepad.deviceId);
    }
}
