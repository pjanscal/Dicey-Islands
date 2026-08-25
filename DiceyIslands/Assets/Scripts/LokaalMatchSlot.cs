using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LokaalMatchSlot : MonoBehaviour
{
    [SerializeField] private Image outputUi;
    [SerializeField] private TextMeshProUGUI nameDisplay;

    private LokaalConnecter.PlayerController plrController;
    private float currentDissconnectTimer = 0f;

    //config
    [SerializeField] private int plrId;
    [SerializeField] private Color slotColor;

    private float timeBeforeDissconnect = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plrController = LokaalConnecter.plrsController[plrId];
        LokaalConnecter.allMatchingSlots.Add(plrId, this);

        nameDisplay.text = $"player {plrId}";
    }

    // Update is called once per frame
    //leaving/ready up while in it
    void Update()
    {

        if (!plrController.occuplied) return;

        //check or it is dissconnected
        CheckOrItIsDissconnected();

        //note make the thing after one sec because if it not then u can leave immedally 
    }

    void CheckOrItIsDissconnected()
    {
        if (plrController.gamepad == null) return; //check or it is not keyboard

        if (!plrController.gamepad.wasUpdatedThisFrame)
        {
            currentDissconnectTimer += Time.unscaledDeltaTime; //add it to timer
            print(currentDissconnectTimer);

            if (currentDissconnectTimer >= timeBeforeDissconnect)
            {
                LokaalConnecter.DissConnectController(plrController.gamepad);
                currentDissconnectTimer = 0;
            }
        }
        else
        {
            currentDissconnectTimer = 0; //reset timer
        }
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
