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
    public System.Action OnResultShown;

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

        // -------------------------
        // Show main die
        // -------------------------

        if (mainDiceViewport != null)
            mainDiceViewport.SetActive(true);

        if (mainDiceDisplay != null)
            mainDiceDisplay.gameObject.SetActive(true);


        // -------------------------
        // Show bonus die if needed
        // -------------------------

        if (bonusDiceViewport != null)
            bonusDiceViewport.SetActive(hasBonusDice);

        if (bonusDiceDisplay != null)
            bonusDiceDisplay.gameObject.SetActive(hasBonusDice);


        // -------------------------
        // Hide result numbers
        // -------------------------

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);


        // -------------------------
        // Disable Animators
        //
        // During idle spinning WE control
        // the rotation, not the Animator.
        // -------------------------

        if (mainDiceAnimator != null)
        {
            mainDiceAnimator.speed = 1f;
            mainDiceAnimator.enabled = false;
        }

        if (bonusDiceAnimator != null)
        {
            bonusDiceAnimator.speed = 1f;
            bonusDiceAnimator.enabled = false;
        }


        // -------------------------
        // Pick initial random
        // spin directions
        // -------------------------

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
        // MAIN DIE
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


        // BONUS DIE
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
        Vector3 direction = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        );

        // Extremely unlikely, but prevents zero rotation.
        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.one;

        return direction.normalized;
    }


    // =========================================================
    // PLAY ROLL
    // =========================================================

    public IEnumerator PlayRoll(
        int mainResult,
        bool hasBonusDice,
        int bonusResult
    )
    {
        rolling = true;
        idleSpinning = false;


        // =====================================================
        // STOP RANDOM SPIN + RESET ROTATION
        // =====================================================

        if (mainDice != null)
            mainDice.localRotation = Quaternion.identity;

        if (bonusDice != null)
            bonusDice.localRotation = Quaternion.identity;


        // =====================================================
        // START MAIN DIE ANIMATION
        // =====================================================

        StartDiceAnimation(
            mainDiceAnimator,
            mainResult,
            "MAIN"
        );


        // =====================================================
        // START BONUS DIE ANIMATION
        // =====================================================

        if (hasBonusDice)
        {
            StartDiceAnimation(
                bonusDiceAnimator,
                bonusResult,
                "BONUS"
            );
        }


        // Give Unity one frame to enter DiceThrow.
        yield return null;


        // =====================================================
        // WAIT FOR BOTH DICE
        // =====================================================

        Coroutine mainWait = null;
        Coroutine bonusWait = null;

        if (mainDiceAnimator != null)
        {
            mainWait = StartCoroutine(
                WaitForResultAnimation(
                    mainDiceAnimator,
                    mainResult
                )
            );
        }

        if (hasBonusDice &&
            bonusDiceAnimator != null)
        {
            bonusWait = StartCoroutine(
                WaitForResultAnimation(
                    bonusDiceAnimator,
                    bonusResult
                )
            );
        }


        // Wait for main die.
        if (mainWait != null)
            yield return mainWait;


        // Wait for bonus die.
        if (bonusWait != null)
            yield return bonusWait;


        // =====================================================
        // SHOW NUMBERS
        // =====================================================

        if (rollNumberText != null)
        {
            rollNumberText.text =
                mainResult.ToString();

            rollNumberText.gameObject.SetActive(true);
        }
        OnResultShown?.Invoke();

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
        OnResultShown?.Invoke();

        // =====================================================
        // HOLD FINAL FRAME
        // =====================================================

        yield return new WaitForSecondsRealtime(
            finalFrameHoldTime
        );

        rolling = false;
    }


    // =========================================================
    // START ONE DIE
    // =========================================================

    private void StartDiceAnimation(
        Animator animator,
        int result,
        string diceName
    )
    {
        if (animator == null)
            return;

        // Turn Animator back on after idle spinning.
        animator.enabled = true;

        // VERY IMPORTANT:
        // Make sure it isn't still frozen from the previous roll.
        animator.speed = 1f;


        // Set result BEFORE starting DiceThrow.
        animator.SetInteger(
            "RollResult",
            result
        );


        Debug.Log(
            diceName +
            " DICE RESULT = " +
            result
        );


        // Restart DiceThrow from the beginning.
        animator.Play(
            "DiceThrow",
            0,
            0f
        );


        // Force Unity to apply this immediately.
        animator.Update(0f);


        Debug.Log(
            diceName +
            " ANIMATOR RollResult = " +
            animator.GetInteger("RollResult")
        );
    }


    // =========================================================
    // WAIT FOR RESULT ANIMATION
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


        // =====================================================
        // WAIT UNTIL DICETHROW ENTERS CORRECT RESULT STATE
        // =====================================================

        while (true)
        {
            AnimatorStateInfo state =
                animator.GetCurrentAnimatorStateInfo(0);

            if (state.IsName(expectedState))
                break;

            yield return null;
        }


        Debug.Log(
            animator.gameObject.name +
            " entered " +
            expectedState
        );


        // =====================================================
        // WAIT UNTIL RESULT ANIMATION REACHES END
        // =====================================================

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


        // =====================================================
        // FORCE EXACT FINAL FRAME
        // =====================================================

        animator.Play(
            expectedState,
            0,
            1f
        );

        animator.Update(0f);


        // =====================================================
        // FREEZE
        // =====================================================

        animator.speed = 0f;


        Debug.Log(
            animator.gameObject.name +
            " froze on final frame of " +
            expectedState
        );
    }


    // =========================================================
    // PLAYER STARTS MOVING
    // =========================================================

    public void HideForMovement()
    {
        idleSpinning = false;
        rolling = false;

        StopAllCoroutines();


        // Reset Animator speeds for next turn.
        if (mainDiceAnimator != null)
            mainDiceAnimator.speed = 1f;

        if (bonusDiceAnimator != null)
            bonusDiceAnimator.speed = 1f;


        // Hide numbers.
        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);


        // Hide displays.
        if (mainDiceDisplay != null)
            mainDiceDisplay.gameObject.SetActive(false);

        if (bonusDiceDisplay != null)
            bonusDiceDisplay.gameObject.SetActive(false);


        // Hide viewport parents.
        if (mainDiceViewport != null)
            mainDiceViewport.SetActive(false);

        if (bonusDiceViewport != null)
            bonusDiceViewport.SetActive(false);
    }


    // =========================================================
    // INITIAL HIDE
    // =========================================================

    private void HideImmediately()
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
}