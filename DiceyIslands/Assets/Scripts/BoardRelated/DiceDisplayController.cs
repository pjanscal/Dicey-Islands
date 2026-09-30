using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceDisplayController : MonoBehaviour
{
    // =========================================================
    // MAIN DIE
    // =========================================================

    [Header("Main Dice")]
    [SerializeField] private GameObject mainDiceViewport;
    [SerializeField] private Transform mainDice;
    [SerializeField] private Animator mainDiceAnimator;
    [SerializeField] private RawImage mainDiceDisplay;
    [SerializeField] private TMP_Text rollNumberText;


    // =========================================================
    // BONUS DIE
    // =========================================================

    [Header("Bonus Dice")]
    [SerializeField] private GameObject bonusDiceViewport;
    [SerializeField] private Transform bonusDice;
    [SerializeField] private Animator bonusDiceAnimator;
    [SerializeField] private RawImage bonusDiceDisplay;
    [SerializeField] private TMP_Text bonusRollNumberText;


    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Idle Animation")]
    [SerializeField] private float idleRotationSpeed = 120f;
    [SerializeField] private float directionChangeInterval = 0.35f;

    [Header("Roll Animation")]
    [SerializeField] private float finalFrameHoldTime = 0.75f;


    // =========================================================
    // INTERNAL STATE
    // =========================================================

    private bool idleSpinning;
    private bool rolling;

    private Vector3 mainSpinDirection;
    private Vector3 bonusSpinDirection;

    private float mainDirectionTimer;
    private float bonusDirectionTimer;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        HideImmediately();
    }

    private void Update()
    {
        if (!idleSpinning || rolling)
            return;

        SpinIdleDice();
    }


    // =========================================================
    // START OF TURN
    // =========================================================

    public void ShowForTurn(bool hasBonusDice)
    {
        StopAllCoroutines();

        rolling = false;
        idleSpinning = true;

        // Main die always exists.
        if (mainDiceViewport != null)
            mainDiceViewport.SetActive(true);

        if (mainDiceDisplay != null)
            mainDiceDisplay.gameObject.SetActive(true);

        // Bonus die only exists for players entitled to one.
        if (bonusDiceViewport != null)
            bonusDiceViewport.SetActive(hasBonusDice);

        if (bonusDiceDisplay != null)
            bonusDiceDisplay.gameObject.SetActive(hasBonusDice);

        // Don't reveal the result yet.
        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        // Animator should NOT control the die while we're
        // doing the idle spin ourselves.
        if (mainDiceAnimator != null)
        {
            mainDiceAnimator.enabled = false;
            mainDiceAnimator.speed = 1f;
        }

        if (bonusDiceAnimator != null)
        {
            bonusDiceAnimator.enabled = false;
            bonusDiceAnimator.speed = 1f;
        }

        // Start with random spin directions.
        mainSpinDirection = GetRandomDirection();
        bonusSpinDirection = GetRandomDirection();

        mainDirectionTimer = directionChangeInterval;
        bonusDirectionTimer = directionChangeInterval;
    }


    // =========================================================
    // IDLE SPIN
    // =========================================================

    private void SpinIdleDice()
    {
        if (mainDice != null &&
            mainDiceViewport != null &&
            mainDiceViewport.activeSelf)
        {
            mainDirectionTimer -= Time.deltaTime;

            if (mainDirectionTimer <= 0f)
            {
                mainSpinDirection = GetRandomDirection();
                mainDirectionTimer = directionChangeInterval;
            }

            mainDice.Rotate(
                mainSpinDirection *
                idleRotationSpeed *
                Time.deltaTime,
                Space.Self
            );
        }


        if (bonusDice != null &&
            bonusDiceViewport != null &&
            bonusDiceViewport.activeSelf)
        {
            bonusDirectionTimer -= Time.deltaTime;

            if (bonusDirectionTimer <= 0f)
            {
                bonusSpinDirection = GetRandomDirection();
                bonusDirectionTimer = directionChangeInterval;
            }

            bonusDice.Rotate(
                bonusSpinDirection *
                idleRotationSpeed *
                Time.deltaTime,
                Space.Self
            );
        }
    }


    private Vector3 GetRandomDirection()
    {
        return new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
    }


    // =========================================================
    // ROLL
    // =========================================================

    public IEnumerator PlayRoll(
        int mainResult,
        bool hasBonusDice,
        int bonusResult
    )
    {
        rolling = true;
        idleSpinning = false;


        // -----------------------------------------------------
        // Stop idle rotation
        // -----------------------------------------------------

        if (mainDice != null)
            mainDice.localRotation = Quaternion.identity;

        if (bonusDice != null)
            bonusDice.localRotation = Quaternion.identity;


        // -----------------------------------------------------
        // Enable Animators
        // -----------------------------------------------------

        if (mainDiceAnimator != null)
        {
            mainDiceAnimator.enabled = true;
            mainDiceAnimator.speed = 1f;

            mainDiceAnimator.SetInteger(
                "RollResult",
                mainResult
            );

            // Explicitly restart DiceThrow.
            mainDiceAnimator.Play(
                "DiceThrow",
                0,
                0f
            );
        }


        if (hasBonusDice &&
            bonusDiceAnimator != null)
        {
            bonusDiceAnimator.enabled = true;
            bonusDiceAnimator.speed = 1f;

            bonusDiceAnimator.SetInteger(
                "RollResult",
                bonusResult
            );

            bonusDiceAnimator.Play(
                "DiceThrow",
                0,
                0f
            );
        }


        // Wait a frame so Animator actually enters DiceThrow.
        yield return null;


        // -----------------------------------------------------
        // Wait for the result animations
        // -----------------------------------------------------

        yield return StartCoroutine(
            WaitForResultAnimation(
                mainDiceAnimator,
                mainResult
            )
        );


        if (hasBonusDice)
        {
            yield return StartCoroutine(
                WaitForResultAnimation(
                    bonusDiceAnimator,
                    bonusResult
                )
            );
        }


        // -----------------------------------------------------
        // Freeze final frame
        // -----------------------------------------------------

        if (mainDiceAnimator != null)
            mainDiceAnimator.speed = 0f;

        if (hasBonusDice &&
            bonusDiceAnimator != null)
        {
            bonusDiceAnimator.speed = 0f;
        }


        // -----------------------------------------------------
        // NOW reveal numbers
        // -----------------------------------------------------

        if (rollNumberText != null)
        {
            rollNumberText.text =
                mainResult.ToString();

            rollNumberText.gameObject.SetActive(true);
        }


        if (bonusRollNumberText != null)
        {
            if (hasBonusDice)
            {
                bonusRollNumberText.text =
                    bonusResult.ToString();

                bonusRollNumberText.gameObject.SetActive(true);
            }
            else
            {
                bonusRollNumberText.gameObject.SetActive(false);
            }
        }


        // Hold the finished dice on screen.
        yield return new WaitForSeconds(
            finalFrameHoldTime
        );

        rolling = false;
    }


    // =========================================================
    // WAIT FOR ANIMATION
    // =========================================================

    private IEnumerator WaitForResultAnimation(
        Animator animator,
        int result
    )
    {
        if (animator == null)
            yield break;

        string expectedState =
            "DiceRolling" + result;


        // First wait until DiceThrow transitions
        // into the correct result animation.
        while (true)
        {
            AnimatorStateInfo state =
                animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(expectedState))
                break;

            yield return null;
        }


        // Now wait until that result animation
        // reaches its final frame.
        while (true)
        {
            AnimatorStateInfo state =
                animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(expectedState) &&
                state.normalizedTime >= 1f)
            {
                break;
            }

            yield return null;
        }
    }


    // =========================================================
    // PLAYER STARTS MOVING
    // =========================================================

    public void HideForMovement()
    {
        idleSpinning = false;
        rolling = false;

        if (mainDiceAnimator != null)
            mainDiceAnimator.speed = 1f;

        if (bonusDiceAnimator != null)
            bonusDiceAnimator.speed = 1f;

        if (mainDiceDisplay != null)
            mainDiceDisplay.gameObject.SetActive(false);

        if (bonusDiceDisplay != null)
            bonusDiceDisplay.gameObject.SetActive(false);

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        if (mainDiceViewport != null)
            mainDiceViewport.SetActive(false);

        if (bonusDiceViewport != null)
            bonusDiceViewport.SetActive(false);
    }


    private void HideImmediately()
    {
        idleSpinning = false;
        rolling = false;

        if (mainDiceDisplay != null)
            mainDiceDisplay.gameObject.SetActive(false);

        if (bonusDiceDisplay != null)
            bonusDiceDisplay.gameObject.SetActive(false);

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        if (mainDiceViewport != null)
            mainDiceViewport.SetActive(false);

        if (bonusDiceViewport != null)
            bonusDiceViewport.SetActive(false);
    }
}