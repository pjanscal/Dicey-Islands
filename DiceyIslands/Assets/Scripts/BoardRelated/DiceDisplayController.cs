
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceDisplayController : MonoBehaviour
{
    [Header("Main Dice")]
    [SerializeField] private GameObject mainDiceViewport;
    [SerializeField] private Transform mainDice;
    [SerializeField] private Animator mainDiceAnimator;
    [SerializeField] private RawImage mainDiceDisplay;
    [SerializeField] private TMP_Text rollNumberText;

    [Header("Bonus Dice")]
    [SerializeField] private GameObject bonusDiceViewport;
    [SerializeField] private Transform bonusDice;
    [SerializeField] private Animator bonusDiceAnimator;
    [SerializeField] private RawImage bonusDiceDisplay;
    [SerializeField] private TMP_Text bonusRollNumberText;

    [Header("Idle Animation")]
    [SerializeField] private float idleRotationSpeed = 120f;
    [SerializeField] private float directionChangeInterval = 0.35f;

    [Header("Roll Animation")]
    [SerializeField] private float finalFrameHoldTime = 0.75f;
    [SerializeField] private float animationTimeout = 8f;

    public Action OnResultShown;

    private bool idleSpinning;
    private bool rolling;

    private Vector3 mainSpinDirection;
    private Vector3 bonusSpinDirection;

    private float mainDirectionTimer;
    private float bonusDirectionTimer;

    private void Awake()
    {
        // Do not deactivate this controller GameObject.
        // Only the dice viewports/displays are hidden.
        HideImmediately();

        ConfigureAnimator(mainDiceAnimator);
        ConfigureAnimator(bonusDiceAnimator);
    }

    private void Start()
    {
        if (mainDice == null)
            Debug.LogError("DICE: Main Dice Transform is missing!", this);

        if (mainDiceAnimator == null)
            Debug.LogError("DICE: Main Dice Animator is missing!", this);

        if (mainDiceViewport == null)
            Debug.LogError("DICE: Main Dice Viewport is missing!", this);

        if (bonusDice == null)
            Debug.LogWarning("DICE: Bonus Dice Transform is missing!", this);

        if (bonusDiceAnimator == null)
            Debug.LogWarning("DICE: Bonus Dice Animator is missing!", this);

        Debug.Log("DICE: DiceDisplayController started successfully.");
    }

    private void Update()
    {
        if (!idleSpinning || rolling)
            return;

        SpinIdleDice();
    }

    private void ConfigureAnimator(Animator animator)
    {
        if (animator == null)
            return;

        animator.enabled = true;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.speed = 0f;
    }

    // =====================================================
    // SHOW DICE AT START OF TURN
    // =====================================================

    public void ShowForTurn(bool hasBonusDice)
    {
        StopAllCoroutines();

        rolling = false;
        idleSpinning = true;

        if (mainDiceViewport != null)
            mainDiceViewport.SetActive(true);

        if (mainDiceDisplay != null)
            mainDiceDisplay.gameObject.SetActive(true);

        if (bonusDiceViewport != null)
            bonusDiceViewport.SetActive(hasBonusDice);

        if (bonusDiceDisplay != null)
            bonusDiceDisplay.gameObject.SetActive(hasBonusDice);

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        PauseAnimator(mainDiceAnimator);
        PauseAnimator(bonusDiceAnimator);

        mainSpinDirection = GetRandomDirection();
        bonusSpinDirection = GetRandomDirection();

        mainDirectionTimer = directionChangeInterval;
        bonusDirectionTimer = directionChangeInterval;

        Debug.Log(
            "DICE: ShowForTurn called. Bonus = " + hasBonusDice +
            ", Controller active = " + gameObject.activeInHierarchy +
            ", Main viewport active = " +
            (mainDiceViewport != null && mainDiceViewport.activeInHierarchy)
        );
    }

    private void PauseAnimator(Animator animator)
    {
        if (animator == null)
            return;

        animator.enabled = true;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.speed = 0f;
    }

    // =====================================================
    // IDLE SPIN
    // =====================================================

    private void SpinIdleDice()
    {
        float delta = Time.unscaledDeltaTime;

        if (mainDice != null &&
            mainDiceViewport != null &&
            mainDiceViewport.activeInHierarchy)
        {
            mainDirectionTimer -= delta;

            if (mainDirectionTimer <= 0f)
            {
                mainSpinDirection = GetRandomDirection();
                mainDirectionTimer = directionChangeInterval;
            }

            mainDice.Rotate(
                mainSpinDirection * idleRotationSpeed * delta,
                Space.Self
            );
        }

        if (bonusDice != null &&
            bonusDiceViewport != null &&
            bonusDiceViewport.activeInHierarchy)
        {
            bonusDirectionTimer -= delta;

            if (bonusDirectionTimer <= 0f)
            {
                bonusSpinDirection = GetRandomDirection();
                bonusDirectionTimer = directionChangeInterval;
            }

            bonusDice.Rotate(
                bonusSpinDirection * idleRotationSpeed * delta,
                Space.Self
            );
        }
    }

    private Vector3 GetRandomDirection()
    {
        Vector3 direction = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        );

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.one;

        return direction.normalized;
    }

    // =====================================================
    // PLAY ROLL
    // =====================================================

    public IEnumerator PlayRoll(
        int mainResult,
        bool hasBonusDice,
        int bonusResult)
    {
        rolling = true;
        idleSpinning = false;

        if (mainDice != null)
            mainDice.localRotation = Quaternion.identity;

        if (bonusDice != null)
            bonusDice.localRotation = Quaternion.identity;

        StartDiceAnimation(mainDiceAnimator, mainResult, "MAIN");

        if (hasBonusDice)
            StartDiceAnimation(bonusDiceAnimator, bonusResult, "BONUS");

        // Allow the Animator to advance.
        yield return null;

        Coroutine mainWait = null;
        Coroutine bonusWait = null;

        if (mainDiceAnimator != null)
        {
            mainWait = StartCoroutine(
                WaitForResultAnimation(mainDiceAnimator, mainResult)
            );
        }

        if (hasBonusDice && bonusDiceAnimator != null)
        {
            bonusWait = StartCoroutine(
                WaitForResultAnimation(bonusDiceAnimator, bonusResult)
            );
        }

        if (mainWait != null)
            yield return mainWait;

        if (bonusWait != null)
            yield return bonusWait;

        // Show results only after both dice finish.
        if (rollNumberText != null)
        {
            rollNumberText.text = mainResult.ToString();
            rollNumberText.gameObject.SetActive(true);
        }

        if (bonusRollNumberText != null)
        {
            bonusRollNumberText.gameObject.SetActive(hasBonusDice);

            if (hasBonusDice)
                bonusRollNumberText.text = bonusResult.ToString();
        }

        // Fire once, after both result texts are updated.
        OnResultShown?.Invoke();

        yield return new WaitForSecondsRealtime(finalFrameHoldTime);

        rolling = false;
    }

    // =====================================================
    // START ANIMATION
    // =====================================================

    private void StartDiceAnimation(
        Animator animator,
        int result,
        string diceName)
    {
        if (animator == null)
        {
            Debug.LogError("DICE: " + diceName + " Animator missing!");
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError(
                "DICE: " + diceName + " has no Animator Controller!"
            );
            return;
        }

        animator.enabled = true;
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.speed = 1f;

        int throwHash = Animator.StringToHash("Base Layer.DiceThrow");

        if (!animator.HasState(0, throwHash))
        {
            Debug.LogError(
                "DICE: " + diceName +
                " cannot find DiceThrow in its Animator Controller!"
            );
            return;
        }

        animator.SetInteger("RollResult", result);
        animator.Play(throwHash, 0, 0f);
        animator.Update(0f);

        Debug.Log(
            "DICE: " + diceName +
            " started DiceThrow, result = " + result
        );
    }

    // =====================================================
    // WAIT FOR RESULT ANIMATION
    // =====================================================

    private IEnumerator WaitForResultAnimation(
        Animator animator,
        int result)
    {
        if (animator == null ||
            animator.runtimeAnimatorController == null)
            yield break;

        string expectedState = "DiceRolling" + result;
        int resultHash = Animator.StringToHash(
            "Base Layer." + expectedState
        );

        if (!animator.HasState(0, resultHash))
        {
            Debug.LogError(
                "DICE: Missing animation state " + expectedState,
                animator
            );
            yield break;
        }

        float startTime = Time.realtimeSinceStartup;
        bool enteredResultState = false;

        while (Time.realtimeSinceStartup - startTime < animationTimeout)
        {
            AnimatorStateInfo state =
                animator.GetCurrentAnimatorStateInfo(0);

            if (state.fullPathHash == resultHash)
            {
                enteredResultState = true;
                break;
            }

            yield return null;
        }

        if (!enteredResultState)
        {
            Debug.LogError(
                "DICE: Timed out waiting for " + expectedState +
                " on " + animator.gameObject.name
            );

            // Fallback: display the expected result pose.
            animator.Play(resultHash, 0, 1f);
            animator.Update(0f);
            animator.speed = 0f;
            yield break;
        }

        startTime = Time.realtimeSinceStartup;
        bool finished = false;

        while (Time.realtimeSinceStartup - startTime < animationTimeout)
        {
            AnimatorStateInfo state =
                animator.GetCurrentAnimatorStateInfo(0);

            if (state.fullPathHash == resultHash &&
                state.normalizedTime >= 1f)
            {
                finished = true;
                break;
            }

            yield return null;
        }

        if (!finished)
        {
            Debug.LogWarning(
                "DICE: Result animation timed out: " +
                expectedState
            );
        }

        // Force the final frame.
        animator.Play(resultHash, 0, 1f);
        animator.Update(0f);
        animator.speed = 0f;

        Debug.Log(
            "DICE: " + animator.gameObject.name +
            " finished " + expectedState
        );
    }

    // =====================================================
    // HIDE FOR MOVEMENT
    // =====================================================

    public void HideForMovement()
    {
        StopAllCoroutines();

        rolling = false;
        idleSpinning = false;

        PauseAnimator(mainDiceAnimator);
        PauseAnimator(bonusDiceAnimator);

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        if (mainDiceDisplay != null)
            mainDiceDisplay.gameObject.SetActive(false);

        if (bonusDiceDisplay != null)
            bonusDiceDisplay.gameObject.SetActive(false);

        if (mainDiceViewport != null)
            mainDiceViewport.SetActive(false);

        if (bonusDiceViewport != null)
            bonusDiceViewport.SetActive(false);
    }

    // =====================================================
    // INITIAL HIDE
    // =====================================================

    private void HideImmediately()
    {
        idleSpinning = false;
        rolling = false;

        if (rollNumberText != null)
            rollNumberText.gameObject.SetActive(false);

        if (bonusRollNumberText != null)
            bonusRollNumberText.gameObject.SetActive(false);

        if (mainDiceDisplay != null)
            mainDiceDisplay.gameObject.SetActive(false);

        if (bonusDiceDisplay != null)
            bonusDiceDisplay.gameObject.SetActive(false);

        if (mainDiceViewport != null)
            mainDiceViewport.SetActive(false);

        if (bonusDiceViewport != null)
            bonusDiceViewport.SetActive(false);
    }
}
