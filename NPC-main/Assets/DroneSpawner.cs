using UnityEngine;

public class DroneSpawner : MonoBehaviour
{
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private int droneCount = 5;
    [SerializeField] private Vector3 spawnCenter = Vector3.zero;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 8f;
    
    private void Start()
    {
        SpawnDrones();
    }
    
    private void SpawnDrones()
    {
        for (int i = 0; i < droneCount; i++)
        {
            // Posición aleatoria en círculo
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            float randomHeight = Random.Range(minHeight, maxHeight);
            
            Vector3 spawnPosition = spawnCenter + new Vector3(
                randomCircle.x,
                randomHeight,
                randomCircle.y
            );
            
            Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
        }
    }
}