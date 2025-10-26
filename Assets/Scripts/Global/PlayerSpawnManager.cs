// ==================== FILE 2: PlayerSpawnManager.cs ====================
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Player Prefab & Spawn")]
    [Tooltip("Prefab Third Person Controller (Starter Assets)")]
    public GameObject playerPrefab;

    [Tooltip("Default spawn point (Empty in scene)")]
    public Transform defaultSpawnPoint;

    [Tooltip("Optional: multiple spawn points; if available, random selection will be made.")]
    public Transform[] optionalSpawnPoints;

    [Header("Options")]
    [Tooltip("Hold player while changing scene (DontDestroyOnLoad)")]
    public bool persistPlayerAcrossScenes = false;

    [Header("Debug")]
    [Tooltip("Show detailed log")]
    public bool debugMode = true;

    private GameObject _currentPlayerInstance;

    private void Start()
    {
        Invoke(nameof(SpawnOrMovePlayer), 0.1f);
    }

    private void SpawnOrMovePlayer()
    {

        if (!IsPlayerLoggedIn())
        {
            LogDebug("Not logged in. Skip spawning.", true);
            return;
        }


        _currentPlayerInstance = FindExistingPlayer();


        Transform spawnTransform = GetSpawnTransform();

        if (spawnTransform == null)
        {
            Debug.LogError("[PlayerSpawnManager] No spawn transform found! Check defaultSpawnPoint.");
            return;
        }

        if (_currentPlayerInstance != null)
        {

            MovePlayerToSpawn(_currentPlayerInstance, spawnTransform);
            LogDebug($"Moved existing player to {spawnTransform.position}");
        }
        else
        {

            _currentPlayerInstance = SpawnNewPlayer(spawnTransform);
            LogDebug($"Spawned new player at {spawnTransform.position}");
        }


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


        string expectedName = $"Player_{GlobalGameState.Instance.PlayFabId}";
        GameObject player = GameObject.Find(expectedName);

        if (player != null)
        {
            LogDebug($"Found existing player: {expectedName}");
            return player;
        }




        try

        {

            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");

            if (taggedPlayer != null)

            {



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



            LogDebug("No 'Player' tag found in project");

        }



        LogDebug("No existing player found");

        return null;
    }

    private Transform GetSpawnTransform()
    {
        LogDebug($"Getting spawn transform. NextSpawnId = '{GlobalGameState.Instance?.NextSpawnId}'");


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


        if (optionalSpawnPoints != null && optionalSpawnPoints.Length > 0)
        {
            Transform randomSpawn = optionalSpawnPoints[Random.Range(0, optionalSpawnPoints.Length)];
            if (randomSpawn != null)
            {
                LogDebug($"Using random optional spawn point: {randomSpawn.name}");
                return randomSpawn;
            }
        }


        if (defaultSpawnPoint != null)
        {
            LogDebug($"Using default spawn point: {defaultSpawnPoint.name}");
            return defaultSpawnPoint;
        }


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


        GameObject player = Instantiate(playerPrefab, spawnTransform.position, spawnTransform.rotation);
        player.name = $"Player_{GlobalGameState.Instance.PlayFabId}";


        if (string.IsNullOrEmpty(player.tag))
        {
            player.tag = "Player";
        }

        if (persistPlayerAcrossScenes)
        {
            DontDestroyOnLoad(player);
        }

        LogDebug($"Created new player: {player.name} at position: {player.transform.position}");


        LogDebug($"Player hierarchy: Root={player.name}, Children={player.transform.childCount}");

        return player;
    }

    private void MovePlayerToSpawn(GameObject player, Transform spawnTransform)
    {
        if (player == null || spawnTransform == null) return;


        var characterController = player.GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = player.GetComponentInChildren<CharacterController>();
        }


        if (characterController != null)
        {
            characterController.enabled = false;
        }


        Transform rootTransform = player.transform;
        rootTransform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);

        // Re-enable character controller
        if (characterController != null)
        {
            characterController.enabled = true;
        }


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