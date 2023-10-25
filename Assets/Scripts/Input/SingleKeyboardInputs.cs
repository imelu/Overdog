using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class SingleKeyboardInputs : MonoBehaviour
{
    public PlayerStateManager P1;
    public PlayerStateManager P2;

    public QuickTimeHandler QTP1;
    public QuickTimeHandler QTP2;

    private void DisableMe()
    {
        GetComponent<PlayerInput>().actions = null;
    }

    #region inputP1
    public void OnMove(InputValue inputValue)
    {
        P1.OnMove(inputValue);
    }

    public void OnSprint()
    {
        P1.OnSprint();
    }

    public void OnPossessAim()
    {
        P1.OnPossessAim();
    }

    public void OnPossessRelease()
    {
        P1.OnPossessRelease();
    }

    public void OnCorruptAim()
    {
        P1.OnCorruptAim();
    }

    public void OnCorruptRelease()
    {
        P1.OnCorruptRelease();
    }

    public void OnAttackAim()
    {
        P1.OnAttackAim();
    }

    public void OnAttackRelease()
    {
        P1.OnAttackRelease();
    }

    public void OnBackToMenu()
    {
        if(P1.OnBackToMenu()) DisableMe();
    }

    #endregion

    #region inputP2
    public void OnMoveP2(InputValue inputValue)
    {
        P2.OnMove(inputValue);
    }

    public void OnSprintP2()
    {
        P2.OnSprint();
    }

    public void OnPossessAimP2()
    {
        P2.OnPossessAim();
    }

    public void OnPossessReleaseP2()
    {
        P2.OnPossessRelease();
    }

    public void OnCorruptAimP2()
    {
        P2.OnCorruptAim();
    }

    public void OnCorruptReleaseP2()
    {
        P2.OnCorruptRelease();
    }

    public void OnAttackAimP2()
    {
        P2.OnAttackAim();
    }

    public void OnAttackReleaseP2()
    {
        P2.OnAttackRelease();
    }

    #endregion

    #region QTP1

    public void OnX()
    {
        QTP1.OnX();
    }

    public void OnSquare()
    {
        QTP1.OnSquare();
    }

    public void OnCircle()
    {
        QTP1.OnCircle();
    }

    public void OnTriangle()
    {
        QTP1.OnTriangle();
    }

    public void OnLeft()
    {
        QTP1.OnLeft();
    }

    public void OnRight()
    {
        QTP1.OnRight();
    }

    public void OnUp()
    {
        QTP1.OnUp();
    }

    public void OnDown()
    {
        QTP1.OnDown();
    }

    #endregion

    #region QTP2

    public void OnXP2()
    {
        QTP2.OnX();
    }

    public void OnSquareP2()
    {
        QTP2.OnSquare();
    }

    public void OnCircleP2()
    {
        QTP2.OnCircle();
    }

    public void OnTriangleP2()
    {
        QTP2.OnTriangle();
    }

    public void OnLeftP2()
    {
        QTP2.OnLeft();
    }

    public void OnRightP2()
    {
        QTP2.OnRight();
    }

    public void OnUpP2()
    {
        QTP2.OnUp();
    }

    public void OnDownP2()
    {
        QTP2.OnDown();
    }

    #endregion
}
