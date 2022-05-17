using UnityEngine;
using Fusion;

[OrderAfter(typeof(HitboxManager))]
public class Bullet : Projectile
{
    public LayerMask hitMask;

    [Networked] TickTimer life {get;set;}
    [Networked] NetworkBool destroyed {get;set;}

    public float speed = 25;
    public NetworkObject shooter;

    public override void InitNetworkState(NetworkObject shooter) {
        life = TickTimer.CreateFromSeconds(Runner, 3f);
        this.shooter = shooter;
    }

    public override void FixedUpdateNetwork() {
        if(life.Expired(Runner) || destroyed) {
            Runner.Despawn(Object);
            return;
        }

        Vector3? hitPoint = CheckHit();
        if(hitPoint != null) {
            destroyed = true;
        } else {
            transform.position += speed * transform.forward * Runner.DeltaTime;
        }

    }

    public Vector3? CheckHit() {
        //     Performs a lag-compensated raycast query against all registered hitboxes. If
        //     the Fusion.HitOptions.IncludePhysX flag is indicated, query will also include
        //     PhysX colliders, PhysX colliders are recommended for static geometry, rather
        //     than Hitboxes. Important: results are NOT sorted by distance.
        //
        // Parameters:
        //   origin:
        //     Raycast origin, in world-space
        //
        //   direction:
        //     Raycast direction, in world-space
        //
        //   length:
        //     Raycast length
        //
        //   player:
        //     Player who "owns" this raycast. Used by the server to find the exact hitbox snapshots
        //     to check against.
        //
        //   hits:
        //     List to be filled with hits (both hitboxes and/or PhysX colliders, if included).
        //
        //   layerMask:
        //     Only objects with matching layers will be checked against.
        //
        //   options:
        //     Opt-in flags to compute with sub-tick accuracy (Fusion.HitOptions.SubtickAccuracy)
        //     and/or to include PhysX (Fusion.HitOptions.IncludePhysX).
        //
        //   clearHits:
        //     Clear list of hits before filling with new ones (defaults to true).
        //
        //   queryTriggerInteraction:
        //     Trigger interaction behavior when also querying PhysX.
        //
        // Returns:
        //     total number of hits
        var dir = transform.forward;
        // We move the origin back from the actual position to make sure we can't shoot through things even if we start inside them
        if (Runner.LagCompensation.Raycast(
            transform.position -0.5f*dir, dir,
            // Mathf.Max(_bulletSettings.radius, speed * Runner.DeltaTime), // length
            speed * Runner.DeltaTime, // length
            Object.InputAuthority,
            out var hit,
            hitMask.value,
            HitOptions.IncludePhysX
        )) {
            if(hit.Hitbox != null) {
                NetworkObject netobj = hit.Hitbox.Root.Object;
                if (netobj != null && Object!=null && netobj.InputAuthority == Object.InputAuthority) {
                    // Don't let us hit ourselves - this is esp. important with lag compensation since,
                    // if we move backwards, we're very likely to hit our own ghost from a previous frame.
                    return null;
                }
            }
            var health = hit.GameObject.GetComponent<Health>();
            if(health) health.TakeDamage(shooter,1);
            return hit.Point;
        }
        return null;
    }

}
