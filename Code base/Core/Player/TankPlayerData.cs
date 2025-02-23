using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Collections;


[System.Serializable]
public struct TankPlayerData : INetworkSerializable
{
    public string PName;
    public int PKills;

    public TankPlayerData(string name, int kills)
    {
        PName = name;
        PKills = kills;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PName);
        serializer.SerializeValue(ref PKills);
    }
}
