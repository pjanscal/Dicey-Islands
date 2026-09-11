using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "MinigameTutorialConfigs", menuName = "Scriptable Objects/MinigameTutorialConfigs")]
public class MinigameTutorialConfigs : ScriptableObject
{
    public Sprite bg; //background of it
    public VideoClip videoClip;
    public LokaalConnecter.InputType[] keyBinds;
}
