using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MaingameScript5 : MonoBehaviour
{
    //DDD note bug were for multiplayer u use navigation ui that is good for playing alone 
    // and the fix for the button is vector2? movedir void i reccendamom u early so u get one axis
    // so u just have to fix the highlight/ delay between down and up DDD out

    //if u ask why not making a mangeren welp i am working with what i begin with so yk

    [SerializeField] int plrId;
    
    [Header("UI")]
    [SerializeField] RawImage mainColor;
    [SerializeField] private Image slowModeUi;
    [SerializeField] Slider redSlider;
    [SerializeField] Slider greenSlider;
    [SerializeField] Slider blueSlider;
    [SerializeField] TextMeshProUGUI scoreText;
    private RawImage targetColor;// => Colorhandler.instance.mainColors[0];

    [Header("Asset")]
    [Tooltip("Change in script folder")] [SerializeField] private Sprite slowModeOnSprite;
    [Tooltip("Change in script folder")] [SerializeField] private Sprite slowModeOffSprite;

    [HideInInspector] public readonly Slider[] sliders = new Slider[3];
    private int selectedSliderIndex = 0;

    LokaalConnecter.PlayerController playerController;

    private bool sliderSlowMode = false; //say if it going slow or fast
    private float sliderBetweenValue = 0; //help to not send .5 but instead wait until this is above
    private bool isActive = false;
    private bool canSwitchSlider = true;
    private bool playerColorConfirm = false;

    //configs
    const float defaultSliderSpeed = 130f; //to get there fast
    const float slowSliderSpeed = 10f; //if u wanna do the final tweeks
    const float timeBetweenSwitchingSlider = .2f;
    const float timeBeforeShowingResult = .5f;
    const float timeBeforeShowingWinner = 1.8f;
    const float scoreASec = 34f; //how fast it go toward the endscore

    //anti magic number
    const int totaleColorSlider = 3;

    public void Init()
    {
        isActive = true;
    }

    void Start()
    {
        GameMangeren.startMiniGame += Init;
        playerController = LokaalConnecter.plrsController[plrId];

        sliders[0] = redSlider;
        sliders[1] = greenSlider;
        sliders[2] = blueSlider;

        if (redSlider != null)
            redSlider.onValueChanged.AddListener(_ => ApplyColorFromSliders());

        if (greenSlider != null)
            greenSlider.onValueChanged.AddListener(_ => ApplyColorFromSliders());

        if (blueSlider != null)
            blueSlider.onValueChanged.AddListener(_ => ApplyColorFromSliders());

        ApplyColorFromSliders();
        
        SelectSlider(0);
        Colorhandler.instance.playersSliders.Add(plrId, sliders);
        Colorhandler.instance.playersScoreText.Add(plrId, scoreText);
        targetColor = Colorhandler.instance.mainColors[0];

        //debug
        if (mainColor == null || redSlider == null || greenSlider == null || blueSlider == null || scoreText == null || slowModeUi == null)
            Debug.LogError($"a var is not assign: plr{plrId}");
    }

    void Update()
    {
        if (!isActive || playerColorConfirm || playerController == null || !playerController.occuplied)
            return;

        HandleSliderControl();
        SlowMode();


        if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
        {
            ConfirmColor();
        }
    }

    void HandleSliderControl()
    {
        if (playerController == null || !playerController.occuplied)
            return;

        Vector2? moveDir = GetMoveDir(); //doing like this could make it so x is not use
        
        if (moveDir == null) return;

        if (moveDir == Vector2.up || moveDir == Vector2.down)
        {
            if (!canSwitchSlider) return; //check or it can be used and checking other if statement
            canSwitchSlider = false;

            //check or it is valid with new or else u can go up immedaly
            int newSelectSliderIndex = Mathf.Clamp(selectedSliderIndex + -(int)moveDir.Value.y, 0, 2);
            if (newSelectSliderIndex == selectedSliderIndex)
            {
                canSwitchSlider = true;
                return;
            }
            
            StartCoroutine(enumerator()); //after x amount of sec make it true again;
            ChangeSliderColor(sliders[selectedSliderIndex], Color.white); //clear the selection
            selectedSliderIndex = newSelectSliderIndex;
            SelectSlider(selectedSliderIndex);

            //put it on cooldown the switchbutton
            IEnumerator enumerator()
            {
                yield return new WaitForSeconds(timeBetweenSwitchingSlider);
                canSwitchSlider = true;
            }
        }
        else if (moveDir == Vector2.left)
        {
            int sliderAdjust = -GetSliderNextValue();
            AdjustSelectedSlider(sliderAdjust);
        }
        else if (moveDir == Vector2.right)
        {
            int sliderAdjust = GetSliderNextValue();
            AdjustSelectedSlider(sliderAdjust);
        }
    }

    void SelectSlider(int index)
    {
        selectedSliderIndex = index;

        if (sliders == null || index < 0 || index >= sliders.Length)
            return;

        Slider chosenSlider = sliders[index];
        if (chosenSlider == null)
            return;

        /* //problem this work *if u playing alone...
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(chosenSlider.gameObject);

        chosenSlider.Select();
        */

        ChangeSliderColor(chosenSlider, Color.red);
    }

    void AdjustSelectedSlider(int amount)
    {
        if (selectedSliderIndex < 0 || selectedSliderIndex >= sliders.Length)
            return;

        Slider activeSlider = sliders[selectedSliderIndex];
        if (activeSlider == null)
            return;

        activeSlider.value = Mathf.Clamp(activeSlider.value + amount, activeSlider.minValue, activeSlider.maxValue);
        ApplyColorFromSliders();
    }

    void ApplyColorFromSliders()
    {
        //float r = Mathf.Max(redSlider.value, 40f) / 255f; for if u don't wanna go below 40 but that is only for the other thing so ye else the score also need a update...
        float r = redSlider.value / 255f;
        float g = greenSlider.value / 255f;
        float b = blueSlider.value / 255f;

        mainColor.color = new Color(r, g, b, 1f);
    }
    
    void SlowMode()
    {
        if (!playerController.GetButtonDown(LokaalConnecter.InputType.jump)) return;

        sliderSlowMode = !sliderSlowMode;
        slowModeUi.sprite = sliderSlowMode? slowModeOnSprite : slowModeOffSprite;
    }

    void ConfirmColor()
    {
        if (playerColorConfirm || !playerController.occuplied)
            return;

        playerColorConfirm = true;
        Colorhandler.instance.confirmedPlayers.Add(plrId);
        ChangeSliderColor(sliders[selectedSliderIndex], Color.white);
        Colorhandler.instance.PlayPlayerReadySound();

        int occupiedPlayerCount = 0;
        foreach (var controller in LokaalConnecter.plrsController.Values)
        {
            if (controller.occuplied)
                occupiedPlayerCount++;
        }

        if (Colorhandler.instance.confirmedPlayers.Count >= occupiedPlayerCount)
        {
            // Everybody has confirmed.
            Colorhandler.instance.PlayAllPlayersReadySound();
            Debug.Log($"Confirmed color: {targetColor.color}");
            ShowResult();
        }
    }

    //show the result / find the winner and send it back
    void ShowResult()
    {
        List<(float, int)> places = new();

        //get all players score
        int redDistance = GetMainColorMaxDistance(0);
        int greenDistance = GetMainColorMaxDistance(1);
        int blueDistance = GetMainColorMaxDistance(2);
        foreach (int plrId in Colorhandler.instance.confirmedPlayers)
        {
            float totaleScore = 0f;

            totaleScore += GetScoreOnAColor(redDistance, 0, plrId);
            totaleScore += GetScoreOnAColor(greenDistance, 1, plrId);
            totaleScore += GetScoreOnAColor(blueDistance, 2, plrId);

            float score =  totaleScore / totaleColorSlider * 100; //make it %
            places.Add((score, plrId));
        }
        places.Sort((a, b) => b.Item1.CompareTo(a.Item1)); //sort largest at top

        //show the result
        //int placeIndex = 1;
        foreach (TextMeshProUGUI text in Colorhandler.instance.playersScoreText.Values)
        {
            text.enabled = true;
        }

        Sequence sequence = DOTween.Sequence(); //play all at same time :3
        sequence.AppendInterval(timeBeforeShowingResult);
        foreach ((float score, int plrId) in places)
        {
            float currentScore = 0;
            float duration = score / scoreASec;
            Tween tween = DOTween.To(() => currentScore, x =>
            {
                currentScore = x;
                Colorhandler.instance.playersScoreText[plrId].text = currentScore.ToString("F1");
            },
            score, duration);
            sequence.Join(tween);

            print($"plr{plrId}, Score: {score}");
            //placeIndex += 1;
        }
        sequence.AppendCallback(() => {}); //wait until the tween is done
        sequence.AppendInterval(timeBeforeShowingWinner);

        sequence.Play().OnComplete(() =>
        {
            //send back the winner and go to the boardscene
            if (MatchData.Instance == null) {Debug.LogError("no matchData found"); return;}

            foreach (var (_, plrId) in places)
            {
                MatchData.Instance.playerOrderNumbers.Add(plrId);
            }

            GameMangeren.MinigameWinner(MatchData.Instance.playerOrderNumbers);
        });
    }

    //helper function
    //get the next value based on speed/ mode/ time/ moveDir
    int GetSliderNextValue()
    {
        float sliderSpeed = !sliderSlowMode? defaultSliderSpeed : slowSliderSpeed;
        float moveDirSpeed = math.abs(playerController.GetMoveDir().x);
        float sliderAdjust = sliderSpeed * moveDirSpeed * Time.deltaTime;

        sliderBetweenValue += sliderAdjust;
        sliderAdjust = math.floor(sliderBetweenValue);
        sliderBetweenValue -= sliderAdjust;

        return (int)sliderAdjust;
    }

    int GetMainColorMaxDistance(int colorId)
    {
        int colorValue = Mathf.RoundToInt(targetColor.color[colorId] * 255); //make from .7 to a number like 0-255
        int distance = colorValue < (255 / 2)? 255 - colorValue : colorValue; //255 is like the color things
        return distance;
    }
    
    //ik the name
    float GetScoreOnAColor(int maxValue, int colorId, int plrId)
    {
        Slider[] playerSliders = Colorhandler.instance.playersSliders[plrId];
        float playerValue = playerSliders[colorId].value;
        float playerDisant = math.abs(Mathf.Round(targetColor.color[colorId] * 255f) - playerValue);
        
        if (playerDisant == 0) return 1; //perfect score
        return 1 - (playerDisant + 1) / (maxValue + 1); //(v + 1) stop the 0 / 0 and make the procent some chaos 
    }

    //change the color of the slider holder
    void ChangeSliderColor(Slider slider, Color selectedColor)
    {
        ColorBlock colorBlock = slider.colors;
        colorBlock.disabledColor = selectedColor;
        slider.colors = colorBlock;
    }

    Vector2? GetMoveDir()
    {
        Vector2 moveDir = playerController.GetMoveDir();

        //check or it is the x or the y
        if (math.abs(moveDir.y) > math.abs(moveDir.x))
        {
            if (math.abs(moveDir.y) < .3f) return null; //make sure it is not a little
            return new Vector2(0, math.sign(moveDir.y));
        }
        else
        {
            if (math.abs(moveDir.x) < .3f) return null; //make sure it is not a little
            return new Vector2(math.sign(moveDir.x), 0);
        }
    }
}
