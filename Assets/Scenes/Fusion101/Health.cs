using UnityEngine;
using Fusion;

public class Health : NetworkBehaviour
{
    [Networked] public float amount {get;set;}

    // [Networked(OnChanged = nameof(OnDestroyed))]
    // public NetworkBool destroyed {get;set;}
    // public static void OnDestroyed(Changed<Health> obj) {
    //     obj.Behaviour.die();
    // }

    public void TakeDamage(NetworkObject attacker, float dmg) {
        if(amount <= 0f) return;

        amount -= dmg;

        Debug.LogFormat($"Id: {attacker.name} causes {dmg} damage, health: {amount}");

        if (amount <= 0f) {
            Money.Transfer(Object,attacker,true);
            // destroyed = true;
            die();
        }
    }

    public void die() {
        Runner.Despawn(Object);
    }
}
