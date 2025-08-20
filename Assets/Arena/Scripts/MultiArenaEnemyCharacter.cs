using AdventureExtras;
using UnityEngine;

/// <summary>
/// special version of the arena enemy meant for multiplayer<br/>
/// centralizes enemy death to host to avoid desync
/// </summary>
public class MultiArenaEnemyCharacter : ArenaEnemy
{
    [Tooltip("component that communicates with instances on other machines")]
    public MultiArenaEnemy Networker;

    public override void Die(Vector3 force)
    {
        if (Networker.IsOwner)
            Networker.SendDeath(force);
    }

    public void DieLocal(Vector3 force)
    {
        base.Die(force);
    }
}