using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class PauseSchrem: UiBasic
{
    [Header("setup")]
    [SerializeField] GameObject beginButton; //the button where the navigation begin on

    private Canvas canvas;
    private GameObject currentSelected; //current selected navigation object so if it null i can pick this one
    private LokaalConnecter.PlayerController currentPlrController; // this controll who is navigating
    private bool canMove = true; //can go navigate move again

    //configs
    private float navigationMoveDur = .3f;

    protected override void Start()
    {
        base.Start();

        DontDestroyOnLoad(gameObject);
        canvas = GetComponent<Canvas>();
    }

    protected override void Update()
    {
        base.Update();

        if (GameMangeren.isLoading) return; //stop it from pausing or doing things inside of loadingscreen

        TryPausing();
        CheckIfNotDisSelected();
        TryNavigation();
    }

    //help if u click on screen that it not dissconnet
    void CheckIfNotDisSelected()
    {
        if (!GameMangeren.isPaused) return;

        //check if eventsystem
        if (!EventSystem.current) return;
        
        //check or it have one else get it back
        if (EventSystem.current.currentSelectedGameObject != null) currentSelected = EventSystem.current.currentSelectedGameObject;
        else if (currentSelected != null) EventSystem.current.SetSelectedGameObject(currentSelected);
    }

    //go toggle pause
    public void TogglePause()
    {
        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.nothing || !GameMangeren.inGame) return; //make sure that it not gonna pause when it should

        GameMangeren.isPaused = !GameMangeren.isPaused; //make sure that some update stop by here
        Time.timeScale = GameMangeren.isPaused? 0 : 1; //stop the time *get the old timescale for minigame for later
        canvas.enabled = GameMangeren.isPaused;

        //set by beginning but there is not more ui so it is then useless
        EventSystem eventSystem = EventSystem.current;

        if (GameMangeren.isPaused)
        {
            eventSystem.sendNavigationEvents = false; //make it so i controll it
            eventSystem.SetSelectedGameObject(beginButton); //set the begin button in so it begin there
        }
        else 
        {
            eventSystem.SetSelectedGameObject(null); //remove it *tip if ther is a other one wich use it then remeber the old one
            eventSystem.sendNavigationEvents = true; //default
        }
    }
    
    //check or someone trying to pause
    void TryPausing()
    {
        if (LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.nothing || !GameMangeren.inGame) return;

        //loop through all input
        foreach (LokaalConnecter.PlayerController playerController in LokaalConnecter.plrsController.Values)
        {
            if (!playerController.GetButtonDown(LokaalConnecter.InputType.Pause)) continue;
            
            currentPlrController = playerController;
            TogglePause();
            return;
        }
    }

    //lokaal manaul eventSystem Support
    void TryNavigation()
    {
        if (!GameMangeren.isPaused) return;

        EventSystem eventSystem = EventSystem.current;

        //sumbit
        if (currentPlrController.GetButtonDown(LokaalConnecter.InputType.jump)) currentSelected.GetComponent<Button>().onClick.Invoke();

        //ui movement
        if (!canMove) return;

        //get the next object one
        Vector3? direction = GetMoveDir();
        if (direction == null) return;

        Selectable next;
        if (direction.Value == Vector3.up) next = currentSelected.GetComponent<Selectable>().FindSelectableOnUp();
        else if (direction.Value == Vector3.right) next = currentSelected.GetComponent<Selectable>().FindSelectableOnRight();
        else if (direction.Value == Vector3.left) next = currentSelected.GetComponent<Selectable>().FindSelectableOnLeft();
        else next = currentSelected.GetComponent<Selectable>().FindSelectableOnDown();
        if (next == null) return;

        canMove = false;
        eventSystem.SetSelectedGameObject(next.gameObject);

        //wait some sec to move again so it won't spam
        IEnumerator WaitToMoveAgain()
        {
            yield return new WaitForSecondsRealtime(navigationMoveDur);
            canMove = true;
        }

        StartCoroutine(WaitToMoveAgain());
    }

    //get the dir u are going
    Vector3? GetMoveDir()
    {
        Vector2 moveDir = currentPlrController.GetMoveDir();

        //check or it is the x or the y
        if (math.abs(moveDir.y) > math.abs(moveDir.x))
        {
            if (math.abs(moveDir.y) < .3f) return null; //make sure it is not a little
            return new Vector3(0, math.sign(moveDir.y), 0);
        }
        else
        {
            if (math.abs(moveDir.x) < .3f) return null; //make sure it is not a little
            return new Vector3(math.sign(moveDir.x), 0, 0);
        }
    }

    //reset all value
    public void QuitToMenu()
    {
        canvas.enabled = false;
        GameMangeren.Exit();
    }

    
}
