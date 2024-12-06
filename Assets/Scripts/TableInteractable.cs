using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TableInteractable : NetworkBehaviour
{
    private GameObject ballObj;

    [SyncVar(hook = nameof(OnTableItemChanged))]
    private string tableItem;

    private void Start()
    {
        ballObj = this.transform.Find("InterestParent").transform.Find("Ball").gameObject;

        if (isServer == true)
        {
            tableItem = "Ball";
        }
        else
        {
            OnTableItemChanged(tableItem, tableItem);
        }
    }

    private void Update()
    {
        PlayerMove[] allPlayers = FindObjectsOfType<PlayerMove>();

        foreach (var player in allPlayers)
        {
            GameObject playerObj = player.gameObject;

            if (playerObj && player.isLocalPlayer == true && player.objPlayerIsNear == this.gameObject)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    CmdInteractWithTable(playerObj);
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    void CmdInteractWithTable(GameObject player)
    {
        if (tableItem == "Ball" && player.GetComponent<PlayerMove>().equippedItem == "")
        {
            player.GetComponent<PlayerMove>().equippedItem = tableItem;

            tableItem = "";
        }
        else if (tableItem == "" && player.GetComponent<PlayerMove>().equippedItem == "Ball")
        {
            tableItem = player.GetComponent<PlayerMove>().equippedItem;

            player.GetComponent<PlayerMove>().equippedItem = "";
        }
    }

    private void OnTableItemChanged(string oldTableItem, string newTableItem)
    {
        if (ballObj)
        {
            if (newTableItem == "")
            {
                ballObj.SetActive(false);
            }
            else 
            {
                if (newTableItem == "Ball")
                {
                    ballObj.SetActive(true);
                }
            }
        }
    }
}
