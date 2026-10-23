
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

    [Header("Combined Roll UI")]
    [SerializeField] private TMP_Text totalRollText;
    [SerializeField] private RectTransform totalRollRect;
    [SerializeField] private Vector2 combinedNumberPosition = new Vector2(0f, 0f);
    [SerializeField] private float combineDuration = 0.35f;
    [SerializeField] private float bonusStartDelay = 0.2f;

    [Header("Number Merge Sound")]
    [SerializeField] private AudioSource numberMergeAudioSource;
    [SerializeField] private AudioClip numberMergeSound;
    [Range(0f, 1f)]
    [SerializeField] private float numberMergeVolume = 1f;

    [Header("Roll Animation")]
    [SerializeField] private float finalFrameHoldTime = 0.75f;
    [SerializeField] private float animationTimeout = 8f;

    [Header("Dice UI Layout")]
    [SerializeField] private RectTransform mainDiceDisplayRect;
    [SerializeField] private RectTransform bonusDiceDisplayRect;

    [SerializeField] private RectTransform rollNumberRect;
    [SerializeField] private RectTransform bonusRollNumberRect;

    [Header("Layout Positions")]
    [SerializeField]
    private Vector2 singleDicePosition =
        new Vector2(0f, 0f);

    [SerializeField]
    private Vector2 mainDiceSplitPosition =
        new Vector2(-160f, 0f);

    [SerializeField]
    private Vector2 bonusDiceSplitPosition =
        new Vector2(160f, 0f);

    [Header("Layout Animation")]
    [SerializeField] private float layoutMoveSpeed = 8f;

    public Action OnResultShown;

    private bool movementCounterVisible;
    private TMP_Text movementCounterText;
    private Vector2 initialMainTextPosition;
    private Vector2 initialBonusTextPosition;
    private bool textPositionsCached;

    private void CacheTextPositions()
    {
        if (textPositionsCached) return;
        if (rollNumberRect == null && rollNumberText != null) rollNumberRect = rollNumberText.rectTransform;
        if (bonusRollNumberRect == null && bonusRollNumberText != null) bonusRollNumberRect = bonusRollNumberText.rectTransform;
        if (totalRollRect == null && totalRollText != null) totalRollRect = totalRollText.rectTransform;
        if (rollNumberRect != null) initialMainTextPosition = rollNumberRect.anchoredPosition;
        if (bonusRollNumberRect != null) initialBonusTextPosition = bonusRollNumberRect.anchoredPosition;
        textPositionsCached = true;
    }



    private void Awake()
    {
        // Do not deactivate this controller GameObject.
        // Only the dice viewports/displays are hidden.
        CacheTextPositions();
        HideImmediately();

        PauseAnimator(mainDiceAnimator);
        PauseAnimator(bonusDiceAnimator);
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

    private void UpdateDiceLayout(bool hasBonusDice)
    {
        Vector2 mainPosition = hasBonusDice
            ? mainDiceSplitPosition
            : singleDicePosition;

        Vector2 bonusPosition = bonusDiceSplitPosition;

        StopCoroutineIfRunning();

        layoutCoroutine = StartCoroutine(
            AnimateDiceLayout(mainPosition, bonusPosition)
        );
    }

    private Coroutine layoutCoroutine;

    private void StopCoroutineIfRunning()
    {
        if (layoutCoroutine != null)
        {
            StopCoroutine(layoutCoroutine);
            layoutCoroutine = null;
        }
    }

    private IEnumerator AnimateDiceLayout(
        Vector2 mainTarget,
        Vector2 bonusTarget)
    {
        while (true)
        {
            bool finished = true;

            if (mainDiceDisplayRect != null)
            {
                mainDiceDisplayRect.anchoredPosition =
                    Vector2.Lerp(
                        mainDiceDisplayRect.anchoredPosition,
                        mainTarget,
                        Mathf.Clamp01(layoutMoveSpeed * Time.unscaledDeltaTime)
                    );

                if (Vector2.Distance(
                    mainDiceDisplayRect.anchoredPosition,
                    mainTarget) > 0.1f)
                {
                    finished = false;
                }
            }

            if (bonusDiceDisplayRect != null)
            {
                bonusDiceDisplayRect.anchoredPosition =
                    Vector2.Lerp(
                        bonusDiceDisplayRect.anchoredPosition,
                        bonusTarget,
                        Mathf.Clamp01(layoutMoveSpeed * Time.unscaledDeltaTime)
                    );

                if (Vector2.Distance(
                    bonusDiceDisplayRect.anchoredPosition,
                    bonusTarget) > 0.1f)
                {
                    finished = false;
                }
            }

            if (rollNumberRect != null)
            {
                rollNumberRect.anchoredPosition =
                    Vector2.Lerp(
                        rollNumberRect.anchoredPosition,
                        mainTarget,
                        Mathf.Clamp01(layoutMoveSpeed * Time.unscaledDeltaTime)
                    );

                if (Vector2.Distance(
                    rollNumberRect.anchoredPosition,
                    mainTarget) > 0.1f)
                {
                    finished = false;
                }
            }

            if (bonusRollNumberRect != null)
            {
                bonusRollNumberRect.anchoredPosition =
                    Vector2.Lerp(
                        bonusRollNumberRect.anchoredPosition,
                        bonusTarget,
                        Mathf.Clamp01(layoutMoveSpeed * Time.unscaledDeltaTime)
                    );

                if (Vector2.Distance(
                    bonusRollNumberRect.anchoredPosition,
                    bonusTarget) > 0.1f)
                {
                    finished = false;
                }
            }

            if (finished)
                break;

            yield return null;
        }

        layoutCoroutine = null;
    }

    // =====================================================
    // SHOW DICE AT START OF TURN
    // =====================================================

    public void ShowForTurn(bool hasBonusDice)
    {
        StopAllCoroutines();
        layoutCoroutine = null;

        movementCounterVisible = false;
        CacheTextPositions();
        if (rollNumberRect != null) rollNumberRect.anchoredPosition = initialMainTextPosition;
        if (bonusRollNumberRect != null) bonusRollNumberRect.anchoredPosition = initialBonusTextPosition;
        if (totalRollText != null) totalRollText.gameObject.SetActive(false);
        UpdateDiceLayout(hasBonusDice);

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
    // PLAY ROLL
    // =====================================================

    public IEnumerator PlayRoll(
        int mainResult,
        bool hasBonusDice,
        int bonusResult)
    {

        if (mainDice != null) mainDice.localRotation = Quaternion.identity;

        if (bonusDice != null) bonusDice.localRotation = Quaternion.identity;

        StartDiceAnimation(mainDiceAnimator, mainResult, "MAIN");

        // Main animation is already playing while we wait to start the bonus.
        if (hasBonusDice)
        {
            yield return new WaitForSecondsRealtime(bonusStartDelay);
            StartDiceAnimation(bonusDiceAnimator, bonusResult, "BONUS");
        }
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

    }

    // Called by BoardManager after PlayRoll, before board movement.
    // The two numbers merge into a new total text. For one die, the
    // main result itself becomes the movement counter.
    public IEnumerator PrepareMovementCounter(int total, bool hasBonusDice)
    {
        CacheTextPositions();
        movementCounterVisible = true;
        movementCounterText = hasBonusDice && totalRollText != null
            ? totalRollText : rollNumberText;

        // Hide only the 3D dice viewports. Keep result texts for merging.
        HideDiceOnly();

        if (hasBonusDice && totalRollText != null &&
            rollNumberRect != null && bonusRollNumberRect != null)
        {
            Vector2 a = rollNumberRect.anchoredPosition;
            Vector2 b = bonusRollNumberRect.anchoredPosition;
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, combineDuration);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                rollNumberRect.anchoredPosition = Vector2.Lerp(a, combinedNumberPosition, t);
                bonusRollNumberRect.anchoredPosition = Vector2.Lerp(b, combinedNumberPosition, t);
                yield return null;
            }
            rollNumberText.gameObject.SetActive(false);
            bonusRollNumberText.gameObject.SetActive(false);
            if (totalRollRect != null) totalRollRect.anchoredPosition = combinedNumberPosition;
            totalRollText.text = total.ToString();
            totalRollText.gameObject.SetActive(true);

            // Play once, exactly when the two values become the total.
            if (numberMergeAudioSource != null && numberMergeSound != null)
                numberMergeAudioSource.PlayOneShot(numberMergeSound, numberMergeVolume);
            RectTransform popRect = totalRollText.rectTransform;
            Vector3 originalScale = popRect.localScale;
            popRect.localScale = originalScale * 0.65f;
            float popElapsed = 0f;
            while (popElapsed < 0.16f)
            {
                popElapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(popElapsed / 0.16f);
                popRect.localScale = Vector3.Lerp(originalScale * 0.65f, originalScale, t);
                yield return null;
            }
            popRect.localScale = originalScale;
        }
        else
        {
            if (bonusRollNumberText != null) bonusRollNumberText.gameObject.SetActive(false);
            if (totalRollText != null) totalRollText.gameObject.SetActive(false);
            if (rollNumberText != null)
            {
                rollNumberText.text = total.ToString();
                rollNumberText.gameObject.SetActive(true);
            }
        }
    }

    public void SetMovementStepsRemaining(int steps)
    {
        if (!movementCounterVisible || movementCounterText == null) return;
        movementCounterText.text = Mathf.Max(0, steps).ToString();
    }

    private void HideDiceOnly()
    {
        PauseAnimator(mainDiceAnimator);
        PauseAnimator(bonusDiceAnimator);
        if (mainDiceDisplay != null) mainDiceDisplay.gameObject.SetActive(false);
        if (bonusDiceDisplay != null) bonusDiceDisplay.gameObject.SetActive(false);
        if (mainDiceViewport != null) mainDiceViewport.SetActive(false);
        if (bonusDiceViewport != null) bonusDiceViewport.SetActive(false);
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
        layoutCoroutine = null;

        movementCounterVisible = false;
        movementCounterText = null;
        if (totalRollText != null) totalRollText.gameObject.SetActive(false);

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
        if (totalRollText != null) totalRollText.gameObject.SetActive(false);

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
