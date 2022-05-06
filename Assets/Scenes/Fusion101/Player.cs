using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [Networked] TickTimer bulletSpawnDelay {get;set;}

    [Networked(OnChanged = nameof(OnKeyC))]
    int materialIndex {get;set;}
    public Material[] materials;

    public Transform turret;
    public Transform muzzle;

    NetworkCharacterControllerPrototype cc;
    Renderer renderer;

    [Networked]
    Vector2 aimDirection {get;set;}

    void Awake() {
        cc = GetComponent<NetworkCharacterControllerPrototype>();
        renderer = GetComponentInChildren<Renderer>();

    }

    public override void Spawned() {
        Debug.LogError("player ObjectIndex: "+this.ObjectIndex);
        Debug.LogError("player Id: "+Id);
        Debug.LogError("player NetworkId: "+Object.Id);
        Debug.LogError("runner : "+Runner.FindObject(Object.Id));

    }

    public override void Render() {
        if(aimDirection.sqrMagnitude>0) {
            turret.forward = Vector3.Lerp(turret.forward, new Vector3(aimDirection.x,0,aimDirection.y), Time.deltaTime * 100f);
        }
    }

    public static void OnKeyC(Changed<Player> obj) {
        var me = obj.Behaviour;
        me.renderer.material = me.materials[me.materialIndex];
    }

    public override void FixedUpdateNetwork() {
        // Get our input struct and act accordingly. This method will only return data if we
        // have Input or State Authority - meaning on the controlling player or the server.
        if(GetInput(out NetworkInputData data)) {
            if(data.C) {
                materialIndex = (materialIndex+1) % materials.Length;
            }

            aimDirection = data.mousePosition - new Vector2(transform.position.x, transform.position.z);

            data.direction.Normalize();

            if(bulletSpawnDelay.ExpiredOrNotRunning(Runner) && data.mouse0) {
                var bulletDirection = new Vector3(aimDirection.x, 0, aimDirection.y);
                Runner.Spawn(bulletPrefab, muzzle.position, Quaternion.LookRotation(bulletDirection),
                    null,
                    (runner, o) => {o.GetComponent<Enemy>().Init();}
                );
                bulletSpawnDelay = TickTimer.CreateFromSeconds(Runner, .5f);
            }
        }
        cc.Move(data.direction);
    }



}

/*
The NetworkRunner is at the core of Fusion (and is a pre build NetworkBehaviour).
    The NetworkRunner manages the connection to the network and controls the simulation - from gathering input to merging snapshots,
    over to invoking callbacks and life-cycle methods on SimulationBehaviours.
    There is only ever one NetworkRunner in the scene on each client and server.

To perform these tasks, the NetworkRunner keeps track of all NetworkObjects, NetworkBehaviours and SimulationBehaviours.


Fusion fully supports having multiple NetworkObjects in a hierarchy
but
A single NetworkObject script on the root node of a GameObject is sufficient to control the entire hierarchy.
    At runtime the NetworkObject will find all SimulationBehaviours and NetworkBehaviours in its hierarchy.

If needed, NetworkObjects can be nested. In this case, the NetworkObject will not search into nested children and
    let child NetworkObjects track all SimulationBehaviours and NetworkBehaviours below it.
    A typical usecase would be a two player controlled tank made of a chassis and a turret where the driver
    controls the chassis' direction (movement) and the gunner controls the turret (shooting).

NetworkObjects can be placed in a scene or instantiated at runtime.
    In Fusion, call Runner.Spawn() instead of PhotonNetwork.Instantiate()

To load scenes, replace PhotonNetwork.LoadLevel() with Runner.SetActiveScene()

important classes in PUN are the MonoBehaviourPun and MonoBehaviourPunCallbacks.
    In Fusion, scripts inherit from NetworkBehaviour to write game logic and keep state.
    Scripts can inherit the SimulationBehaviour, if they will not contain game state.
    Where PUN called OnPhotonInstantiate(), Fusion uses the Spawned() callback to initialize network scripts.

All SimulationBehaviours can access their associated NetworkObject via the Object property.
    Simulation is Fusion's way of updating the network state. Any behaviours which are part of or affect the
    simulation state have to derive from SimulationBehaviour instead of MonoBehaviour
If the behaviour requires access to [Networked] properties, it has to derive from NetworkBehaviour instead.

Fusion synchronizes plain C# properties instead:
    As part of a NetworkBehaviour, auto-implemented properties just need a [Networked] attribute
    to become part of the game state. Only the authority of an object can change the values and
    they are automatically replicated over the network, which means less potential for cheating.

    [Networked]
    public Vector3 targetPosition {get;set;}

    [Networked(OnChanged = nameof(OnColor), Default = nameof(defaultColor))]
    public Color color {get;set;}
    Color defaultColor = new Color(0,0,0,1);

    public static void OnColor(Changed<Player> obj) {
        obj.Behaviour.renderer.material.SetColor("_BaseColor",obj.Behaviour.color);
    }

The HasStateAuthority property is valid in both Shared and Server/Client modes,and will return true if:
    - Shared Mode; StateAuthority == Runner.LocalPlayer.
    - Server/Client Mode; Runner.IsServer is true.
    Use this property in scripts to determine if this is the authoritative instance of this NetworkObject.

The InputAuthority property indicates which PlayerRef's input data (INetworkInput) is returned when GetInput() is called
    in any FixedUpdateNetwork on this NetworkObject. Player Input is only available to the client with Input Authority, and
    to the State Authority. All other clients (Proxies) will return false when GetInput() is called.
The HasInputAuthority property will return true if
    the Runner.LocalPlayer == Object.InputAuthority.
    Use this in scripts to test if the NetworkRunner.LocalPlayer is the Input Authority for this NetworkObject.

NOTE: Input Authority and Fusion's INetworkInput handling are primarily meant for Server/Client Mode,
    as they are fundamental to client prediction and resimulation.
    In Shared Mode however, user inputs are consumed immediately by the State Authority - making the
    INetworkStruct input system non-essential. You may however still wish to use the Fusion input system with
    Shared Mode if you are considering switching to Server/Client Mode in the future, and want to avoid refactoring later.

NetworkMecanimAnimator
    Only apply changes to the Animator if input is available
    (which is true for StateAuthority and InputAuthority),
    and only on Forward ticks (resimulation should be ignored).
    void FixedUpdateNetwork() {
        if (GetInput(out var input && Runner.IsForward)) {
            // Apply inputs to Animator
        }
    }
    The pass-through NetworkMecanimAnimator.SetTrigger() methods should used instead of Animator.SetTrigger()




PhotonNetwork                           ->	SimulationBehaviour.Runner and SimulationBehaviour.Object
MonoBehaviourPunCallbacks               -> 	SimulationBehaviour and NetworkBehaviour
PhotonNetwork.AddCallbackTarget(this)   -> 	N/A (automatic)
PhotonNetwork.Instantiate()             -> 	Runner.Spawn()
PhotonView                              -> 	NetworkObject
IPunObservable.OnPhotonSerializeView()  -> 	C# auto-implemented properties and Input Sync
PhotonTransformView                     -> 	NetworkTransform
PhotonRigidbodyView                     -> 	NetworkRigidbody
PhotonAnimatorView                      -> 	NetworkMecanimAnimator
[PunRPC]                                ->	[Rpc]
PhotonNetwork.LoadLevel()               -> 	Runner.SetActiveScene()
N/A	                                    ->  NetworkCharacterController

*/