using AdventureCore;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// synchronizes equipped items and their usage over the network<br/>
/// when an item is equipped on the owner the networker commands other instances to do the same<br/>
/// using an item on a slot is also synced so usable slots can correctly instantiate their prefabs
/// </summary>
public class EquipmentNetworker : NetworkBehaviour
{
    private class InUseForwarder
    {
        private EquipmentNetworker _networker;
        private int _index;

        public InUseForwarder(EquipmentNetworker networker, int index)
        {
            _networker = networker;
            _index = index;
        }

        public void UsingItem(int quantity)
        {
            _networker.usingSlotItem(_index, quantity);
        }
    }

    [Tooltip("the equipment and usage of this inventory is synchronized over the network with the owner as the source")]
    public InventoryBase Inventory;

    private UnityAction<ItemSlotBase> _equipmentChanged;
    private InUseForwarder[] _useChanged;

    private void Awake()
    {
        _equipmentChanged = new UnityAction<ItemSlotBase>(equipmentChanged);
        _useChanged = new InUseForwarder[Inventory.Slots.Length];
        for (int i = 0; i < Inventory.Slots.Length; i++)
        {
            _useChanged[i] = new InUseForwarder(this, i);
        }
    }

    private void OnEnable()
    {
        Inventory.EquipmentChanged.AddListener(_equipmentChanged);
        for (int i = 0; i < Inventory.Slots.Length; i++)
        {
            Inventory.Slots[i].UsingItem += _useChanged[i].UsingItem;
        }
    }
    private void OnDisable()
    {
        Inventory.EquipmentChanged.RemoveListener(_equipmentChanged);
        for (int i = 0; i < Inventory.Slots.Length; i++)
        {
            Inventory.Slots[i].UsingItem -= _useChanged[i].UsingItem;
        }
    }

    private void equipmentChanged(ItemSlotBase slot)
    {
        if (!IsOwner)
            return;

        var slotIndex = Array.IndexOf(Inventory.Slots, slot);
        if (slotIndex < 0)
            return;

        setEquipmentRpc(slotIndex, slot?.EquippedInventoryItem?.Item?.Key ?? string.Empty);
    }

    private void usingSlotItem(int slotIndex, int quantity)
    {
        if (!IsOwner)
            return;

        useEquipmentRpc(slotIndex, quantity);
    }

    [Rpc(SendTo.NotOwner)]
    private void setEquipmentRpc(int slotIndex, string itemKey)
    {
        Inventory.Slots[slotIndex].Equip(Inventory.GetItem(Inventory.ItemSet.GetItem(itemKey)));
    }

    [Rpc(SendTo.NotOwner)]
    private void useEquipmentRpc(int slotIndex, int quantity)
    {
        Inventory.Slots[slotIndex].Use(quantity);
    }
}
