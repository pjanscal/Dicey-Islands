using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    //in progess thinking about wa to do

    [Header("CharConfigs")]
    public GameObject character; //the char that go in the game
    [Tooltip("image of the char when selecting a char")] //could also do with 3D into a viewporframe
    public Sprite preview;
    //mabyeSound or do it with humanoid
    public string charName; //ingame name
}
