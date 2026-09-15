using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaingameScript5 : MonoBehaviour
{
    [SerializeField] int plrId;
    [SerializeField] RawImage mainColor;
    [SerializeField] Slider redSlider;
    [SerializeField] Slider greenSlider;
    [SerializeField] Slider blueSlider;
    [SerializeField] int sliderStep = 5;

    private readonly Slider[] sliders = new Slider[3];
    private int selectedSliderIndex = 0;
    private bool hasSelectedSlider = false;

    private static readonly HashSet<int> confirmedPlayers = new();
    private static bool colorPrinted = false;

    LokaalConnecter.PlayerController playerController;

    void Start()
    {
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
    }

    void Update()
    {
        if (playerController == null || !playerController.occuplied)
            return;

        if (!hasSelectedSlider)
        {
            SelectSlider(0);
            hasSelectedSlider = true;
        }

        HandleSliderControl();

        if (playerController.GetButtonDown(LokaalConnecter.InputType.x))
        {
            ConfirmColor();
        }
    }

    void HandleSliderControl()
    {
        if (playerController == null || !playerController.occuplied)
            return;

        Vector2 moveDir = playerController.GetMoveDir();

        if (moveDir.y > 0.3f)
        {
            selectedSliderIndex = Mathf.Max(0, selectedSliderIndex - 1);
            SelectSlider(selectedSliderIndex);
        }
        else if (moveDir.y < -0.3f)
        {
            selectedSliderIndex = Mathf.Min(2, selectedSliderIndex + 1);
            SelectSlider(selectedSliderIndex);
        }
        else if (moveDir.x < -0.3f)
        {
            AdjustSelectedSlider(-sliderStep);
        }
        else if (moveDir.x > 0.3f)
        {
            AdjustSelectedSlider(sliderStep);
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

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(chosenSlider.gameObject);

        chosenSlider.Select();
    }

    void AdjustSelectedSlider(float amount)
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
        if (mainColor == null || redSlider == null || greenSlider == null || blueSlider == null)
            return;

        float r = Mathf.Max(redSlider.value, 40f) / 255f;
        float g = Mathf.Max(greenSlider.value, 40f) / 255f;
        float b = Mathf.Max(blueSlider.value, 40f) / 255f;

        mainColor.color = new Color(r, g, b, 1f);
    }

    void ConfirmColor()
    {
        if (mainColor == null || colorPrinted || playerController == null || !playerController.occuplied)
            return;

        if (confirmedPlayers.Contains(plrId))
            return;

        confirmedPlayers.Add(plrId);

        int occupiedPlayerCount = 0;
        foreach (var controller in LokaalConnecter.plrsController.Values)
        {
            if (controller != null && controller.occuplied)
                occupiedPlayerCount++;
        }

        if (confirmedPlayers.Count >= occupiedPlayerCount)
        {
            colorPrinted = true;
            Debug.Log("Confirmed color: " + mainColor.color);
        }
    }
}
