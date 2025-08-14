using Unity.Netcode;

public class MovementNetworker : NetworkBehaviour
{
    public NetworkVariable<float> SpeedFactorForward { get; } = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> SpeedFactorSideways { get; } = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
}
