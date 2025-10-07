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

    private static GameObject _playerInstance;

    private void Start()
    {
        // 1) Đảm bảo đã login
        if (GlobalGameState.Instance == null || !GlobalGameState.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[PlayerSpawnManager] Not logged in. Skip spawning.");
            return;
        }

        // 2) Nếu player đã tồn tại (scene trước), không spawn lại
        if (_playerInstance != null)
        {
            Transform spawn = ResolveSpawnPoint();
            if (spawn != null)
                _playerInstance.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

            LockCursor(true);
            return;
        }
        // Nếu chưa có player (lần đầu vào MainGame) → spawn mới
        Transform firstSpawn = ResolveSpawnPoint();
        if (firstSpawn == null) firstSpawn = ChooseSpawnPoint();

        _playerInstance = Instantiate(playerPrefab, firstSpawn.position, firstSpawn.rotation);
        _playerInstance.name = $"Player_{GlobalGameState.Instance.PlayFabId}";

        if (persistPlayerAcrossScenes)
            DontDestroyOnLoad(_playerInstance);

        LockCursor(true);
    }
    private Transform ResolveSpawnPoint()
    {
        var gs = GlobalGameState.Instance;
        if (gs != null && !string.IsNullOrEmpty(gs.NextSpawnId))
        {
            // Tìm SpawnPoint trong scene có spawnId khớp
            var all = FindObjectsOfType<SpawnPoint>();
            foreach (var sp in all)
            {
                if (sp != null && sp.spawnId == gs.NextSpawnId)
                    return sp.transform;
            }
        }
        // fallback
        return defaultSpawnPoint != null ? defaultSpawnPoint : transform;
    }

    private Transform ChooseSpawnPoint()
    {
        if (optionalSpawnPoints != null && optionalSpawnPoints.Length > 0)
            return optionalSpawnPoints[Random.Range(0, optionalSpawnPoints.Length)];
        return defaultSpawnPoint != null ? defaultSpawnPoint : transform;
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    // Nếu bạn KHÔNG giữ player qua scene, xoá tham chiếu khi object bị destroy
    private void OnDestroy()
    {
        if (!persistPlayerAcrossScenes)
            _playerInstance = null;
    }

    // Gizmo để thấy điểm spawn trong editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        var sp = defaultSpawnPoint != null ? defaultSpawnPoint : transform;
        Gizmos.DrawWireSphere(sp.position, 0.25f);
    }
}
