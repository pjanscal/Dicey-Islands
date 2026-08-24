using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LokaalMatchSlot : MonoBehaviour
{
    [SerializeField] private Image outputUi;
    [SerializeField] private TextMeshProUGUI nameDisplay;

    //config
    [SerializeField] private int plrId;
    [SerializeField] private Color slotColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LokaalConnecter.allMatchingSlots.Add(plrId, this);
        nameDisplay.text = $"player {plrId}";
    }

    // Update is called once per frame
    //leaving/ready up while in it
    void Update()
    {
        //note make the thing after one sec because if it not then u can leave immedally 
    }

    //set the color on/off
    public void SwitchColor(bool state)
    {
        outputUi.color = state? slotColor : Color.white;
    }

    //switch the image of that slot
    public void SwitchImage(Sprite sprite)
    {
        outputUi.sprite = sprite;
    }
}
