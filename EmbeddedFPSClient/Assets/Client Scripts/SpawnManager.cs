using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;


    public List<Spawners> spawners = new List<Spawners>();
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    void OnDestroy()
    {
        Instance = null;
    }

    [System.Serializable]
    public class Spawners
    {
        public Spawner spawner;
        public bool used;
    }

    public Transform GetUnusedTransform()
    {
        for (int i = 0; i < spawners.Count; i++)
        {
            if (!spawners[i].used)
            {
                spawners[i].used = true;
                return spawners[i].spawner.gameObject.transform;
            }
        }
        return null;
    }
    public void ReEnableSpawnPoint(Spawner sSpawner)
    {
        for (int i = 0; i < spawners.Count; i++)
        {

            if (spawners[i].spawner == sSpawner)

                spawners[i].used = false;

            break;
        }
    }

}

