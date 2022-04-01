using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public List<Transform> spawners = new List<Transform>();
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

    public void GetSpawnpoint(ServerPlayer player, IEnumerable<ServerPlayer> allPlayers, out Vector3 position, out Quaternion rotation)
    {
        float bestValue = -float.MaxValue;
        Transform bestSpawn = null;

        foreach (Transform spawn in spawners)
        {
            Vector3 point = spawn.position;

            float distanceToEnemy = float.MaxValue;
            foreach (ServerPlayer otherPlayer in allPlayers)
            {
                if (otherPlayer == player)
                    continue;

                float distance = (point - otherPlayer.transform.position).sqrMagnitude;
                if (distance < distanceToEnemy)
                {
                    distanceToEnemy = distance;
                }
            }

            distanceToEnemy = Mathf.Sqrt(distanceToEnemy);

            float distanceToPrevious = (point - player.SpawnPosition).magnitude;

            float distanceToDeath = (point - player.transform.position).magnitude;

            float spawnValue = 10 * Mathf.Min(20, distanceToPrevious) + 2 * distanceToEnemy + distanceToDeath;
            spawnValue *= Random.value - 0.5f;
            if (spawnValue > bestValue)
            {
                bestValue = spawnValue;
                bestSpawn = spawn;
            }
        }

        position = bestSpawn.position;
        rotation = Quaternion.Euler(0, Random.value * 360, 0);
    }
}

