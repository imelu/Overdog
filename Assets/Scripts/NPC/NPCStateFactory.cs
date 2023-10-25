using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateFactory
{
    private NPCAIStateManager _ctx;
    public NPCStateFactory(NPCAIStateManager currentContext)
    {
        _ctx = currentContext;
    }

    #region general

    public NPCIdleState Idle()
    {
        return new NPCIdleState(_ctx, this);
    }

    public NPCWalkState Walk()
    {
        return new NPCWalkState(_ctx, this);
    }

    public NPCWalkToRoomState WalkToRoom()
    {
        return new NPCWalkToRoomState(_ctx, this);
    }

    public NPCFollowGroupState FollowGroup()
    {
        return new NPCFollowGroupState(_ctx, this);
    }

    public NPCGroupIdleState GroupIdle()
    {
        return new NPCGroupIdleState(_ctx, this);
    }

    public NPCFormGroupState FormGroup()
    {
        return new NPCFormGroupState(_ctx, this);
    }
    public NPCJoinGroupState JoinGroup()
    {
        return new NPCJoinGroupState(_ctx, this);
    }

    public NPCGetCorruptedState GetCorrupted()
    {
        return new NPCGetCorruptedState(_ctx, this);
    }

    public NPCAvoidPlayerState AvoidPlayer()
    {
        return new NPCAvoidPlayerState(_ctx, this);
    }

    public NPCReleasedState Released()
    {
        return new NPCReleasedState(_ctx, this);
    }

    public NPCGoToBossState GoToBoss()
    {
        return new NPCGoToBossState(_ctx, this);
    }

    public NPCFollowPlayerState FollowPlayer()
    {
        return new NPCFollowPlayerState(_ctx, this);
    }

    #endregion

    #region Janitor

    public NPCJanitorWalkState JanitorWalk()
    {
        return new NPCJanitorWalkState(_ctx, this);
    }

    #endregion

    #region Police

    public NPCPoliceWatchState PoliceWatch()
    {
        return new NPCPoliceWatchState(_ctx, this);
    }

    public NPCPolicePunishState PolicePunish()
    {
        return new NPCPolicePunishState(_ctx, this);
    }

    public NPCPoliceCorruptState PoliceCorrupt()
    {
        return new NPCPoliceCorruptState(_ctx, this);
    }

    #endregion

    #region Security

    public NPCSecurityPatrolState SecurityPatrol()
    {
        return new NPCSecurityPatrolState(_ctx, this);
    }

    #endregion

    #region Guard

    public NPCGuardState Guard()
    {
        return new NPCGuardState(_ctx, this);
    }

    #endregion

    #region Entertainer

    public NPCEntertainerPlayingState EntertainerPlaying()
    {
        return new NPCEntertainerPlayingState(_ctx, this);
    }

    public NPCListenToEntertainerState ListenToEntertainer()
    {
        return new NPCListenToEntertainerState(_ctx, this);
    }

    #endregion

    #region Tutorial

    public NPCTutorialState Tutorial()
    {
        return new NPCTutorialState(_ctx, this);
    }

    #endregion
}
