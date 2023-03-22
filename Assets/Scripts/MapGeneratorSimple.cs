using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneratorSimple : MapGenerator
{
    [SerializeField] private float mapSize = 20f;
    [SerializeField] private float roomSize = 0.5f;
    [SerializeField] private float doorWidth = 2.5f;
    [SerializeField] private float wallY = 1f;
    [SerializeField] private float wallThickness = 0.25f;
    [SerializeField] private float agentRadius = 0.75f;
    [SerializeField] private float movableRadius = 1.5f;

    [SerializeField] private AgentActions[] hiders = null;
    [SerializeField] private AgentActions[] seekers = null;
    [SerializeField] private BoxHolding[] movables = null;

    [SerializeField] private Transform wallsParent = null;
    [SerializeField] private GameObject wallPrefab = null;

    private const int ntries = 20;

    private List<GameObject> generatedWalls = null;

    public override void Generate()
    {
        generatedWalls?.ForEach((GameObject wall) => Destroy(wall));
        generatedWalls = new List<GameObject>();

        PlaceWalls(0f);
        PlaceWalls(-90f);

        for (int i = 0; i < ntries; i++)
        {
            if (TryPlaceObjects())
            {
                break;
            }

            if (i == ntries - 1)
            {
                Debug.LogWarning("Couldn't randomize object placement");
            }
        }
    }


    private void PlaceWalls(float rotation)
    {
        float wallSize = mapSize * roomSize;
        Vector3 roomCenter = 0.5f * (mapSize - wallSize) * new Vector3(1f, 0f, -1f);
        float wallZ = -mapSize * 0.5f + wallSize;

        float doorPosition = Random.Range(doorWidth * 0.5f, wallSize - doorWidth * 0.5f);
        if (doorPosition > doorWidth * 0.5f + 0.25f)
        {
            float wallLength = doorPosition - doorWidth * 0.5f;
            float wallX = mapSize * 0.5f - wallSize + wallLength * 0.5f;
            GameObject wall = Instantiate(wallPrefab, new Vector3(wallX, wallY, wallZ), Quaternion.identity, wallsParent);
            wall.transform.localScale = new Vector3(wallLength, wall.transform.localScale.y, wall.transform.localScale.z);
            wall.transform.RotateAround(roomCenter, Vector3.up, rotation);

            generatedWalls.Add(wall);
        }
        if (doorPosition < wallSize - doorWidth * 0.5f - 0.25f)
        {
            float wallLength = wallSize - doorPosition - doorWidth * 0.5f;
            float wallX = mapSize * 0.5f - wallLength * 0.5f;
            GameObject wall = Instantiate(wallPrefab, new Vector3(wallX, wallY, wallZ), Quaternion.identity, wallsParent);
            wall.transform.localScale = new Vector3(wallLength, wall.transform.localScale.y, wall.transform.localScale.z);
            wall.transform.RotateAround(roomCenter, Vector3.up, rotation);

            generatedWalls.Add(wall);
        }
    }

    private bool TryPlaceObjects()
    {
        List<(Vector2, float)> itemPlacement = new List<(Vector2, float)>();

        for (int i = 0; i < hiders.Length; i++)
        {
            itemPlacement.Add((PickPointRoom(0.5f * wallThickness + agentRadius), agentRadius));
        }
        for (int i = 0; i < seekers.Length; i++)
        {
            itemPlacement.Add((PickPointOutside(0.5f * wallThickness + agentRadius), agentRadius));
        }
        for (int i = 0; i < movables.Length; i++)
        {
            Vector2 placement = Random.Range(0, 2) == 0 ? PickPointRoom(0.5f * wallThickness + movableRadius)
                                                        : PickPointOutside(0.5f * wallThickness + movableRadius);
            itemPlacement.Add((placement, movableRadius));
        }

        for (int i = 0; i < itemPlacement.Count; i++)
        {
            for (int j = i + 1; j < itemPlacement.Count; j++)
            {
                if (Vector2.Distance(itemPlacement[i].Item1, itemPlacement[j].Item1) < itemPlacement[i].Item2 + itemPlacement[j].Item2)
                {
                    return false;
                }
            }
        }

        for (int i = 0; i < hiders.Length; i++)
        {
            float x = itemPlacement[i].Item1.x;
            float z = itemPlacement[i].Item1.y;
            hiders[i].transform.position = new Vector3(x, hiders[i].transform.position.y, z);
        }
        for (int i = 0; i < seekers.Length; i++)
        {
            float x = itemPlacement[i + hiders.Length].Item1.x;
            float z = itemPlacement[i + hiders.Length].Item1.y;
            seekers[i].transform.position = new Vector3(x, seekers[i].transform.position.y, z);
        }
        for (int i = 0; i < movables.Length; i++)
        {
            float x = itemPlacement[i + hiders.Length + seekers.Length].Item1.x;
            float z = itemPlacement[i + hiders.Length + seekers.Length].Item1.y;
            movables[i].transform.position = new Vector3(x, movables[i].transform.position.y, z);
        }
        return true;
    }

    private Vector2 PickPointRoom(float margin)
    {
        float u = mapSize * (0.5f - roomSize) + margin;
        float v = mapSize * 0.5f - margin;
        return PickPointRect(u, v, -v, -u);
    }

    private Vector2 PickPointOutside(float margin)
    {
        float u = mapSize * (0.5f - roomSize);
        float v = mapSize * 0.5f;
        
        float x1A = u + margin;
        float x2A = v - margin;
        float z1A = -u + margin;
        float z2A = v - margin;
        float areaA = (x2A - x1A) * (z2A - z1A);

        float x1B = -v + margin;
        float x2B = u - margin;
        float z1B = -v + margin;
        float z2B = v - margin;
        float areaB = (x2B - x1B) * (z2B - z1B);

        return Random.Range(0f, areaA + areaB) < areaA ? PickPointRect(x1A, x2A, z1A, z2A)
                                                       : PickPointRect(x1B, x2B, z1B, z2B);
    }

    private Vector2 PickPointRect(float minX, float maxX, float minZ, float maxZ)
    {
        return new Vector2(Random.Range(minX, maxX), Random.Range(minZ, maxZ));
    }
}
