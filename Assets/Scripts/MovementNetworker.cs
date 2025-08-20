using Unity.Netcode;

/// <summary>
/// networking component that synchornizes movement values over the network
/// </summary>
public class MovementNetworker : NetworkBehaviour
{
    public NetworkVariable<float> SpeedFactorForward { get; } = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> SpeedFactorSideways { get; } = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
}
