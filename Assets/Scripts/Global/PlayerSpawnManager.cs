// ==================== FILE 2: PlayerSpawnManager.cs ====================
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Player Prefab & Spawn")]
    [Tooltip("Prefab Third Person Controller (Starter Assets)")]
    public GameObject playerPrefab;

    [Tooltip("Điểm spawn mặc định (Empty trong scene)")]
    public Transform defaultSpawnPoint;

    [Tooltip("Tùy chọn: nhiều spawn points; nếu có thì sẽ chọn ngẫu nhiên.")]
    public Transform[] optionalSpawnPoints;

    [Header("Options")]
    [Tooltip("Giữ player khi đổi scene (DontDestroyOnLoad)")]
    public bool persistPlayerAcrossScenes = false;

    [Header("Debug")]
    [Tooltip("Hiển thị log chi tiết")]
    public bool debugMode = true;

    private GameObject _currentPlayerInstance;

    private void Start()
    {
        // Đợi 1 frame để đảm bảo scene đã load xong
        Invoke(nameof(SpawnOrMovePlayer), 0.1f);
    }

    private void SpawnOrMovePlayer()
    {
        // 1) Kiểm tra đã login chưa
        if (!IsPlayerLoggedIn())
        {
            LogDebug("Not logged in. Skip spawning.", true);
            return;
        }

        // 2) Tìm player hiện tại
        _currentPlayerInstance = FindExistingPlayer();

        // 3) Lấy spawn transform
        Transform spawnTransform = GetSpawnTransform();

        if (spawnTransform == null)
        {
            Debug.LogError("[PlayerSpawnManager] No spawn transform found! Check defaultSpawnPoint.");
            return;
        }

        if (_currentPlayerInstance != null)
        {
            // Player đã tồn tại, chỉ di chuyển
            MovePlayerToSpawn(_currentPlayerInstance, spawnTransform);
            LogDebug($"Moved existing player to {spawnTransform.position}");
        }
        else
        {
            // Chưa có player, spawn mới
            _currentPlayerInstance = SpawnNewPlayer(spawnTransform);
            LogDebug($"Spawned new player at {spawnTransform.position}");
        }

        // Clear NextSpawnId sau khi đã sử dụng
        if (GlobalGameState.Instance != null)
        {
            GlobalGameState.Instance.ClearNextSpawnId();
        }

        LockCursor(true);
    }

    private bool IsPlayerLoggedIn()
    {
        return GlobalGameState.Instance != null && GlobalGameState.Instance.IsLoggedIn;
    }

    private GameObject FindExistingPlayer()
    {
        if (GlobalGameState.Instance == null) return null;

        // Tìm player theo tên
        string expectedName = $"Player_{GlobalGameState.Instance.PlayFabId}";
        GameObject player = GameObject.Find(expectedName);

        if (player != null)
        {
            LogDebug($"Found existing player: {expectedName}");
            return player;
        }


        // Fallback: tìm theo tag - NHƯNG không trả về PlayerSpawnManager

        try

        {

            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");

            if (taggedPlayer != null)

            {

                // Kiểm tra KHÔNG phải là chính object này

                if (taggedPlayer != gameObject && !taggedPlayer.GetComponent<PlayerSpawnManager>())

                {

                    LogDebug($"Found player by tag: {taggedPlayer.name}");

                    return taggedPlayer;

                }

                else

                {

                    LogDebug($"Found tag 'Player' but it's the PlayerSpawnManager itself, ignoring");

                }

            }

        }

        catch (UnityException)

        {

            // Tag "Player" không tồn tại

            LogDebug("No 'Player' tag found in project");

        }



        LogDebug("No existing player found");

        return null;
    }

    private Transform GetSpawnTransform()
    {
        LogDebug($"Getting spawn transform. NextSpawnId = '{GlobalGameState.Instance?.NextSpawnId}'");

        // 1) Ưu tiên NextSpawnId nếu có
        if (GlobalGameState.Instance != null && !string.IsNullOrEmpty(GlobalGameState.Instance.NextSpawnId))
        {
            Transform namedSpawn = ResolveNamedSpawnPoint(GlobalGameState.Instance.NextSpawnId);
            if (namedSpawn != null)
            {
                LogDebug($"Using named spawn point: {GlobalGameState.Instance.NextSpawnId}");
                return namedSpawn;
            }
            else
            {
                LogDebug($"Named spawn point '{GlobalGameState.Instance.NextSpawnId}' not found, using fallback", true);
            }
        }

        // 2) Chọn random từ optionalSpawnPoints
        if (optionalSpawnPoints != null && optionalSpawnPoints.Length > 0)
        {
            Transform randomSpawn = optionalSpawnPoints[Random.Range(0, optionalSpawnPoints.Length)];
            if (randomSpawn != null)
            {
                LogDebug($"Using random optional spawn point: {randomSpawn.name}");
                return randomSpawn;
            }
        }

        // 3) Fallback: defaultSpawnPoint
        if (defaultSpawnPoint != null)
        {
            LogDebug($"Using default spawn point: {defaultSpawnPoint.name}");
            return defaultSpawnPoint;
        }

        // 4) Last resort: vị trí của manager này
        LogDebug("Using PlayerSpawnManager position as spawn", true);
        return transform;
    }

    private Transform ResolveNamedSpawnPoint(string spawnId)
    {
        SpawnPoint[] allSpawnPoints = FindObjectsOfType<SpawnPoint>();
        LogDebug($"Searching for spawn point '{spawnId}' among {allSpawnPoints.Length} spawn points");

        foreach (SpawnPoint sp in allSpawnPoints)
        {
            if (sp != null)
            {
                LogDebug($"  - Checking spawn point: '{sp.spawnId}' at {sp.transform.position}");

                if (sp.spawnId == spawnId)
                {
                    LogDebug($"✓ Found matching spawn point: {spawnId}");
                    return sp.transform;
                }
            }
        }

        return null;
    }

    private GameObject SpawnNewPlayer(Transform spawnTransform)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawnManager] Player prefab is not assigned!");
            return null;
        }

        // Spawn tại vị trí spawn point
        GameObject player = Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
        player.name = $"Player_{GlobalGameState.Instance.PlayFabId}";

        // Tag ROOT object để dễ tìm
        if (string.IsNullOrEmpty(player.tag))
        {
            player.tag = "Player";
        }

        if (persistPlayerAcrossScenes)
        {
            DontDestroyOnLoad(player);
        }

        LogDebug($"Created new player: {player.name} at position: {player.transform.position}");

        // Log hierarchy để debug
        LogDebug($"Player hierarchy: Root={player.name}, Children={player.transform.childCount}");

        return player;
    }

    private void MovePlayerToSpawn(GameObject player, Transform spawnTransform)
    {
        if (player == null || spawnTransform == null) return;

        // Tìm CharacterController - có thể ở root hoặc child
        var characterController = player.GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = player.GetComponentInChildren<CharacterController>();
        }

        // Disable character controller nếu có để tránh conflict
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // Set position cho ROOT object (parent cao nhất)
        Transform rootTransform = player.transform;
        rootTransform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);

        // Re-enable character controller
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        // Reset velocity nếu có Rigidbody (check cả root và children)
        var rb = player.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = player.GetComponentInChildren<Rigidbody>();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        LogDebug($"Moved player root transform to: {spawnTransform.position}");
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void LogDebug(string message, bool isWarning = false)
    {
        if (!debugMode) return;

        if (isWarning)
            Debug.LogWarning($"[PlayerSpawnManager] {message}");
        else
            Debug.Log($"[PlayerSpawnManager] {message}");
    }

    private void OnDestroy()
    {
        if (!persistPlayerAcrossScenes && _currentPlayerInstance != null)
        {
            _currentPlayerInstance = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Transform sp = defaultSpawnPoint != null ? defaultSpawnPoint : transform;
        Gizmos.DrawWireSphere(sp.position, 0.5f);
        Gizmos.DrawLine(sp.position, sp.position + sp.forward * 2f);

        // Draw optional spawn points
        if (optionalSpawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform t in optionalSpawnPoints)
            {
                if (t != null)
                {
                    Gizmos.DrawWireSphere(t.position, 0.3f);
                }
            }
        }
    }
}