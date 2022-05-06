using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private LayerMask mouseLayerMask;

    NetworkRunner _runner;

    Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    void OnGUI() {
        if(_runner == null) {
            if(GUI.Button(new Rect(0,0,200,40), "Auto")) StartGame(GameMode.AutoHostOrClient);
            if(GUI.Button(new Rect(0,40,200,40), "Host")) StartGame(GameMode.Host);
            if(GUI.Button(new Rect(0,80,200,40), "Join")) StartGame(GameMode.Client);
            if(GUI.Button(new Rect(0,120,200,40), "Shared")) StartGame(GameMode.Shared);
            if(GUI.Button(new Rect(0,160,200,40), "Single")) StartGame(GameMode.Single);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W)) data.direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) data.direction += Vector3.back;
        if (Input.GetKey(KeyCode.A)) data.direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) data.direction += Vector3.right;

        data.mousePosition = GetLookDirection();

        data.mouse0 = Input.GetMouseButton(0);
        data.C = Input.GetKey(KeyCode.C);

        input.Set(data);
    }

    Vector3 GetLookDirection() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector3 mouseCollisionPoint = Vector3.zero;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mouseLayerMask)) {
            if (hit.collider) mouseCollisionPoint = hit.point;
        }

        return new Vector2(mouseCollisionPoint.x,mouseCollisionPoint.z);
    }


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        if (!runner.IsServer) return;

        // Create a unique position for the player
        Vector3 spawnPosition = new Vector3((player.RawEncoded%runner.Config.Simulation.DefaultPlayers)*3, .6f, 0);
        NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        Debug.LogError("Spawn......."+player);
        // Keep track of the player avatars so we can remove it when they disconnect
        spawnedCharacters.Add(player, networkPlayerObject);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        // Find and remove the players avatar
        if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject)) {
            runner.Despawn(networkObject);
            spawnedCharacters.Remove(player);
        }
    }


    async void StartGame(GameMode mode) {
        string roomId = SystemInfo.deviceUniqueIdentifier;
        roomId = "aaa";
        Debug.Log("uid: "+roomId);
          // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        Debug.LogError($"StartGame: mode {mode}");
        await _runner.StartGame(new StartGameArgs() {
            GameMode = mode,
            SessionName = roomId,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnConnectedToServer(NetworkRunner runner) {
        Debug.Log("Connected to server");
        if(runner.Topology == SimulationConfig.Topologies.Shared) Debug.Log("Shared mode");
        else Debug.Log("ClientServer mode");
    }
    public void OnDisconnectedFromServer(NetworkRunner runner) {
        Debug.Log("Disconnected from server");
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
        Debug.Log("requst accept()");
        request.Accept();
    }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
        Debug.Log($"Connect failed {reason} {remoteAddress}");
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }


}
