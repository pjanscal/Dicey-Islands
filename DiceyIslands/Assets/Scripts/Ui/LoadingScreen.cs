using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [Header("Ui")]
    [SerializeField] private TextMeshProUGUI loadingText; //text that saw the dot dot dot

    private Canvas canvas;
    private string loadingBeginText = "Loading"; //default text for the dot dot text
    private float currentTimerBetweenTextUpdate = 0f;
    private int amountOfDot = 0; //current
    private int maxAmountOfDot = 3; //can't have more now it reset by ...

    //config
    private Vector2 fakeLoadingTime = new Vector2(2f, 3f); //time it take before loading the scene for real *(left is min right is max)
    private float timeBetweenTextUpdate = .4f; //time before getting a dot

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
        if (!GameMangeren.isLoading) return;

        UpdateText();
    }

    //update the dot dot dot text
    void UpdateText()
    {
        currentTimerBetweenTextUpdate += Time.deltaTime;
        if (currentTimerBetweenTextUpdate < timeBetweenTextUpdate) return; //wait until it get a other dot

        amountOfDot = (amountOfDot + 1) %(maxAmountOfDot + 1); //reset it when reaching 4
        loadingText.text = loadingBeginText + GetDotInString(); //set in the text with dot

        currentTimerBetweenTextUpdate = 0;
    }

    //get base on the amount of dot the string of dot's
    string GetDotInString()
    {
        string dot = ""; //default
        for (int i = 1; i <= amountOfDot; i++) //add one for every amount of dot there are
        {
            dot = dot + ".";
        }

        return dot;
    }

    //Load the scene with a loadingscreen
    public IEnumerator LoadScene(string sceneName)
    {
        canvas.enabled = true;
        GameMangeren.isLoading = true;

        //setup
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        yield return new WaitForSeconds(GetFakeLoadingTime()); //wait min for the feeling

        operation.allowSceneActivation = true;

        yield return new WaitUntil(() => operation.isDone); //wait until it is really done

        canvas.enabled = false;
        GameMangeren.isLoading = false;
        amountOfDot = 0;
        loadingText.text = loadingBeginText + GetDotInString();
    }

    //Find a random time to fake load so it feel random not a scam
    float GetFakeLoadingTime()
    {
        float rngTime = UnityEngine.Random.Range(fakeLoadingTime.x, fakeLoadingTime.y); //get a rng value
        return rngTime;
    }
}
