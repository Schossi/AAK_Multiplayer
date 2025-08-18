using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MultiArenaEnemy : NetworkBehaviour
{
    public int PrefabIndex;
    public ScriptMachine ScriptMachine;
    public CapsuleCollider Collider;

    private void Awake()
    {
        ScriptMachine.enabled = false;

        Collider.enabled = false;
        StartCoroutine(bringInCollider());
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ScriptMachine.enabled = IsOwner;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        ScriptMachine.enabled = false;
    }

    /// <summary>
    /// brings in collider over time to avoid kicking players around from sudden spawn
    /// </summary>
    /// <returns></returns>
    private IEnumerator bringInCollider()
    {
        yield return null;

        var end = Collider.center;
        var start = end - new Vector3(0, 0, 1.5f);
        var duration = 0.5f;
        var time = 0.0f;

        Collider.enabled = true;

        while (time < duration)
        {
            Collider.center = Vector3.Lerp(start, end, time / duration);
            yield return null;
            time += Time.deltaTime;
        }

        Collider.center = end;
    }
}
