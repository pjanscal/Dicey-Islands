using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LokaalMatchSlot : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] private Image outputUi;
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private Image readyUpMark;
    [SerializeField] private GameObject pingEffect;

    private LokaalConnecter.PlayerController plrController;
    private float currentPingTimer = 0f;
    [HideInInspector] public bool isReadyUp = false; //so the server can see when all 4 is readyUP

    //config
    [Header("configs")]
    [SerializeField] private int plrId;
    [SerializeField] private Color slotColor;

    private float timeBetweenPing = .15f;
    private float durPing = .4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        plrController = LokaalConnecter.plrsController[plrId];
        LokaalConnecter.allMatchingSlots.Add(plrId, this);

        nameDisplay.text = $"player {plrId}";

        //setup ping ui
        pingEffect.transform.localScale = Vector3.zero;
        pingEffect.GetComponent<Image>().color = slotColor;
    }

    // Update is called once per frame
    //leaving/ready up while in it
    void Update()
    {

        if (!plrController.occuplied) return;

        //note make the thing after one sec because if it not then u can leave immedally 
        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.reConnecting) return; //it to make sure it only do if it is reconnecting

        //CheckOrItWantToLeave();
        ReadyUp();
        Ping();
    }

    //default some thing for the next time
    public void ClearSlot(bool hardReset)
    {
        isReadyUp = false;
        SwitchReadyUpMark(false);

        if (!hardReset) return; //for if it just wanna reset the readyup

        SwitchColor(false);
        SwitchImage(LokaalMatchingUi.instance.nothingEnabledUi);
    }

    void ReadyUp()
    {
        //on press toggle ready up so when everyone is ready up it will start
        if (plrController.GetButtonDown(LokaalConnecter.InputType.x))
        {
            SwitchReadyUpMark(!isReadyUp);

            if (!isReadyUp) return; //don't check if not needed

            CheckOfEveryoneIsReady();
        }
    }

    void CheckOfEveryoneIsReady()
    {
        foreach (LokaalConnecter.PlayerController plrData in LokaalConnecter.plrsController.Values)
        {
            if (!plrData.occuplied) continue;
            if (!LokaalConnecter.allMatchingSlots[LokaalConnecter.GetPlrIdFromPlrData(plrData)].isReadyUp) return;
        }

        Debug.LogWarning("EveryoneIsReadyUp");

        //if matchmaking say when going further
        //if it is dissconnected then go further without them *with warning*
        //if it dissconnected and everyone is here again just go further

        //what happend when there is not everyone?
        if (LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.reConnecting && LokaalConnecter.currentPlr != GameMangeren.plrInGame)
        {
            Debug.LogError("should give a waring for contine without everyone joining");
            LokaalConnecter.FinishMatchMaking();
        }
    }

    void Ping()
    {
        //on press ping urself so u can see who is who
        if (plrController.GetButtonDown(LokaalConnecter.InputType.jump) && currentPingTimer >= timeBetweenPing)
        {

            GameObject newPingUi = Instantiate(pingEffect, transform);

            //could also go into a new script that go on start :3 but that when it finalized it a prototype :3 i hope
            newPingUi.SetActive(true);
            Tween tween = newPingUi.transform.DOScale(1, durPing).SetUpdate(true);
            tween.OnComplete(() =>
            {
               Destroy(newPingUi); 
            });
            tween.Play();
            //sound

            currentPingTimer = 0;
        }

        currentPingTimer += Time.unscaledDeltaTime;
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
