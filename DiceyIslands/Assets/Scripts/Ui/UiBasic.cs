using UnityEngine;

public class UiBasic : MonoBehaviour
{
    /*tutorial
    put all ui in a empty that will be the gui inside the main GUi(Canvas)
    */

    //[SerializeField] protected GameObject beginGui;
    
    //protected GameObject currentGui; //the gui u in right now

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        //currentGui = beginGui;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    //go send a messaage to the server to switch to that scene
    public virtual void SwitchScene(string sceneName)
    {
        GameMangeren.SwitchScene(sceneName);
    }

    //go switch gui and disable the old one 
    /*
    public virtual void SwitchGui(GameObject newGui)
    {
        newGui.SetActive(true);
        currentGui.SetActive(false);

        currentGui = newGui;
    } */
}
