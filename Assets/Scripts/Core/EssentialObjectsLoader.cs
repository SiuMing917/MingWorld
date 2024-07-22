using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EssentialObjectsLoader : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectsPrefab;

    private void Awake()
    {
        var exisstiongObjects = FindObjectsOfType<EssentialObject>();
        if (exisstiongObjects.Length == 0)
        {
            //讓Player 出現在中心
            var spawnPos = new Vector3(0, 0, 0);
            var grid = FindObjectOfType<Grid>();
            if (grid != null)
                spawnPos = grid.transform.position;

            Instantiate(essentialObjectsPrefab, spawnPos, Quaternion.identity);
        }
    }
}
