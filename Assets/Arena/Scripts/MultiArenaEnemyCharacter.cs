using AdventureExtras;
using UnityEngine;

public class MultiArenaEnemyCharacter : ArenaEnemy
{
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