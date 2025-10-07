using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneTransitionPoint : MonoBehaviour
{
    [Header("Target")]
    public string targetSceneName = "EducationBuilding"; // tên scene cần load
    public string targetSpawnId = "EntranceFromMainGame"; // spawnId trong scene đích

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ phản ứng nếu player bước vào
        if (!other.CompareTag("Player")) return;

        // Lưu spawnId vào GlobalGameState để scene đích biết phải spawn ở đâu
        var gs = GlobalGameState.Instance;
        if (gs != null) gs.NextSpawnId = targetSpawnId;

        // Load scene đích
        SceneManager.LoadScene(targetSceneName);
    }
}
