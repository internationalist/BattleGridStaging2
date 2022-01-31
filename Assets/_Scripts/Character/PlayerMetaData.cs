using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerMetaData 
{
    [SerializeField]
    private int hp;
    [SerializeField]
    public int maxHP;
    public int maxActionPoints;
    public float distanceToMove;
    public float movementRange;
    public Vector3 moveLocation;
    public int maxAttackCount;
    [SerializeField]
    private int attackCount;
    [SerializeField]
    private int itemUseCount;
    public int maxMoveCount;
    [SerializeField]
    private int moveCount;
    public int itemCoolDown;
    [SerializeField]
    public int turnsLeftForItemUse;
    [Range(0,1)]
    public float decimateChance;
    public float grenadeTargetDistance;

    public int maxCommands;

    public int maxReloadCommands = 1;
    private int reloadCommandsGiven;
    public VoiceModule voice;

    public Grunts grunts;


    //public float ApNeeded { get => apNeeded; set => apNeeded = value; }
    //public float Ap { get => ap; set => ap = value; }
    public int Hp { get => hp; set => hp = value; }
    public int AttackCount { get => attackCount; set => attackCount = value; }
    public int MoveCount { get => moveCount; set => moveCount = value; }
    public int ItemUseCount { get => itemUseCount; set => itemUseCount = value; }

    public void Initialize()
    {
        hp = maxHP;
    }

    public bool WithinRange() 
    {
        return distanceToMove <= movementRange;
    }


    public void IncrementAttackCount()
    {
        //TODO: We should not maintain two different variables here.
        ++AttackCount;
    }

    public void IncrementMoveCount()
    {
        ++MoveCount;
    }

    public void IncrementItemUseCount()
    {
        turnsLeftForItemUse = itemCoolDown;
        ++ItemUseCount;
    }

    public void IncrementReloadCount()
    {
        ++reloadCommandsGiven;
    }

    public bool CanUseItem()
    {
        return turnsLeftForItemUse == 0;
    }

    public bool CanAttack()
    {
        bool canAttack = maxAttackCount - AttackCount > 0;
        return canAttack;
    }

    public bool CanReload()
    {
        return maxReloadCommands - reloadCommandsGiven > 0;
    }

    public bool CanMove()
    {
        return maxMoveCount - MoveCount > 0;
    }

    public bool CanRunCommand()
    {
        return maxCommands - (attackCount + moveCount + reloadCommandsGiven + itemUseCount) > 0;
    }

    public void TakeDamage(int damageAmt)
    {
        hp -= damageAmt;
    }

    public float DamagePercent(int damageAmt)
    {
        return (float)damageAmt / hp;
    }

    public float NormalizedHealthRemaining()
    {
        float normalizedValue = (float)hp / maxHP;
        return normalizedValue;
    }

    public void GetReadyForTurn()
    {
        AttackCount = 0;
        MoveCount = 0;
        itemUseCount = 0;
        reloadCommandsGiven = 0;
        if(turnsLeftForItemUse > 0)
        {
            --turnsLeftForItemUse;
        }
    }
}
