using System;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "MinigameTutorialConfigs", menuName = "Scriptable Objects/MinigameTutorialConfigs")]
public class MinigameTutorialConfigs : ScriptableObject
{
    [Serializable]
    public class KeybindsShowCaseInfo
    {
        [Tooltip("right, left, up, down is all move u only need one it just say then L3")]
        public LokaalConnecter.InputType keybind; //*right, left, up, down is L3
        public string actionName = ":";
    }

    public int minigameId; //wich minigame
    public Sprite bg; //background of it
    public VideoClip videoClip;
    public KeybindsShowCaseInfo[] keyBindsInfo;
}
