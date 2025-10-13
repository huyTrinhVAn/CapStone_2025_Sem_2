// ==================== FILE 4: SpawnPoint.cs ====================
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Unique identifier for this spawn point")]
    public string spawnId;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.5f);

        // Draw label in editor
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, spawnId);
#endif
    }
}