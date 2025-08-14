using AdventureCore;
using AdventureCore.Assets.SoftLeitner.AdventureCore.Item.Slots;
using AdventureExtras;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MultiArenaPlayer : NetworkBehaviour
{
    public CharacterBase Character;
    public NetworkCharacterControllerMovement Movement;
    public CharacterActionArea ActionArea;
    public ActionItemSlot WeaponSlot;
    public CharacterActionBase JumpAction;
    public CharacterActionBase RollAction;
    public UsableItemSlot UsableSlot1;
    public UsableItemSlot UsableSlot2;
    public UsableItemSlot UsableSlot3;
    public LockOnManager LockOnManager;

    private ArenaInput _input;

    private UnityAction<string> _gameStateChanged;
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

            _gameStateChanged = new UnityAction<string>(gameStateChanged);
            StateManager.Main.StateChanged.AddListener(_gameStateChanged);
            gameStateChanged(StateManager.Main.State);

            CharacterBase.RegisterCharacter(Character, "PL");
        }

        CharacterBase.RegisterCharacter(Character, "P" + OwnerClientId);
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsOwner)
        {
            gameStateChanged("-");
            StateManager.Main.StateChanged.RemoveListener(_gameStateChanged);

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
}
