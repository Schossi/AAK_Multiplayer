using AdventureCore;
using AdventureExtras;
using System.Linq;
using UnityEngine;

public class MultiArenaPlayerCharacter : ArenaPlayer
{
    public MultiArenaPlayer Networker;
    public Material[] MaterialsDefault;
    public Material[] MaterialsGhost;
    public Material[] MaterialsFade;

    private MultiArenaGhostInstruction _ghostInstruction = new MultiArenaGhostInstruction();

    public override bool PreDamageReceive(IDamageSender sender, IDamageReceiver receiver)
    {
        if (IsReceiveDamageSuspended)
            return false;

        if (Networker.CheckReceiveDamage(sender.Damages.FirstOrDefault()?.Kind))
            return base.PreDamageReceive(sender, receiver);
        else
            return false;
    }
    public override bool PreDamageSend(IDamageSender sender, IDamageReceiver receiver)
    {
        if (IsSendDamageSuspended)
            return false;

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
        Model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = getDefaultMaterial();
    }

    public void DieLocal(Vector3 force)
    {
        Dead = true;

        var ragdoll = createRagdoll(force);

        ragdoll.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = getDefaultMaterial();
        ragdoll.GetComponent<FadeAndDestroy>().FadeMaterial = getFadeMaterial();

        Model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = getGhostMaterial();

        OnMessage("DEATH");

        AddInstruction(_ghostInstruction);

        MultiArenaStage.CurrentStage?.CheckGameOver();
    }

    public void ReviveLocal()
    {
        Dead = false;

        ResourcePool.AddResource(MultiArenaCommon.Instance.Health, 10, this);

        Model.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = getDefaultMaterial();

        OnMessage("REVIVE");

        RemoveInstruction(_ghostInstruction);
    }

    public void OnTargetChanged(LockOnPoint point)
    {
        if (MultiArenaCommon.Instance.LockOn)
            MultiArenaCommon.Instance.LockOn.SetTarget(point);
    }

    private Material getDefaultMaterial() => MaterialsDefault[(int)Networker.OwnerClientId % MaterialsDefault.Length];
    private Material getGhostMaterial() => MaterialsGhost[(int)Networker.OwnerClientId % MaterialsGhost.Length];
    private Material getFadeMaterial() => MaterialsFade[(int)Networker.OwnerClientId % MaterialsFade.Length];
}