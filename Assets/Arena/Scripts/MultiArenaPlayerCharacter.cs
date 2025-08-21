using AdventureCore;
using AdventureExtras;
using System.Linq;
using UnityEngine;

/// <summary>
/// special version of arena player for multiplayer<br/>
/// implements special damage handling so players get and receive damage locally and then send it to other instances<br/>
/// also handles the different player colors and player death/revival
/// </summary>
public class MultiArenaPlayerCharacter : ArenaPlayer
{
    [Tooltip("component that communicates with instances on other machines")]
    public MultiArenaPlayer Networker;
    [Tooltip("default material for each player in join order")]
    public Material[] MaterialsDefault;
    [Tooltip("material for each player when they have run out of health")]
    public Material[] MaterialsGhost;
    [Tooltip("material for the fade animation of the ragdoll of each player")]
    public Material[] MaterialsFade;

    private MultiArenaGhostInstruction _ghostInstruction = new MultiArenaGhostInstruction();

    private void LateUpdate()
    {
        if (transform.position.y < 0)
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

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
        ResourcePool.Persist();

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