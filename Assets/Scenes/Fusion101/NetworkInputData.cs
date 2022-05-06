using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public NetworkBool mouse0;
    public NetworkBool C;
    public Vector3 direction;
    public Vector2 mousePosition;
}
