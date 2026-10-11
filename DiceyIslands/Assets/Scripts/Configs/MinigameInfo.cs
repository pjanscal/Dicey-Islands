using UnityEngine;

[CreateAssetMenu(fileName = "MinigameInfo", menuName = "Scriptable Objects/MinigameInfo")]
public class MinigameInfo : ScriptableObject
{
    public int minigameId;
    public string minigameName = "Untiteld";
    public Sprite minigameShowcaseBackground;

    const string minigameSceneBeginName = "Minigame";

    public string GetSceneName()
    {
        return $"{minigameSceneBeginName}{minigameId}";
    }
}
