using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IslandPart : MonoBehaviour {

    List<GameObject> adjacentIslandParts = new List<GameObject>();

    public List<GameObject> GetAdjacentParts()
    {
        return adjacentIslandParts;
    }

    public void AddAdjacentPart(GameObject islandpart)
    {
        adjacentIslandParts.Add(islandpart);
    }
}
