using UnityEngine;

[CreateAssetMenu(fileName = "GameMangerSettings", menuName = "Scriptable Objects/GameMangerSettings")]
public class GameMangerSettings : ScriptableObject
{
    [Header("must")]
    public GameObject pauseSchrem;
    public GameObject eventSystemUi;
}
