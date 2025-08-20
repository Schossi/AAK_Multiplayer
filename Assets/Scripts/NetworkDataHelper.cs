using AdventureCore;
using System.Linq;
using Unity.Netcode;

public static class NetworkDataHelper
{
    public static NetworkCharacterData GetData(CharacterBase character)
    {
        return new NetworkCharacterData()
        {
            InventoryItems = GetInventoryData(character),
            ResourceValues = GetResourceData(character),
            AttributeValues = GetAttributeData(character),
        };
    }
    public static void SetData(CharacterBase character, NetworkCharacterData data)
    {
        character.InventoryBase.Persister?.Set(GetInventoryData(data.InventoryItems), InventoryBase.PERSISTENCE_SUB_KEY);
        character.InventoryBase.Persister?.Set(GetResourceData(data.ResourceValues), ResourcePool.PERSISTENCE_SUB_KEY);
        character.InventoryBase.Persister?.Set(GetAttributeData(data.AttributeValues), AttributePool.PERSISTENCE_SUB_KEY);
    }
    public static void ApplyData(CharacterBase character)
    {
        character.InventoryBase.Retrieve();
        character.ResourcePool.Retrieve();
        character.AttributePool.Retrieve();
    }

    public struct NetworkCharacterData : INetworkSerializable
    {
        public NetworkInventoryItem[] InventoryItems;
        public NetworkResourceValue[] ResourceValues;
        public NetworkAttributeValue[] AttributeValues;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref InventoryItems);
            serializer.SerializeValue(ref ResourceValues);
            serializer.SerializeValue(ref AttributeValues);
        }
    }

    public static NetworkInventoryItem[] GetInventoryData(CharacterBase character)
    {
        var persister = character.InventoryBase.Persister;
        if (!persister || !persister.Check(InventoryBase.PERSISTENCE_SUB_KEY))
            return new NetworkInventoryItem[0];

        var data = persister.Get<ListedInventory.ListedInventoryData>(InventoryBase.PERSISTENCE_SUB_KEY);

        var items = new NetworkInventoryItem[data.Items.Length];
        for (int i = 0; i < data.Items.Length; i++)
        {
            var item = data.Items[i];
            items[i] = new NetworkInventoryItem()
            {
                Key = item.Key,
                Quantity = item.Quantity,
                Slot = item.Slot,
            };
        }

        return items;
    }
    public static ListedInventory.ListedInventoryData GetInventoryData(NetworkInventoryItem[] items)
    {
        return new ListedInventory.ListedInventoryData()
        {
            Items = items.Select(i => new InventoryItem.InventoryItemData()
            {
                Key = i.Key,
                Quantity = i.Quantity,
                Slot = i.Slot,
            }).ToArray()
        };
    }
    public struct NetworkInventoryItem : INetworkSerializable
    {
        public string Key;
        public int Quantity;
        public string Slot;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Key);
            serializer.SerializeValue(ref Quantity);
            serializer.SerializeValue(ref Slot);
        }
    }

    public static NetworkResourceValue[] GetResourceData(CharacterBase character)
    {
        var persister = character.ResourcePool.Persister;
        if (!persister || !persister.Check(ResourcePool.PERSISTENCE_SUB_KEY))
            return new NetworkResourceValue[0];

        var data = persister.Get<ResourcePool.ResourceData>(ResourcePool.PERSISTENCE_SUB_KEY);

        var values = new NetworkResourceValue[data.Values.Length];
        for (int i = 0; i < data.Values.Length; i++)
        {
            var value = data.Values[i];
            values[i] = new NetworkResourceValue()
            {
                Key = value.Key,
                Value = value.Value,
            };
        }

        return values;
    }
    public static ResourcePool.ResourceData GetResourceData(NetworkResourceValue[] values)
    {
        return new ResourcePool.ResourceData()
        {
            Values = values.Select(i => new ResourcePool.ValueData()
            {
                Key = i.Key,
                Value = i.Value,
            }).ToArray()
        };
    }
    public struct NetworkResourceValue : INetworkSerializable
    {
        public string Key;
        public float Value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Key);
            serializer.SerializeValue(ref Value);
        }
    }

    public static NetworkAttributeValue[] GetAttributeData(CharacterBase character)
    {
        var persister = character.AttributePool.Persister;
        if (!persister || !persister.Check(AttributePool.PERSISTENCE_SUB_KEY))
            return new NetworkAttributeValue[0];

        var data = persister.Get<AttributePool.AttributeData>(AttributePool.PERSISTENCE_SUB_KEY);

        var values = new NetworkAttributeValue[data.Values.Length];
        for (int i = 0; i < data.Values.Length; i++)
        {
            var value = data.Values[i];
            values[i] = new NetworkAttributeValue()
            {
                Key = value.Key,
                Value = value.Value,
            };
        }

        return values;
    }
    public static AttributePool.AttributeData GetAttributeData(NetworkAttributeValue[] values)
    {
        return new AttributePool.AttributeData()
        {
            Values = values.Select(i => new AttributePool.ValueData()
            {
                Key = i.Key,
                Value = i.Value,
            }).ToArray()
        };
    }
    public struct NetworkAttributeValue : INetworkSerializable
    {
        public string Key;
        public int Value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Key);
            serializer.SerializeValue(ref Value);
        }
    }
}
