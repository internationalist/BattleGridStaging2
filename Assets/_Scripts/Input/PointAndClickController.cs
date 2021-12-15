using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PointAndClickController : MonoBehaviour
{
    PlayerController pc = null;
    private void Update()
    {
        if (PCInputManager.Instance.ClickOnDestination() && !EventSystem.current.IsPointerOverGameObject())
        {
            if(!TurnBasedSystem.I.activeTeam.aiAgent)
            {
                Clicked();
            }
        } else if (PCInputManager.Instance.RightClickOnDestination() && !EventSystem.current.IsPointerOverGameObject())
        {
            RightClicked();
        } else
        {
            RaycastHit hit = new RaycastHit();
            PlayerController thisPc = GeneralUtils.MousePointerOnCharacter(out hit);
            if(thisPc != null)
            {
                pc = thisPc;
                if(GameManager._currentPlayer == null
                    || "IdleFSM".Equals(GameManager._currentPlayer.getCurrentCommand().GetType().ToString())) { 
                //Command cmd = pc.getCurrentCommand();
                //if("IdleFSM".Equals(cmd.GetType().ToString())) {
                    pc.ShowInfoPanel();
                } else
                {
                    pc.HideInfoPanel();
                }
            } else
            {
                //Debug.Log("NO hover over character");
                if (pc != null)
                {
                    pc.HideInfoPanel();
                }
            }
        }
    }

    void Clicked()
    {
        RaycastHit hit = new RaycastHit();

        if (GeneralUtils.MousePointerOnGroundAndCharacters(out hit))
        {
            if ("Friendly".Equals(hit.transform.gameObject.tag))
            {
                GameManager.SelectPlayer(hit.transform.gameObject.GetComponent<PlayerController>());
            }
            else if (GameManager.playerSelected && "Ground".Equals(hit.transform.gameObject.tag) &&
                    !GameManager._currentPlayer.isAgent)
            {
                //Move command.
                GameManager.ActivateCommand(null, GameManager._currentPlayer.playerMetaData.moveLocation);
            }
            else if (GameManager.playerSelected && "Enemy".Equals(hit.transform.gameObject.tag))
            {
                GameManager.ActivateCommand(hit.transform, hit.point);
            } else
            {
                GameManager.SelectPlayer(null);
            }
        } else
        {
            GameManager.SelectPlayer(null);
        }
    }

    void RightClicked()
    {
        if(GameManager.playerSelected)
        {
            if(!GameManager._currentPlayer.getCurrentCommand().isActivated)
            {
                GameManager._currentPlayer.getCurrentCommand().cancel = true;
            }
        }
    }
}
