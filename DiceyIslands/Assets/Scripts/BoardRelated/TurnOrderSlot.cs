using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderSlot : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private Image slotColor;
    [SerializeField] private Image headSlot;

    [Header("info")]
    [SerializeField] private Sprite[] slotInfos; //less then 1 hour no dictionary

    public void GenarateSlot(int plrId)
    {
        if (!GameMangeren.inGame) {Debug.LogError("u started in boardgame no ingame bool"); return;}
        GameMangeren.PlrData plrData = GameMangeren.GetPlrDataFromId(plrId);
        
        slotColor.sprite = slotInfos[plrId - 1];
        headSlot.sprite = plrData.charData.characterIcon;
        playerName.text = $"Player {plrId}";
    }
}
