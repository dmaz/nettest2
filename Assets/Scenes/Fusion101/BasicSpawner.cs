using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    NetworkRunner _runner;
    [SerializeField] private NetworkPrefabRef playerPrefab;
    Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
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
        Debug.Log($"Connect failed {reason} {remoteAddress.ToString()}");
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        var data = new NetworkInputData();

        if (Input.GetKey(KeyCode.W)) data.direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) data.direction += Vector3.back;
        if (Input.GetKey(KeyCode.A)) data.direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) data.direction += Vector3.right;

        input.Set(data);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        // if (!runner.IsServer) return;

        // Create a unique position for the player
        Vector3 spawnPosition = new Vector3((player.RawEncoded%runner.Config.Simulation.DefaultPlayers)*3,1,0);
        NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        Debug.LogError("Spawn.......");
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
          // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        Debug.LogError($"StartGame: mode {mode}");
        await _runner.StartGame(new StartGameArgs() {
            GameMode = mode,
            SessionName = "TestRoomMaz",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    void OnGUI() {
        if(_runner == null) {
            if(GUI.Button(new Rect(0,0,200,40), "Host")) StartGame(GameMode.Host);
            if(GUI.Button(new Rect(0,40,200,40), "Join")) StartGame(GameMode.Client);
        }
    }
}
