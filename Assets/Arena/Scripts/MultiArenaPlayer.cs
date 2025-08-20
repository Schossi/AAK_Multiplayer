using AdventureCore;
using AdventureExtras;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiArenaPlayer : NetworkBehaviour
{
    public MultiArenaPlayerCharacter Character;
    public NetworkCharacterControllerMovement Movement;
    public CharacterActionArea ActionArea;
    public ActionItemSlot WeaponSlot;
    public CharacterActionBase JumpAction;
    public CharacterActionBase RollAction;
    public UsableItemSlot UsableSlot1;
    public UsableItemSlot UsableSlot2;
    public UsableItemSlot UsableSlot3;
    public LockOnManager LockOnManager;
    public ResourceDamage[] Damages;

    private ArenaInput _input;

    private bool _isInGame;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            _input = new ArenaInput();

            subscribe(_input.Default.Move, Movement.OnMove);

            subscribe(_input.Default.Action, ActionArea.OnStartAction);
            subscribe(_input.Default.Attack, WeaponSlot.Input);

            subscribe(_input.Default.Jump, JumpAction.Input);
            subscribe(_input.Default.Roll, RollAction.Input);

            subscribe(_input.Default.Quick1, UsableSlot1.Use);
            subscribe(_input.Default.Quick2, UsableSlot2.Use);
            subscribe(_input.Default.Quick3, UsableSlot3.Use);

            subscribe(_input.Default.Lock, LockOnManager.OnLock);
            subscribe(_input.Default.LockRight, LockOnManager.OnRight);
            subscribe(_input.Default.LockLeft, LockOnManager.OnLeft);

            StateManager.MainStateChanged += gameStateChanged;
            gameStateChanged(StateManager.Main.State);

            CharacterBase.RegisterCharacter(Character, "PL");
        }

        CharacterBase.RegisterCharacter(Character, "P" + OwnerClientId);

        Character.OnSpawn();
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsOwner)
        {
            gameStateChanged("-");
            StateManager.MainStateChanged -= gameStateChanged;

            unsubscribe(_input.Default.Move, Movement.OnMove);

            unsubscribe(_input.Default.Action, ActionArea.OnStartAction);
            unsubscribe(_input.Default.Attack, WeaponSlot.Input);

            unsubscribe(_input.Default.Jump, JumpAction.Input);
            unsubscribe(_input.Default.Roll, RollAction.Input);

            unsubscribe(_input.Default.Quick1, UsableSlot1.Use);
            unsubscribe(_input.Default.Quick2, UsableSlot2.Use);
            unsubscribe(_input.Default.Quick3, UsableSlot3.Use);

            unsubscribe(_input.Default.Lock, LockOnManager.OnLock);
            unsubscribe(_input.Default.LockRight, LockOnManager.OnRight);
            unsubscribe(_input.Default.LockLeft, LockOnManager.OnLeft);

            CharacterBase.UnregisterCharacter(Character, "PL");
        }

        CharacterBase.UnregisterCharacter(Character, "P" + OwnerClientId);
    }

    private void subscribe(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        action.started += callback;
        action.performed += callback;
        action.canceled += callback;
    }
    private void unsubscribe(InputAction action, Action<InputAction.CallbackContext> callback)
    {
        action.started += callback;
        action.performed += callback;
        action.canceled += callback;
    }

    private void gameStateChanged(string state)
    {
        _isInGame = state.Equals("DEFAULT", StringComparison.OrdinalIgnoreCase);

        if (_isInGame)
        {
            _input.Enable();
        }
        else
        {
            _input.Disable();
        }
    }

    public bool CheckReceiveDamage(DamageKind damageKind)
    {
        if (Array.IndexOf(Damages, damageKind) < 0)
            return true;//non-networked damage kind > LOCAL

        return IsOwner;
    }
    public void ReceiveDamage(DamageEvent damageEvent)
    {
        var i = Array.IndexOf(Damages, damageEvent.Kind);
        if (i < 0)
            return;

        receiveDamageRpc(i, damageEvent.Value, damageEvent.Vector);
    }
    [Rpc(SendTo.NotMe)]
    private void receiveDamageRpc(int damageIndex, float value, Vector3 vector)
    {
        var damage = Damages[damageIndex];
        var e = new DamageEvent()
        {
            Kind = damage,
            Value = value,
            Vector = vector,
            Receiver = Character,
        };

        if (ResourceBarManager.Instance)
            ResourceBarManager.Instance.Damage(e.Receiver.AssociatedCharacter.ResourcePool, damage.ResourceType, e, damage.ShowBar);

        if (damage.Add)
            Character.ResourcePool.AddResource(new ResourceQuantity(damage.ResourceType, value), this);
        else
            Character.ResourcePool.RemoveResource(new ResourceQuantity(damage.ResourceType, value), this);

        Character.PostDamageReceive(null, Character, new List<DamageEvent> { e });
    }

    public bool CheckSendDamage(IDamageReceiver receiver, DamageKind damageKind)
    {
        if (receiver?.AssociatedCharacter?.GetComponent<NetworkObject>() == null)
            return true;//receiver is not networked > LOCAL

        if (Array.IndexOf(Damages, damageKind) < 0)
            return true;//non-networked damage kind > LOCAL

        return IsOwner;
    }
    public void SendDamage(DamageEvent damageEvent)
    {
        var i = Array.IndexOf(Damages, damageEvent.Kind);
        if (i < 0)
            return;

        var receiver = damageEvent.Receiver.AssociatedCharacter.GetComponent<NetworkObject>();

        sendDamageRpc(receiver, i, damageEvent.Value, damageEvent.Vector);
    }
    [Rpc(SendTo.NotMe)]
    private void sendDamageRpc(NetworkObjectReference receiverReference, int damageIndex, float value, Vector3 vector)
    {
        if (!receiverReference.TryGet(out var receiver))
            return;


        var character = receiver.GetComponent<CharacterBase>();

        var damage = Damages[damageIndex];
        var e = new DamageEvent()
        {
            Kind = damage,
            Value = value,
            Vector = vector,
            Receiver = character,
        };

        if (ResourceBarManager.Instance)
            ResourceBarManager.Instance.Damage(e.Receiver.AssociatedCharacter.ResourcePool, damage.ResourceType, e, damage.ShowBar);

        if (damage.Add)
            character.ResourcePool.AddResource(new ResourceQuantity(damage.ResourceType, value), this);
        else
            character.ResourcePool.RemoveResource(new ResourceQuantity(damage.ResourceType, value), this);

        character.PostDamageReceive(null, character, new List<DamageEvent> { e });
    }

    public void SendDeath(Vector3 force)
    {
        if (Character.Dead)
            return;

        sendDeathRpc(force);
    }
    [Rpc(SendTo.Everyone)]
    private void sendDeathRpc(Vector3 force)
    {
        Character.DieLocal(force);
    }

    public void SendRevive()
    {
        if (!Character.Dead)
            return;

        Character.ReviveLocal();
        sendReviveRpc();
    }
    [Rpc(SendTo.NotMe)]
    private void sendReviveRpc()
    {
        Character.ReviveLocal();
    }

    public void SendEssence(int quantity)
    {
        sendEssenceRpc(quantity);
    }
    [Rpc(SendTo.Everyone)]
    private void sendEssenceRpc(int quantity)
    {
        Character.Inventory.AddItems(new ItemQuantity(MultiArenaCommon.Instance.Essence, quantity));
    }

    public void SendFadeOut()
    {
        fadeOutRpc();
    }
    [Rpc(SendTo.Everyone)]
    private void fadeOutRpc()
    {
        MultiArenaCommon.Instance.Fader.FadeOut();
    }
}
