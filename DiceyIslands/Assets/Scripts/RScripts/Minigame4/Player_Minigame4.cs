using UnityEngine;

public class Player_Minigame4 : MonoBehaviour
{
    [HideInInspector] public float speed;

    //configs
    private float defaultSpeed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!Minigame4Mangeren.instance.isActive) return;

        Move();
    }

    //move the player
    void Move()
    {
        
    }
}
