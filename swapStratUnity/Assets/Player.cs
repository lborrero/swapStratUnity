using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

class Player : NetworkBehaviour
{
    public GameObject particle;

    [SyncVar]
    int health;

    [ClientRpc]
    void RpcDamage(int amount)
    {
        Debug.Log("Took damage:" + amount);
    }

    public void TakeDamage(int amount)
    {
        if (!isServer)
            return;

        health -= amount;
        RpcDamage(amount);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Asdfasdf");
            TakeDamage(1);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("whatever");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray))
                Instantiate(particle, transform.position, transform.rotation);
        }
    }

    void OnStartClient () 
    {
        Debug.Log("OnStartClient"); 
    }

    void OnStartServer()
    {
        Debug.Log("OnStartServer");
    }
}