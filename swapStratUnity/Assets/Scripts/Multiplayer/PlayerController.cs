
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAction
{
    SHOOT,
    JUMP
}

public class PlayerController : MonoBehaviour
{
    public delegate void PlayerInputCallback(PlayerAction action, float deg);
    public event PlayerInputCallback OnPlayerInput;
    public bool isLocalPlayer = false;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Shoot();
    }

    public void SetupLocalPlayer()
    {
        //add color to your player
        isLocalPlayer = true;
    }

    public void TurnStart()
    {
        if (isLocalPlayer)
        {
            //spawn or enable player
        }
    }

    public void TurnEnd()
    {
        if (isLocalPlayer)
        {
            // unspawn or disable player
        }
    }

    public void Shoot()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        float power = 50f;

        if (Application.platform == RuntimePlatform.Android)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                //OnPlayerInput(PlayerAction.SHOOT, power);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Shoot");
                //OnPlayerInput(PlayerAction.SHOOT, power);
            }
        }
    }
}