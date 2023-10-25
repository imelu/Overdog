using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoverEvents : MonoBehaviour
{
    public void DropCurtain()
    {
        GlobalGameManager.Instance.PlayerWon();
    }

    public void ConfettiDrop()
    {
        GlobalGameManager.Instance.ConfettiDrop();
    }
}
