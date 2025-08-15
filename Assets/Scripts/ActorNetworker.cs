using AdventureCore;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ActorNetworker : NetworkBehaviour
{
    public CharacterActorBase Actor;

    private UnityAction<CharacterActionBase> _actionStarted;

    private void Awake()
    {
        _actionStarted = new UnityAction<CharacterActionBase>(actionStarted);
    }

    private void OnEnable() => Actor.ActionStarted.AddListener(_actionStarted);
    private void OnDisable() => Actor.ActionStarted.RemoveListener(_actionStarted);

    private void actionStarted(CharacterActionBase action)
    {
        if (!IsOwner)
            return;

        if (Actor.Character.InventoryBase.Slots.OfType<UsableItemSlot>().Any(s => s.CurrentAction == action))
            return;//dont restart actions start by using item

        if (Actor.ChildActions.Values.Contains(action))
            startActorChildRpc(action.name);
        else if (action.transform.IsChildOf(Actor.Character.transform))
            startCharacterChildRpc(getPath(action.transform, Actor.Character.transform));
        else
            startGlobalRpc(getPath(action.transform));
    }

    [Rpc(SendTo.NotOwner)]
    private void startActorChildRpc(string name)
    {
        if (Actor.ChildActions.TryGetValue(name, out var action))
            Actor.StartAction(action, force: true);
    }

    [Rpc(SendTo.NotOwner)]
    private void startCharacterChildRpc(string path)
    {
        var transform = Actor.Character.transform.Find(path);
        var action = transform?.GetComponent<CharacterActionBase>();
        if (action)
            Actor.StartAction(action, force: true);
    }

    [Rpc(SendTo.NotOwner)]
    private void startGlobalRpc(string path)
    {
        var transform = GameObject.Find(path);
        var action = transform?.GetComponent<CharacterActionBase>();
        if (action)
            Actor.StartAction(action, force: true);
    }

    private static string getPath(Transform transform, Transform parent = null)
    {
        string path = transform.name;
        while (transform.parent != parent)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
