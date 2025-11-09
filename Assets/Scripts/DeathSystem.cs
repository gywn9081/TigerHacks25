using UnityEngine;
using System;

/*
    This file is used to centralize information about deaths to have communications between players and controllers
*/

public class DeathSystem : MonoBehaviour
{
    public static int deathCount = 0;

    // This is the event used by players to indicate that they have died and need the whole team to re-spawn
    // public static event Action OnAnyPlayerDied;

    public static event System.Action OnAnyPlayerDied;

    public static void TriggerPlayerDeath()
    {
        Debug.Log("Player died invoking respawn");
        deathCount++;
        OnAnyPlayerDied?.Invoke();
    }
}