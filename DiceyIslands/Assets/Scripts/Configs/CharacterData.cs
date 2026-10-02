using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : ScriptableObject
{
    //in progess thinking about wa to do

    [Header("CharConfigs")]
    public GameObject character; //the char that go in the game
    //public GameObject winnerCharacter; //where is light on level 1
    public Sprite characterIcon; //the icon of the label in game
    public Sprite inGamePreview;
    //mabyeSound or do it with humanoid
    public string charName; //ingame name
}
