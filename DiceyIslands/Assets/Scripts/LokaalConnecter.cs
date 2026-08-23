using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public static class LokaalConnecter
{
    /*Tutorail on how to use
    to get the plrData of it u can ask for plrsControllers[plrId] to get the id
    input is plrdata like plrdata.GetButtonDown(type) the type is in InputType
    *u can use occupied to see if it already but it won't bug if u ask input it just return false or vector2.zero
    */


    //must be outside here so the script can use it and the other one don't need to look inside playercontoller
    public enum InputType //so u can ask for jump or movement and it return the thing
    {
        jump,
    }

    public class PlayerController
    {
        public bool occuplied = false; //is this is true this is being use *ngl i wanted to check id but then i thought it is better
        
        //controls// one of those controll it so we can play with laptop/pc
        public Gamepad gamepad; //gamepad that control it :3
        public Keyboard keyboard; //keyboard that controll it
        //public int keyboardId; //id that help to make it 2 plrs testing

        //input Registest
        //keybinds
        private Dictionary<InputType, GamepadButton> gamePadButtons = new()
        {
            {InputType.jump, GamepadButton.South},
        };
        private Dictionary<InputType, Key> keyboardButtons = new()
        {
            {InputType.jump, Key.Space},
        };

        //i have 3 function so u won't also need to do action like up, down, realsease so it ez to use :3 else it was 2 function shorter
        public bool GetButtonDown(InputType action)
        {
            //get the key from the dictionary
            if (gamepad != null)
            {
                return gamepad[gamePadButtons[action]].wasPressedThisFrame;
            }

            if (keyboard != null)
            {
                return keyboard[keyboardButtons[action]].wasPressedThisFrame;
            }
            
            //a error happend
            Debug.LogError("no gamepad or keyboard found");
            return false;
        }

        public bool GetButton(InputType action)
        {
            //get the key from the dictionary
            if (gamepad != null)
            {
                return gamepad[gamePadButtons[action]].isPressed;
            }

            if (keyboard != null)
            {
                return keyboard[keyboardButtons[action]].isPressed;
            }
            
            //a error happend
            Debug.LogError("no gamepad or keyboard found");
            return false;
        }

        public bool GetButtonUp(InputType action)
        {
            //get the key from the dictionary
            if (gamepad != null)
            {
                return gamepad[gamePadButtons[action]].wasReleasedThisFrame;
            }

            if (keyboard != null)
            {
                return keyboard[keyboardButtons[action]].wasReleasedThisFrame;
            }
            
            //a error happend
            Debug.LogError("no gamepad or keyboard found");
            return false;
        }

        //this is always move
        public Vector2 GetMoveDir()
        {
            bool GetKeyDown(Key key)
            {
                return keyboard[key].isPressed;
            }

            //get the key from the dictionary
            if (gamepad != null)
            {
                return gamepad.leftStick.ReadValue();
            }

            if (keyboard != null)
            {
                //begin value to do +
                Vector2 moveDir = Vector2.zero;

                //check all possible keys
                moveDir += GetKeyDown(Key.W)? Vector2.up : Vector2.zero;
                moveDir += GetKeyDown(Key.D)? Vector2.right : Vector2.zero;
                moveDir += GetKeyDown(Key.A)? Vector2.left : Vector2.zero;
                moveDir += GetKeyDown(Key.S)? Vector2.down : Vector2.zero;

                //normalize it so it won't be faster with W/D
                return moveDir.normalized;
            }
            
            //a error happend
            Debug.LogError("no gamepad or keyboard found");
            return Vector2.zero;
        }
    }
    
    static public List<int> alrUsedControllers = new();
    static public Dictionary<int, PlayerController> plrsController = new();

    static private GameObject lokaalConnecterSupporter = Resources.Load<GameObject>("LokaalConnecter/LokaalConnecterSupporter"); //get the gameobject with the support in so it can use update
    

    //when the game start it go once
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        //setup vars
        for (int i = 1; i <= 4; i++)
        {
            plrsController.Add(i, new());
        }
        
        //setup gameobject in scenes
        GameObject.Instantiate(lokaalConnecterSupporter);

        //setup connections
        InputSystem.onDeviceChange += OnDeviceStateChanged;
    }

    // Init when a device stateChanged use for leaving
    static void OnDeviceStateChanged(InputDevice device, InputDeviceChange state)
    {
        //check if it left
        if (state != InputDeviceChange.Disconnected && state != InputDeviceChange.Removed) return;

        //check or this is a valid device
        if (device is Gamepad gamepad)
        {
            DissConnectController(gamepad);
        }
    }

    static public void ConnectController(Gamepad gamepad)
    {
        //get first free spot
        PlayerController plrData = GetFirstFreeSlot();

        plrData.occuplied = true;
        alrUsedControllers.Add(gamepad.deviceId);
        plrData.gamepad = gamepad;

        Debug.LogWarning($"Plr{GetPlrIdFromPlrData(plrData)} joined");
    }

    //dissconnect the controller from the party
    static void DissConnectController(Gamepad gamepad)
    {
        PlayerController plrData = FindControllerSlot(gamepad);
        if (plrData == null) return;

        plrData.occuplied = false;
        alrUsedControllers.Remove(gamepad.deviceId);
        plrData.gamepad = null;

        //here for the logic wa happend if they leave

        Debug.LogWarning($"plr{GetPlrIdFromPlrData(plrData)} left the game");
    }

    //testing control so we don't need a control :3
    static public void ConnectKeyboard(Keyboard keyboard)
    {
        //get first free spot
        PlayerController plrData = GetFirstFreeSlot();

        plrData.occuplied = true;
        plrData.keyboard = keyboard;

        Debug.LogWarning($"Plr{GetPlrIdFromPlrData(plrData)} joined *with keyboard*");
    }

    //find the first free plr slot to concent to
    static PlayerController GetFirstFreeSlot()
    {
        //loop through everyone form 1-4 until finding it
        foreach (PlayerController plrData in plrsController.Values)
        {
            if (!plrData.occuplied) return plrData;
        }

        //this immposble for it but if this happend it warn because it happend after the check for free space
        Debug.LogError("somehow a plr joined at same time when a check happend"); //ik mabye use this for both ik
        return null;
    }

    //find the one that have same device
    static PlayerController FindControllerSlot(Gamepad gamepad)
    {
        //loop through everyone form 1-4 until finding it
        foreach (PlayerController plrData in plrsController.Values)
        {
            if (plrData.gamepad == gamepad) return plrData;
        }

        //useless left
        return null;
    }

    static int GetPlrIdFromPlrData(PlayerController plrData)
    {
        int plrId = plrsController.First(x => x.Value == plrData).Key; //look for the thing with same value
        return plrId;
    }
}
