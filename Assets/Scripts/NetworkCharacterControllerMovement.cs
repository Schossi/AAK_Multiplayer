using AdventureCore;
using UnityEngine;

public class NetworkCharacterControllerMovement : CharacterControllerMovement
{
    [Header("Network")]
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
