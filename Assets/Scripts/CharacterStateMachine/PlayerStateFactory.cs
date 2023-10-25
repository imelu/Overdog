using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFactory
{
    private PlayerStateManager _ctx;
    public PlayerStateFactory(PlayerStateManager currentContext)
    {
        _ctx = currentContext;
    }
    
    public PlayerIdleState Idle()
    {
        return new PlayerIdleState(_ctx, this);
    }
    
    public PlayerWalkState Walk()
    {
        return new PlayerWalkState(_ctx, this);
    }

    public PlayerPossessAimState PossessAim()
    {
        return new PlayerPossessAimState(_ctx, this);
    }

    public PlayerCorruptAimState CorruptAim()
    {
        return new PlayerCorruptAimState(_ctx, this);
    }

    public PlayerAttackAimState AttackAim()
    {
        return new PlayerAttackAimState(_ctx, this);
    }

    public PlayerQTEventState QTEvent()
    {
        return new PlayerQTEventState(_ctx, this);
    }
}
