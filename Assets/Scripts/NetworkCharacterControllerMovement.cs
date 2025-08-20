using AdventureCore;
using UnityEngine;

/// <summary>
/// special version of movement that mostly disables movement on non-owners<br/>
/// send speeds needed for animation over its networker<br/>
/// the actual transformn needs to be synchronized using a network transform
/// </summary>
public class NetworkCharacterControllerMovement : CharacterControllerMovement
{
    [Header("Network")]
    [Tooltip("component that communicates with instances on other machines")]
    public MovementNetworker Networker;

    private void OnEnable()
    {
        Networker.SpeedFactorForward.OnValueChanged += speedFactorForwardChanged;
        Networker.SpeedFactorSideways.OnValueChanged += speedFactorSidewaysChanged;
    }

    private void OnDisable()
    {
        Networker.SpeedFactorForward.OnValueChanged -= speedFactorForwardChanged;
        Networker.SpeedFactorSideways.OnValueChanged -= speedFactorSidewaysChanged;
    }

    protected override void Update()
    {
        if (Networker.IsOwner)
        {
            base.Update();

            Networker.SpeedFactorForward.Value = SpeedFactorForward;
            Networker.SpeedFactorSideways.Value = SpeedFactorSideways;
        }
        else
        {
            if (GroundingSphere)
                IsGrounded = Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - GroundingSphereOffset, transform.position.z), GroundingSphereRadius, GroundingSphereLayers, QueryTriggerInteraction.Ignore);
            else
                IsGrounded = _characterController.isGrounded;
        }
    }

    public override void ApplyRootMotion(Animator animator)
    {
        if (Networker.IsOwner)
            base.ApplyRootMotion(animator);
    }

    private void speedFactorForwardChanged(float oldValue, float newValue) => SpeedFactorForward = newValue;
    private void speedFactorSidewaysChanged(float oldValue, float newValue) => SpeedFactorSideways = newValue;
}
