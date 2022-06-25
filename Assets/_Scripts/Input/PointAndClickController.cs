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
        if (PCInputManager.Instance.MouseClick() && !EventSystem.current.IsPointerOverGameObject())
        {
            Clicked();
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
                if(GameManager.playerSelected 
                    && Command.type.buff
                        .Equals(GameManager._currentPlayer.getCurrentCommand().commandType)
                        && !GameManager._currentPlayer.getCurrentCommand().isActivated)
                {
                    //TODO implement player controls to heal over here
                    //Buff/Heal friendly player
                    //GameManager.ActivateCommand(hit.transform, hit.point);
                }
                else
                {
                    //Select player
                    GameManager.SelectPlayer(hit.transform.gameObject.GetComponent<PlayerController>());
                }
            }
            else if (GameManager.playerSelected && "Ground".Equals(hit.transform.gameObject.tag) &&
                    !GameManager._currentPlayer.isAgent
                    && !GameManager._currentPlayer.getCurrentCommand().isActivated)
            {
                //TODO implement player controls to move over here
                //Move command.
                //GameManager.ActivateCommand(null, GameManager._currentPlayer.playerMetaData.moveLocation);
                GameManager._currentPlayer.SetMoveLocation(GameManager._currentPlayer.playerMetaData.moveLocation);
            }
            else if (GameManager.playerSelected && "Enemy".Equals(hit.transform.gameObject.tag)
                && !GameManager._currentPlayer.getCurrentCommand().isActivated)
            {
                //TODO implement player controls to attack over here
                //Attack command.
                //GameManager.ActivateCommand(hit.transform, hit.point);
            } /*else
            {
                //Select player
                GameManager.SelectPlayer(null);
            }*/
        }/* else
        {
            GameManager.SelectPlayer(null);
        }*/
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
