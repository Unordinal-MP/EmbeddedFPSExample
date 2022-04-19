using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    private Transform[] spawns;

    private void Awake()
    {
        Instance = this; //map specific

        spawns = FindSpawns();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private Transform[] FindSpawns()
    {
        List<Transform> childrenAndSelf = transform.GetComponentsInChildren<Transform>().ToList();
        childrenAndSelf.Remove(transform);
        return childrenAndSelf.ToArray();
    }

    private void OnDrawGizmos()
    {
        var theSpawns = spawns;
        if (theSpawns == null || theSpawns.Length == 0)
        {
            //true in editor
            theSpawns = FindSpawns();
        }

        var color = Color.red;
        color.a = 0.55f;
        Gizmos.color = color;

        foreach (Transform spawn in theSpawns)
        {
            Gizmos.DrawSphere(spawn.position, 1.2f);
        }
    }

    public void GetSpawnpoint(ServerPlayer player, IEnumerable<ServerPlayer> allPlayers, out Vector3 position, out Quaternion rotation)
    {
        float bestValue = -float.MaxValue;
        Transform bestSpawn = spawns[0];

        foreach (Transform spawn in spawns)
        {
            Vector3 point = spawn.position;

            float distanceToEnemy = float.MaxValue;
            foreach (ServerPlayer otherPlayer in allPlayers)
            {
                if (otherPlayer == player)
                {
                    continue;
                }

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
            spawnValue *= Random.value + 1;
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
