using AdventureCore;
using System;
using UnityEngine;

/// <summary>
/// special version of movement that mostly disables movement on non-owners<br/>
/// send speeds needed for animation over its networker<br/>
/// the actual transformn needs to be synchronized using a network transform
/// </summary>
public class NetworkNavMeshAgentMovement : NavMeshAgentMovement
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
    }

    public override void ApplyRootMotion(Animator animator)
    {
        if (Networker.IsOwner)
            base.ApplyRootMotion(animator);
    }

    private void speedFactorForwardChanged(float oldValue, float newValue) => SpeedFactorForward = newValue;
    private void speedFactorSidewaysChanged(float oldValue, float newValue) => SpeedFactorSideways = newValue;

    public override void StartApproach(Action finished, float distance, float speed)
    {
        if (!Networker.IsOwner)
            return;

        base.StartApproach(finished, distance, speed);
    }
    public override void StartApproach(Transform destination, Action finished, float distance, float speed)
    {
        if (!Networker.IsOwner)
            return;

        base.StartApproach(destination, finished, distance, speed);
    }
    public override void StartApproach(Vector3 destination, Action finished, float distance, float speed)
    {
        if (!Networker.IsOwner)
            return;

        base.StartApproach(destination, finished, distance, speed);
    }
    protected override void startApproach(Transform transform, Vector3? position, Action finished, float distance, float speed, Vector3 destination)
    {
        if (!Networker.IsOwner)
            return;

        base.startApproach(transform, position, finished, distance, speed, destination);
    }

    public override void StopApproach()
    {
        if (!Networker.IsOwner)
            return;

        base.StopApproach();
    }

    public override void PropelCharacter(Vector3 value)
    {
        if (!Networker.IsOwner)
            return;

        base.PropelCharacter(value);
    }
}