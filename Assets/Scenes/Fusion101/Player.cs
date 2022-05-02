using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    NetworkCharacterController cc;

    void Awake() {
        cc = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork() {
        if(GetInput(out NetworkInputData data)) {
            data.direction.Normalize();
            // data.direction = Vector3.zero;
            cc.Move(5*data.direction * Runner.DeltaTime);
        }
        base.FixedUpdateNetwork();
    }
}
