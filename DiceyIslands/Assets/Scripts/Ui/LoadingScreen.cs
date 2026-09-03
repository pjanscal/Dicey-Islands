using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingText;

    private Canvas canvas;
    private string loadingBeginText = "Loading";
    private bool isLoading = false; //so it can do things while loading
    private float currentTimerBetweenTextUpdate = 0f;
    private int amountOfDot = 0; //current
    private int maxAmountOfDot = 3; //can't have more now it reset by ...

    //config
    private Vector2 fakeLoadingTime = new Vector2(2f, 3f); //left is min right is max
    private float timeBetweenTextUpdate = .4f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameMangeren.loadingScreen = this;
        canvas = GetComponent<Canvas>();

        DontDestroyOnLoad(gameObject);
        loadingText.text = loadingBeginText + GetDotInString();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLoading) return;

        UpdateText();
    }

    void UpdateText()
    {
        currentTimerBetweenTextUpdate += Time.deltaTime;
        if (currentTimerBetweenTextUpdate < timeBetweenTextUpdate) return;

        amountOfDot = (amountOfDot + 1) %(maxAmountOfDot + 1); //reset it when reaching 4
        loadingText.text = loadingBeginText + GetDotInString();

        currentTimerBetweenTextUpdate = 0;
    }

    string GetDotInString()
    {
        string dot = "";
        for (int i = 1; i <= amountOfDot; i++)
        {
            dot = dot + ".";
        }

        return dot;
    }

    public IEnumerator LoadScene(string sceneName)
    {
        canvas.enabled = true;
        isLoading = true;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        yield return new WaitForSeconds(GetFakeLoadingTime());

        operation.allowSceneActivation = true;

        yield return new WaitUntil(() => operation.isDone);

        canvas.enabled = false;
        isLoading = false;
        amountOfDot = 0;
        loadingText.text = loadingBeginText + GetDotInString();
    }

    float GetFakeLoadingTime()
    {
        float rngTime = UnityEngine.Random.Range(fakeLoadingTime.x, fakeLoadingTime.y);
        return rngTime;
    }
}
