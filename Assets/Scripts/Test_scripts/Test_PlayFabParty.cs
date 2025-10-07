using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Party;

public class PlayFabPartySetupTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Testing PlayFab setup...");
        TestPlayFabConnection();
    }

    void TestPlayFabConnection()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = System.Guid.NewGuid().ToString(),
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("✅ PlayFab Core SDK: Working!");
        Debug.Log($"Player ID: {result.PlayFabId}");

        if (PlayFabMultiplayerManager.Get() != null)
        {
            Debug.Log("✅ PlayFab Party Manager: Found!");
            TestPartyInitialization();
        }
        else
        {
            Debug.LogError("❌ PlayFab Party Manager: Not found!");
        }
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError($"❌ PlayFab login failed: {error.GenerateErrorReport()}");
    }

    void TestPartyInitialization()
    {
        PlayFabMultiplayerManager.Get().OnNetworkJoined += OnNetworkJoined;

        Debug.Log("🔄 Testing Party network creation...");
        PlayFabMultiplayerManager.Get().CreateAndJoinNetwork();
    }

    void OnNetworkJoined(object sender, string networkId)
    {
        Debug.Log($"✅ Successfully joined Party Network: {networkId}");
        Debug.Log("🎉 PlayFab Party setup hoàn tất!");

        var manager = PlayFabMultiplayerManager.Get();

        // ✅ FIXED: Sử dụng properties có sẵn
        Debug.Log($"Network ID: {manager.NetworkId}");
        Debug.Log($"Local Player: {(manager.LocalPlayer != null ? "Found" : "Not found")}");
        Debug.Log($"Remote Players Count: {(manager.RemotePlayers != null ? manager.RemotePlayers.Count : 0)}");
        Debug.Log($"Manager State: {manager.State}");
    }
}
