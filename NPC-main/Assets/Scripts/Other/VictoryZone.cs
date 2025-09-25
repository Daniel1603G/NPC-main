using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WinFloor : MonoBehaviour
{
    [SerializeField] private string victorySceneName = "EscenaVictoria";
    [SerializeField] private bool oneShot = true;

    private bool used;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (oneShot && used) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        SceneManager.LoadScene(victorySceneName);
    }
}
