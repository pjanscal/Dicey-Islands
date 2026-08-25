using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LokaalMatchSlot : MonoBehaviour
{
    [SerializeField] private Image outputUi;
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private Image readyUpMark;

    private LokaalConnecter.PlayerController plrController;
    private float currentDissconnectTimer = 0f;
    public bool isReadyUp = false; //so the server can see when all 4 is readyUP

    //config
    [SerializeField] private int plrId;
    [SerializeField] private Color slotColor;

    private float timeBeforeDissconnect = 1f; //time before corfirming it is dissconnected 

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
        //CheckOrItIsDissconnected();

        //note make the thing after one sec because if it not then u can leave immedally 
        if (!LokaalConnecter.canConnect) return;

        //CheckOrItWantToLeave();
        //ReadyUp();
        Ping();
    }

    void CheckOrItWantToLeave()
    {
        //on left press go dissconnect
        if (plrController.GetButtonDown(LokaalConnecter.InputType.secondAction))
        {
            LokaalConnecter.DissConnectController(plrId);
            LokaalMatchingUi.instance.ChangeOutputUi(plrId, LokaalMatchingUi.ConnectionTypes.Leave);
        }
    }

    void CheckOrItIsDissconnected()
    {
        if (plrController.gamepad == null) return; //check or it is not keyboard

        if (!plrController.gamepad.wasUpdatedThisFrame)
        {
            currentDissconnectTimer += Time.unscaledDeltaTime; //add it to timer

            if (currentDissconnectTimer >= timeBeforeDissconnect)
            {
                LokaalConnecter.ControllerDissConnected(plrController.gamepad);
                currentDissconnectTimer = 0;
            }
        }
        else
        {
            currentDissconnectTimer = 0; //reset timer
        }
    }

    void ReadyUp()
    {
        //on press toggle ready up so when everyone is ready up it will start
        if (plrController.GetButtonDown(LokaalConnecter.InputType.x))
        {
            SwitchReadyUpMark(!isReadyUp);

            //foreach (PlayerCont)
        }
    }

    void Ping()
    {
        //on press ping urself so u can see who is who
        if (plrController.GetButtonDown(LokaalConnecter.InputType.jump))
        {
            
        }
    }

    //set the color on/off
    public void SwitchColor(bool state)
    {
        outputUi.color = state? slotColor : Color.white;
    }

    //set the ready up mark on/off
    public void SwitchReadyUpMark(bool state)
    {
        readyUpMark.enabled = state;
        isReadyUp = state;
    }

    //switch the image of that slot
    public void SwitchImage(Sprite sprite)
    {
        outputUi.sprite = sprite;
    }
}
