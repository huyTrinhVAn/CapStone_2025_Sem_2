using UnityEngine;
using PlayFab.ClientModels;

public class GlobalGameState : MonoBehaviour
{
    public static GlobalGameState Instance { get; private set; }

    public bool IsLoggedIn { get; private set; }
    public string PlayFabId { get; private set; }
    public LoginResult LoginResult { get; private set; }
    public string CharacterId { get; private set; } // (tùy chọn) dùng sau khi tạo nhân vật
    public string NextSpawnId;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLogin(LoginResult result)
    {
        LoginResult = result;
        PlayFabId = result.PlayFabId;
        IsLoggedIn = true;
        Debug.Log($"[GlobalGameState] Logged in as {PlayFabId}");
    }

    public void SetCharacter(string characterId)
    {
        CharacterId = characterId;
        Debug.Log($"[GlobalGameState] CharacterId set: {characterId}");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
