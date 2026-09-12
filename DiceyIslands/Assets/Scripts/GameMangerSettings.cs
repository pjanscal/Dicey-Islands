using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameMangerSettings", menuName = "Scriptable Objects/GameMangerSettings")]
public class GameMangerSettings : ScriptableObject
{
    [Serializable]
    public class KeybindSprites
    {
        public LokaalConnecter.InputType inputType;
        public Sprite sprite;
    }

    [Header("must")]
    public GameObject pauseSchrem;
    public GameObject eventSystemUi;
    public GameObject loadingScreen;
    public GameObject miniGameTutorial;

    [Header("configs")]
    public KeybindSprites[] keybindSprites;
}
