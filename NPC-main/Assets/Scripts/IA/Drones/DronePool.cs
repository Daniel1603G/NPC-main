using System.Collections.Generic;
using UnityEngine;


public class DronePool : MonoBehaviour
{
    public static DronePool Instance { get; private set; }
    
    [Header("Pool Settings")]
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private bool allowGrowth = true;
    
    private Queue<GameObject> availableDrones = new Queue<GameObject>();
    private List<GameObject> allDrones = new List<GameObject>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializePool();
    }
    

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewDrone();
        }
        
        Debug.Log($"Drone pool initialized with {initialPoolSize} drones");
    }
    
    
    private GameObject CreateNewDrone()
    {
        GameObject drone = Instantiate(dronePrefab, transform);
        drone.SetActive(false);
        allDrones.Add(drone);
        availableDrones.Enqueue(drone);
        return drone;
    }
    
  
    public GameObject GetDrone(Vector3 position, Quaternion rotation)
    {
        GameObject drone;
        
        if (availableDrones.Count > 0)
        {
            drone = availableDrones.Dequeue();
        }
        else if (allowGrowth)
        {
            drone = CreateNewDrone();
            Debug.Log($"Pool expanded. Total drones: {allDrones.Count}");
        }
        else
        {
            Debug.LogWarning("Drone pool exhausted and growth not allowed!");
            return null;
        }
        
       
        drone.transform.position = position;
        drone.transform.rotation = rotation;
        drone.SetActive(true);
  
        var droneAI = drone.GetComponent<DroneAI>();
        if (droneAI != null)
        {
            droneAI.ResetDrone();
        }
        
        return drone;
    }
    
    
    public void ReturnDrone(GameObject drone)
    {
        if (drone == null) return;
        
        drone.SetActive(false);
        
        if (!availableDrones.Contains(drone))
        {
            availableDrones.Enqueue(drone);
        }
    }
    
  
    public void ReturnAllDrones()
    {
        foreach (var drone in allDrones)
        {
            if (drone.activeInHierarchy)
            {
                ReturnDrone(drone);
            }
        }
    }
}