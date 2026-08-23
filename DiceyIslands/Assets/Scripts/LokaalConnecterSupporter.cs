using UnityEngine;
using UnityEngine.InputSystem;

public class LokaalConnecterSupporter : MonoBehaviour
{
    //this script help the main script use update... ye ik

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject); //so it can swithc between scene
    }

    // Update is called once per frame
    void Update()
    {
        //here the rule when he can't join

        //connectcontrolls
        TryConnectingControllers();

        //keyboard Testing connector
        #if UNITY_EDITOR
            TryConnectingKeyboard();
        #endif
    }

    void TryConnectingKeyboard()
    {
        #if UNITY_EDITOR
            //get ur keyboard
            Keyboard keyboard = Keyboard.current;

            //check if it not being use again 
            if (!ControllIsFree()) return; //in foreach statement else when added it won't count that

            //check if it press join button
            if (!keyboard[Key.P].wasPressedThisFrame) return;

            //connect it
            LokaalConnecter.ConnectKeyboard(keyboard);
        #endif
    }

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
