using UnityEngine;
using Fusion;

public class EnemySpawner : NetworkBehaviour
{
    public Enemy enemyPrefab;
    int tick;
    public override void FixedUpdateNetwork()
    {
        if(!Object.HasStateAuthority) return;

        tick++;
        if(tick % 500 == 0) {
            Runner.Spawn(
                enemyPrefab,
                new Vector3(Random.Range(-15,15), .5f, Random.Range(-15,15)),
                Quaternion.identity,
                null
            );
        }
    }
}
