using AdventureCore;
using AdventureExtras;
using System.Linq;
using UnityEngine;

public class MultiArenaPlayerCharacter : ArenaPlayer
{
    public MultiArenaPlayer Networker;
    public Material[] MaterialsDefault;
    public Material[] MaterialsFade;

    public override bool PreDamageReceive(IDamageSender sender, IDamageReceiver receiver)
    {
        if (Networker.CheckReceiveDamage(sender.Damages.FirstOrDefault()?.Kind))
            return base.PreDamageReceive(sender, receiver);
        else
            return false;
    }
    public override bool PreDamageSend(IDamageSender sender, IDamageReceiver receiver)
    {
        if (Networker.CheckSendDamage(receiver, sender.Damages.FirstOrDefault()?.Kind))
            return base.PreDamageSend(sender, receiver);
        else
            return false;
    }

    public override void OnDamageReceive(DamageEvent e)
    {
        base.OnDamageReceive(e);

        Networker.ReceiveDamage(e);
    }
    public override void OnDamageSend(DamageEvent e)
    {
        base.OnDamageSend(e);

        Networker.SendDamage(e);
    }

    public override void Die(Vector3 force)
    {
        if (Networker.IsOwner)
            Networker.SendDeath(force);
    }

    public void OnSpawn()
    {
        Model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = getPlayerMaterial((int)Networker.OwnerClientId);
    }

    public void DieLocal(Vector3 force)
    {
        var ragdoll = createRagdoll(force);

        ragdoll.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = getPlayerMaterial((int)Networker.OwnerClientId);
        ragdoll.GetComponent<FadeAndDestroy>().FadeMaterial = getPlayerMaterial((int)Networker.OwnerClientId, true);

        Movement.TeleportCharacter(Vector3.zero, Quaternion.identity);
        ResourcePool.ResetResources();
        EffectPool.ClearEffects();
    }

    private Material getPlayerMaterial(int playerIndex, bool fade = false)
    {
        if (fade)
            return MaterialsFade[playerIndex % MaterialsFade.Length];
        else
            return MaterialsDefault[playerIndex % MaterialsDefault.Length];
    }
}