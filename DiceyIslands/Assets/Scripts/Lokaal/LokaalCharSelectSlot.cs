using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LokaalCharSelectSlot : MonoBehaviour
{
    [Header("ui")]
    [SerializeField] private Image previewUi; //after it prototype is finish do some QOL thingy
    [SerializeField] private Transform previewFrame;
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private Image[] arrows;
    [SerializeField] private Image downTutorial;
    [SerializeField] private Sprite pressAToJoinImage;
    private Image secondaryPreviewUi;
    private Image bgImage;

    private LokaalConnecter.PlayerController plrController;
    private LokaalConnecter.characterSelectState currentState = LokaalConnecter.characterSelectState.Connecting;
    private bool canSwitchChar = true; //debounce when doing the an
    [HideInInspector] public bool isReadyUp = false;
    private int currentCharSelected = 0;
    private int? oldCharId;
    private bool previewPrimeSelected = true;
    //private float currentTimerBetweenChar;

    //config
    [Header("configs")]
    [SerializeField] private int plrId;
    [SerializeField] private Color slotColor;

    private Color bgDisableColor = new Color(215 / 255f, 215 / 255f, 215 / 255f, 220f / 255f);
    const float colorSwitchDur = .5f;
    const float charSwitchDur = .9f;
    private Color charDisableColor = new Color(.15f, .15f, .15f);
    private Color CharReadyUpBetaColor = new Color(.4f, .5f, 0);
    //private float timeBoforeSwitchingChar = .5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LokaalConnecter.allCharacterSlots.Add(plrId, this);
        plrController = LokaalConnecter.plrsController[plrId];
        bgImage = GetComponent<Image>();

        bgImage.color = bgDisableColor;
        nameDisplay.text = $"player {plrId}";
        previewUi.sprite = pressAToJoinImage;

        GameObject newPreview = Instantiate(previewUi.gameObject, previewFrame);
        RectTransform rectTransform = newPreview.GetComponent<RectTransform>();
        rectTransform.localPosition = Vector2.up * previewUi.rectTransform.rect.height;
        secondaryPreviewUi = newPreview.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!plrController.occuplied) return;

        //note make the thing after one sec because if it not then u can leave immedally 
        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.matchConnect 
         || currentState == LokaalConnecter.characterSelectState.Connecting) return; //it to make sure it only do if it is reconnecting

        SwitchCharPreview();
        Select();

        //could also init only when it happend
        CheckCharColor();
        
        CheckOrItWantToLeave();
    }

    //Init when trying to switch
    void SwitchCharPreview()
    {
        if (currentState != LokaalConnecter.characterSelectState.Choosing) return;

        Vector2 moveDir = plrController.GetMoveDir();
        
        //check of it is right or left not above :3
        var (succes, dir) = GetLeftRight(moveDir);
        if (!succes || !canSwitchChar) return;
        canSwitchChar = false; //here for the debounce

        SetCharacter(currentCharSelected + dir);
        SwitchPreviewUi(Vector2.right * dir);
    }

    (bool succes, int dir) GetLeftRight(Vector2 moveDir)
    {
        //check of it not up or down
        if (math.abs(moveDir.y) > math.abs(moveDir.x)) return (false, 0);

        int dir = (int)math.sign(moveDir.x);
        if (dir == 0) return (false, dir);

        return (true, dir);
    }

    //init when intended to select smth or to readyup
    void Select()
    {
        if (plrController.GetButtonDown(LokaalConnecter.InputType.x))

        //on press send message u want it if it is charselect state
        if (currentState == LokaalConnecter.characterSelectState.Choosing)
        {
            //try to find first if u name are not in the list
            if (LokaalConnecter.characterDataToAdd.Any(x => x.plrId == plrId)) return;
            LokaalConnecter.characterDataToAdd.Add((plrId, currentCharSelected));
        }

        //on press readyUp if it is finish state
        else if (currentState == LokaalConnecter.characterSelectState.Finish)
        {
            ToggleReadyUp(!isReadyUp);

            if (!isReadyUp) return; //don't check if not needed

            LokaalConnecter.FinishMatchMaking();
        }
    }

    void CheckOrItWantToLeave()
    {
        //on left press go dissconnect
        if (plrController.GetButtonDown(LokaalConnecter.InputType.secondAction))
        {
            //can't leave while char are switching
            if (!canSwitchChar || DOTween.IsTweening(previewUi) || DOTween.IsTweening(secondaryPreviewUi)) return;

            if (currentState == LokaalConnecter.characterSelectState.Finish)
            {
                SwitchState(LokaalConnecter.characterSelectState.Choosing);
                return;
            }

            LokaalConnecter.DissConnectController(plrId);
            LokaalMatchingUi.instance.ChangeOutputUi(plrId, LokaalMatchingUi.ConnectionTypes.Leave);
            LokaalConnecter.FinishMatchMaking();
        }
    }
    
    public void ResetSlot()
    {
        currentState = LokaalConnecter.characterSelectState.Connecting;
        isReadyUp = false;
        SetupConnecting();
    }

    //set the char
    void SetCharacter(int charId)
    {
        Image target = !previewPrimeSelected? previewUi : secondaryPreviewUi;

        //set the id in valid reach
        if (charId > GameMangeren.charsData.Length - 1) charId = 0; //1 is the beginning of a array
        if (charId < 0) charId = GameMangeren.charsData.Length - 1;

        CharacterData charData = GameMangeren.GetCharacterDataFromId(charId);
        //set the color alright so i don't tween when it happend
        if (CharAlrBeenUsed(charId) &&
         currentState != LokaalConnecter.characterSelectState.Finish) target.color = charDisableColor;
        else target.color = Color.white;
        target.sprite = charData.preview;
        oldCharId = currentCharSelected;
        currentCharSelected = charId;

        //tween
    }

    //set the color alright when it happend
    void CheckCharColor()
    {
        if (currentState != LokaalConnecter.characterSelectState.Choosing) return;
        
        CheckPreviewDisable(previewUi, previewPrimeSelected);
        CheckPreviewDisable(secondaryPreviewUi, !previewPrimeSelected);
    }

    void CheckPreviewDisable(Image target, bool isPrime)
    {
        if (DOTween.IsTweening(target)) return;

        int? charId = isPrime? currentCharSelected : oldCharId;
        if (charId == null) return;

        if (CharAlrBeenUsed(charId.Value))
        {
            if (target.color == charDisableColor) return;

            ToggleColor(target, charDisableColor);
        }
        else
        {
            if (target.color == Color.white) return;

            ToggleColor(target, Color.white);
        }
    }

    //check of it still exist
    bool CharAlrBeenUsed(int charId)
    {
        return !LokaalConnecter.charLeft.Contains(charId);
    }

    //init when a whole state is going to change
    public void SwitchState(LokaalConnecter.characterSelectState state) //3 state begin, selecting, ready
    {
        //setting up so it have to wait until someone join
        if (state == LokaalConnecter.characterSelectState.Connecting) SetupConnecting();
        //setting up the char select
        else if (state == LokaalConnecter.characterSelectState.Choosing) SetupChoosing();
        //setting up finaleLize step (ready up thingy)
        else SetupFinalize();

        currentState = state;
    }

    //--all of state function--//
    void SetupConnecting()
    {
        ToggleColor(bgImage, bgDisableColor);
        Image target = !previewPrimeSelected? previewUi : secondaryPreviewUi;
        target.sprite = pressAToJoinImage;
        target.color = Color.white;
        SwitchPreviewUi(Vector2.up);
        oldCharId = null;

        foreach (Image arrow in arrows)
        {
            arrow.enabled = false;
        }
        downTutorial.enabled = false;

        //clean up from finalized
        if (currentState == LokaalConnecter.characterSelectState.Finish) DisableFinalize();
    }

    void SetupChoosing()
    {
        if (currentState == LokaalConnecter.characterSelectState.Connecting)
        {
            ToggleColor(bgImage, slotColor);
            SetCharacter(1);
            SwitchPreviewUi(Vector2.down);
        }

        foreach (Image arrow in arrows)
        {
            arrow.enabled = true;
        }
        downTutorial.enabled = true;

        //clean up from finalized
        if (currentState == LokaalConnecter.characterSelectState.Finish) DisableFinalize();
    }

    void DisableFinalize()
    {
        GameMangeren.PlrData plrData = GameMangeren.GetPlrDataFromId(plrId);
        ToggleReadyUp(false);
        LokaalConnecter.charLeft.Add(GameMangeren.GetIdFromCharData(plrData.charData));
        plrData.charData = null;
        plrData.occupied = false;
    }

    void SetupFinalize()
    {
        foreach (Image arrow in arrows)
        {
            arrow.enabled = false;
        }
    }
    //--ended--//

    void ToggleColor(Image target, Color targetColor) //true = show red color
    {
        //play the tween between color
        target.DOColor(targetColor, colorSwitchDur)
        .SetEase(Ease.OutSine).SetUpdate(true);
    }

    void SwitchPreviewUi(Vector2 dir)
    {
        RectTransform selected = previewPrimeSelected? previewUi.rectTransform : secondaryPreviewUi.rectTransform;
        RectTransform newPreview = !previewPrimeSelected? previewUi.rectTransform : secondaryPreviewUi.rectTransform;
        Vector2 targetPos = new Vector2(previewUi.rectTransform.rect.width, previewUi.rectTransform.rect.height) * dir;
        newPreview.localPosition = targetPos * -1; //get to the - side to start

        DOTween.Sequence() //so it can start all at the exact same time
        .Append(selected.DOLocalMove(targetPos, charSwitchDur))
        .Join(newPreview.DOLocalMove(Vector2.zero, charSwitchDur))
        .SetEase(Ease.OutBounce).SetUpdate(true) //settings
        .OnComplete(() =>
        {
            canSwitchChar = true;
        });

        previewPrimeSelected = !previewPrimeSelected;
    }

    //later
    void ToggleReadyUp(bool state)
    {
        isReadyUp = state;

        ToggleColor(previewPrimeSelected? previewUi : secondaryPreviewUi, state? CharReadyUpBetaColor : Color.white);
    }
}
