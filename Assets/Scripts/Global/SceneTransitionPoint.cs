// ==================== FILE 3: SceneTransitionPoint.cs ====================
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneTransitionPoint : MonoBehaviour
{
    [Header("Target")]
    public string targetSceneName = "EducationBuilding";
    public string targetSpawnId = "EntranceFromMainGame";

    [Header("Debug")]
    public bool debugMode = true;

    private bool _isTransitioning = false;

    private void Start()
    {

        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[SceneTransitionPoint] Collider on {gameObject.name} was not set as trigger. Fixed.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isTransitioning) return;


        if (!other.CompareTag("Player"))
        {
            if (debugMode)
                Debug.Log($"[SceneTransitionPoint] Non-player entered: {other.name} (tag: {other.tag})");
            return;
        }

        _isTransitioning = true;

        if (debugMode)
            Debug.Log($"[SceneTransitionPoint] Player entered! Transitioning to '{targetSceneName}' with spawn '{targetSpawnId}'");


        var gs = GlobalGameState.Instance;
        if (gs != null)
        {
            gs.NextSpawnId = targetSpawnId;
            if (debugMode)
                Debug.Log($"[SceneTransitionPoint] Set NextSpawnId to: {targetSpawnId}");
        }
        else
        {
            Debug.LogError("[SceneTransitionPoint] GlobalGameState.Instance is null!");
        }


        SceneManager.LoadScene(targetSceneName);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider box)
                Gizmos.DrawWireCube(box.center, box.size);
            else if (col is SphereCollider sphere)
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }
}