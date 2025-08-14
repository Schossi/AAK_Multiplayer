using AdventureCore;
using Unity.Netcode;
using UnityEngine;

public class NetworkCharacterControllerMovement : CharacterControllerMovement
{
    [Header("Network")]
    public NetworkObject NetworkObject;

    protected override void Update()
    {
        if (NetworkObject.IsOwner)
        {
            base.Update();
        }
        else
        {

        }
    }

    public override void ApplyRootMotion(Animator animator)
    {
        if (NetworkObject.IsOwner)
            base.ApplyRootMotion(animator);
    }
}
