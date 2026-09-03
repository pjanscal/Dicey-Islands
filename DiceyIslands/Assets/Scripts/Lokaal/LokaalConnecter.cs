using System;
using System.Collections;
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
    in PlrControllTesting is a example on how to use
    
    *to join press A, to ready up press X
    *if it don't connect because u testing before u come to me click "[" first

    to test with keyboard press P*
    to cconnect anywhere use [*
    *waring plrId begin by 1 and end with 4
    *u can use occupied to see if it already but it won't bug if u ask input it just return false or vector2.zero
    */

    //yo DDD it is me DDD check soon what happend if u getbuttondown after a frame and before that it is asked can it be used? 
    //btw cpu need to reset on reset do it torrmorrow

    //must be outside here so the script can use it and the other one don't need to look inside playercontoller
    public enum InputType //so u can ask for jump or movement and it return the thing
    {
        jump,
        x,
        secondAction, // o on ps5 idk the name
        y,
        Pause,
        left,
        right,
        up,
        down
    }

    //so ik if u can't connect, canconnect or need to reconnect
    public enum ConnectionTypes
    {
        nothing,
        matchConnect,
        CPUDifficultySelect,
        reConnecting
    }
    
    //say wich state the char select is
    public enum characterSelectState
    {
        Connecting,
        Choosing,
        Finish
    }

    public class PlayerController
    {
        public bool occuplied = false; //is this is true this is being use *ngl i wanted to check id but then i thought it is better
        
        //controls// one of those controll it so we can play with laptop/pc
        public Gamepad gamepad; //gamepad that control it :3
        public bool isCPU = false; //if there is no controll and it is cpu
        public Keyboard keyboard; //keyboard that controll it
        public int keyboardId; //id that help to make it 2 plrs testing

        //CPU input thingy
        public Vector2 cpuMoveDir = Vector2.zero; //for get movedir
        public HashSet<InputType> cpuButtonDown = new(); //for getButtonDown
        public HashSet<InputType> cpuButtonStateChanged = new();//for button up/down

        //input Registest
        //keybinds
        private Dictionary<InputType, GamepadButton> gamePadButtons = new()
        {
            {InputType.jump, GamepadButton.South},
            {InputType.x, GamepadButton.West},
            {InputType.secondAction, GamepadButton.East},
            {InputType.y, GamepadButton.North},
            {InputType.Pause, GamepadButton.Start}
        };
        private Dictionary<int, Dictionary<InputType, Key>> keyboardButtons = new()
        {
            {1, new() //KeyboardId 1
                {
                    {InputType.jump, Key.E},
                    {InputType.left, Key.A},
                    {InputType.up, Key.W},
                    {InputType.right, Key.D},
                    {InputType.down, Key.S},
                    {InputType.x, Key.Q},
                    {InputType.secondAction, Key.R},
                    {InputType.y, Key.F},
                    {InputType.Pause, Key.Digit1}
                }
            },
            {2, new() //keyboardId 2
                {
                    {InputType.jump, Key.O},
                    {InputType.left, Key.J},
                    {InputType.up, Key.I},
                    {InputType.right, Key.L},
                    {InputType.down, Key.K},
                    {InputType.x, Key.U},
                    {InputType.secondAction, Key.Digit0},
                    {InputType.y, Key.H},
                    {InputType.Pause, Key.Digit2}
                }
            }
        };

        //i have 3 function so u won't also need to do action like up, down, realsease so it ez to use :3 else it was 2 function shorter
        public bool GetButtonDown(InputType action)
        {
            //get the key from the dictionary
            if (gamepad != null)
            {
                return gamepad[gamePadButtons[action]].wasPressedThisFrame || false; //if failed
            }

            if (keyboard != null)
            {
                return keyboard[keyboardButtons[keyboardId][action]].wasPressedThisFrame || false; //if failed
            }

            if (isCPU)
            {
                return cpuButtonStateChanged.Contains(action) && cpuButtonDown.Contains(action);
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
                return keyboard[keyboardButtons[keyboardId][action]].isPressed;
            }

            if (isCPU)
            {
                return cpuButtonDown.Contains(action);
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
                return keyboard[keyboardButtons[keyboardId][action]].wasReleasedThisFrame;
            }

            if (isCPU)
            {
                return cpuButtonStateChanged.Contains(action) && !cpuButtonDown.Contains(action);
            }
            
            //a error happend
            Debug.LogError("no gamepad or keyboard found");
            return false;
        }

        //this is always move
        public Vector2 GetMoveDir()
        {
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
                moveDir += GetButton(InputType.up)? Vector2.up : Vector2.zero;
                moveDir += GetButton(InputType.right)? Vector2.right : Vector2.zero;
                moveDir += GetButton(InputType.left)? Vector2.left : Vector2.zero;
                moveDir += GetButton(InputType.down)? Vector2.down : Vector2.zero;

                //normalize it so it won't be faster with W/D
                return moveDir.normalized;
            }

            if (isCPU)
            {
                return cpuMoveDir.normalized;
            }
            
            //a error happend
            Debug.LogError("no gamepad or keyboard found");
            return Vector2.zero;
        }
        
        public void CPUSetButton(InputType action, bool state)
        {
            if (state) cpuButtonDown.Add(action);
            else cpuButtonDown.Remove(action);

            LokaalMatchingUi.instance.StartCoroutine(CPUButtonDownOrUp(action));
        }

        IEnumerator CPUButtonDownOrUp(InputType action)
        {
            cpuButtonStateChanged.Add(action);
            yield return null; //wait one frame or 2 and make it so that the thing deleted it
            cpuButtonStateChanged.Remove(action);
        }
    }
    
    static public List<int> alrUsedControllers = new(); //remeber all controll that alr being used
    static public List<int> alrUsedKeyboardId = new(); //remeber all keyboard id that beind used
    //static private List<int> plrsDissconnected = new();
    static public List<int> charLeft = new(); //wich char are left for visuale things so it find faster
    static public List<(int plrId, int charId)> characterDataToAdd = new(); //things in it make it automatic a char in plrData
    static public int maxKeyboardTester = 2; //don't change it *it not a config
    static public ConnectionTypes connectionType = ConnectionTypes.nothing; //make it so u can't join midgame
    static public int currentPlr = 0; //how many plr there are sign in
    static public Dictionary<int, PlayerController> plrsController = new(); //all slot of hte party
    static public Dictionary<int, LokaalMatchSlot> allMatchingSlots = new(); //all slot of LokaalmatchSlots
    static public Dictionary<int, LokaalCharSelectSlot> allCharacterSlots = new(); //all slot of character
    static public CPUDifficultySelect cPUDifficultySelect; //help initing it

    //actions
    static public Action<bool> outOfMatchMaking; //the bool for if it go back to main menu or if this is succes into the main game

    static private GameObject lokaalMatchingUi = Resources.Load<GameObject>("LokaalConnecter/LokaalConnectUi"); //get the ui of the connetionMatch
    static public int maxPlr = 4; //how many plr there can go in a game

    //configs
    static public string bordGameScene = "BoardTestScene";


    //when the game start it go once
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        //setup vars
        for (int i = 1; i <= maxPlr; i++)
        {
            plrsController.Add(i, new());
        }
        
        //setup gameobject in scenes
        GameObject.Instantiate(lokaalMatchingUi);

        //setup connections
        InputSystem.onDeviceChange += OnDeviceStateChanged;
        LokaalMatchingUi.instance.StartCoroutine(CharacterDataAddingWaitList());
    }

    //ResetLokaal
    static public void ResetLokaal()
    {
        for (int plrId = 1; plrId <= maxPlr; plrId++)
        {
            DissConnectController(plrId); //disconnect from the slotData
            GameMangeren.PlrData plrData = GameMangeren.GetPlrDataFromId(plrId);
            plrData.occupied = false;
            plrData.charData = null;
        }

        charLeft.Clear();
        for (int charId = 0; charId < GameMangeren.charsData.Length; charId++)
        {
            LokaalConnecter.charLeft.Add(charId);
        }

        //reset the slots
        foreach (LokaalMatchSlot slotData in allMatchingSlots.Values)
        {
            slotData.ClearSlot(true);
        }

        foreach (LokaalCharSelectSlot slotData in allCharacterSlots.Values)
        {
            slotData.ResetSlot();
        }
    }

    // Init when a device stateChanged use for leaving
    static void OnDeviceStateChanged(InputDevice device, InputDeviceChange state)
    {
        //check if it left
        if (state != InputDeviceChange.Disconnected && state != InputDeviceChange.Removed) return;

        //check or this is a valid device
        if (device is Gamepad gamepad)
        {
            Debug.Log("dissconnected");
            //ControllerDissConnected(gamepad);
        }
    }

    //init when a controller disconnect
    static public void ControllerDissConnected(Gamepad gamepad)
    {
        //get the vars
        PlayerController plrData = FindControllerSlot(gamepad); 
        if (plrData == null) return;
        int plrId = GetPlrIdFromPlrData(plrData);

        DissConnectController(plrId); //dissconnect it from the slots
        Debug.LogWarning($"plr{plrId} have dissconnected");

        //check or it was in game or not
        if (connectionType == ConnectionTypes.matchConnect) LokaalMatchingUi.instance.ChangeOutputUi(plrId, LokaalMatchingUi.ConnectionTypes.Leave);
        else if (GameMangeren.inGame)
        {
            LokaalMatchingUi.instance.ChangeOutputUi(plrId, LokaalMatchingUi.ConnectionTypes.Dissconnect);
            //plrsDissconnected.Add(plrId);
            if (connectionType == ConnectionTypes.nothing) SwitchMatchMaking(true);
        }
    }

    //koppel de controller aan de slots
    static public void ConnectController(Gamepad gamepad)
    {
        //get first free spot
        PlayerController plrData = GetFirstFreeSlot();
        int plrId = GetPlrIdFromPlrData(plrData);

        plrData.occuplied = true;
        alrUsedControllers.Add(gamepad.deviceId);
        plrData.gamepad = gamepad;

        LokaalMatchingUi.instance.ChangeOutputUi(plrId, LokaalMatchingUi.ConnectionTypes.Join);
        currentPlr += 1;

        Debug.LogWarning($"Plr{plrId} joined");
    }

    //dissconnect the controller/keyboard from the party
    static public void DissConnectController(int plrId)
    {
        PlayerController plrData = plrsController[plrId];
        //if it not check here then remeber

        plrData.occuplied = false;

        if (plrData.gamepad != null) //check wich device it is to remove
        {
            alrUsedControllers.Remove(plrData.gamepad.deviceId);
            plrData.gamepad = null;
        }
        else if (plrData.isCPU)
        {
            plrData.isCPU = false;
            plrData.cpuButtonDown.Clear();
            plrData.cpuButtonStateChanged.Clear();
            plrData.cpuMoveDir = Vector2.zero;
        }
        else
        {
            #if UNITY_EDITOR
                plrData.keyboard = null;
                alrUsedKeyboardId.Remove(plrData.keyboardId);
                plrData.keyboardId = 0;
            #endif
        }

        currentPlr -= 1;

        //here for the logic wa happend if they leave

        Debug.LogWarning($"plr{plrId} left the game");
    }

    //testing control so we don't need a control :3
    #if UNITY_EDITOR
    static public void ConnectKeyboard(Keyboard keyboard, int keyboardId)
    {
        //get first free spot
        PlayerController plrData = GetFirstFreeSlot();
        int plrId = GetPlrIdFromPlrData(plrData);

        plrData.occuplied = true;
        plrData.keyboard = keyboard;
        plrData.keyboardId = keyboardId;
        alrUsedKeyboardId.Add(keyboardId);

        LokaalMatchingUi.instance.ChangeOutputUi(plrId, LokaalMatchingUi.ConnectionTypes.JoinDev);
        currentPlr += 1;

        Debug.LogWarning($"Plr{plrId} joined *with keyboard*");
    }
    #endif

    //enable thing that it work
    static public void SwitchMatchMaking(bool state)
    {
        if (state)
        {
            Time.timeScale = 0; //if a minigame that play with timescale get old one like make a var to remeber;

            if (!GameMangeren.inGame) connectionType = ConnectionTypes.matchConnect; //make sure it not doing it when it should not
            else connectionType = ConnectionTypes.reConnecting;

            LokaalMatchingUi.instance.SwitchVisible(true, connectionType == ConnectionTypes.matchConnect);
        }
        else
        {
            Time.timeScale = 1; //if a minigame that play with timescale get old one like make a var to remeber;
            LokaalMatchingUi.instance.SwitchVisible(false, connectionType == ConnectionTypes.matchConnect);
            connectionType = ConnectionTypes.nothing;
        }
    }

    //wait for a item added to the list
    //fixes that u can't press at the exact same time but there is no add event in list...
    static IEnumerator CharacterDataAddingWaitList()
    {
        while (true)
        {
            //when contine of going again it is gone
            if (characterDataToAdd.Count >= 1) characterDataToAdd.RemoveAt(0);

            yield return new WaitUntil(() => characterDataToAdd.Count >= 1);

            var (plrId, charId) = characterDataToAdd[0];
            CharacterData charData = GameMangeren.GetCharacterDataFromId(charId);
            GameMangeren.PlrData plrData = GameMangeren.GetPlrDataFromId(plrId);

            //soon updating the speed with the new one
            if (plrData.occupied || CharacterAlrBeingUse(charData)) continue;

            plrData.occupied = true;
            plrData.charData = charData;
            allCharacterSlots[plrId].SwitchState(characterSelectState.Finish);
            charLeft.Remove(charId);

            Debug.LogWarning($"plr{plrId} chosed: {charData.charName}");
        }
    }

    static bool CharacterAlrBeingUse(CharacterData charData)
    {
        foreach (GameMangeren.PlrData plrData in GameMangeren.plrsData.Values)
        {
            if (plrData.charData == charData) return true;
        }

        return false;
    }

    //check first or everyone is ready to finish the charselect and go to cpu difficulty select
    static public void FinishMatchMaking()
    {
        if (currentPlr == 0) return;

        //check if it is valid
        foreach (PlayerController plrData in plrsController.Values)
        {
            if (!plrData.occuplied) continue;
            if (!allCharacterSlots[GetPlrIdFromPlrData(plrData)].isReadyUp) return;
        }

        connectionType = ConnectionTypes.nothing; //so it won't lett anyone connect when they shouldn't

        Debug.LogWarning("EveryoneIsReadyUp");

        //set in all cpu
        foreach (PlayerController plrData in plrsController.Values)
        {
            if (!plrData.occuplied) SetCPU(plrData);
        }

        IEnumerator enumerator()
        {
            yield return new WaitForSecondsRealtime(allCharacterSlots[1].charSwitchDur + .2f);

            connectionType = ConnectionTypes.CPUDifficultySelect;
            LokaalMatchingUi.instance.SwitchToCpuGui(true);
            cPUDifficultySelect.Init(); //help active it
        }

        LokaalMatchingUi.instance.StartCoroutine(enumerator());
    }

    //return to the charselect
    static public void CPUDifficultySelectQuit()
    {
        ResetLokaal();

        //here smth to delay if it go instant *wich is

        LokaalMatchingUi.instance.SwitchToCpuGui(false);
        connectionType = ConnectionTypes.matchConnect;
    }

    //start the game
    static public void FinishCpuDifficultySelect()
    {
        connectionType = ConnectionTypes.nothing;

        SwitchMatchMaking(false);
        outOfMatchMaking?.Invoke(true);
        GameMangeren.inGame = true; //it would be a prob to make true = true :3* if this is found
        GameMangeren.plrInGame = currentPlr;

        foreach (LokaalMatchSlot slotData in allMatchingSlots.Values)
        {
            slotData.ClearSlot(false);
        }
    }

    static void SetCPU(PlayerController plrData)
    {
        plrData.isCPU = true;
        plrData.occuplied = true;

        int plrId = GetPlrIdFromPlrData(plrData);
        GameMangeren.allCPU.Add(plrId);

        //set in the rng char
        int charId = charLeft[0];
        charLeft.RemoveAt(0); //remove the old one immedaly
        CharacterData charData = GameMangeren.GetCharacterDataFromId(charId);

        //show it in the charselect
        LokaalCharSelectSlot charSelectSlot = allCharacterSlots[plrId];
        charSelectSlot.currentCharSelected = charId;
        charSelectSlot.SwitchState(characterSelectState.Choosing);

        GameMangeren.GetPlrDataFromId(plrId).charData = charData;

        //setup the pause connection
        LokaalMatchSlot matchSlot = allMatchingSlots[plrId];
        matchSlot.SwitchImage(LokaalMatchingUi.instance.cpuUi);
        matchSlot.SwitchColor(true);
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
    static PlayerController FindControllerSlot(InputDevice device)
    {
        //loop through everyone form 1-4 until finding it
        foreach (PlayerController plrData in plrsController.Values)
        {
            if (plrData.gamepad == device) return plrData;

            //check keyboard
            #if UNITY_EDITOR
                if (plrData.keyboard == device) return plrData;
            #endif
        }

        //useless left
        return null;
    }

    static public int GetPlrIdFromPlrData(PlayerController plrData)
    {
        int plrId = plrsController.First(x => x.Value == plrData).Key; //look for the thing with same value
        return plrId;
    }
}
