using AdventureCore;
using UnityEngine;

/// <summary>
/// instruction that disables damage and collisions for players that have run out of HP
/// </summary>
public class MultiArenaGhostInstruction : CharacterInstructionBase
{
    public override void Apply(CharacterBase character)
    {
        var player = (MultiArenaPlayerCharacter)character;

        player.tag = "Untagged";
        player.Movement.GetComponent<CharacterController>().excludeLayers = LayerMask.GetMask("Enemy");
        player.IsSendDamageSuspended = true;
        player.IsReceiveDamageSuspended = true;
    }

    public override void Reset(CharacterBase character)
    {
        var player = (MultiArenaPlayerCharacter)character;

        player.tag = "Player";
        player.Movement.GetComponent<CharacterController>().excludeLayers = 0;
        player.IsSendDamageSuspended = false;
        player.IsReceiveDamageSuspended = false;
    }
}