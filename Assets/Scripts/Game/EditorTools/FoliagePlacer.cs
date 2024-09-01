using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class FoliagePlacer : MonoBehaviour
{
    // Editor values
    [SerializeField] private bool bRegenerate = false;
    [SerializeField] GameObject[] ValidPlacementObjects;
    [SerializeField] GameObject FoliagePrefab;
    [SerializeField] float PlacementDensity = 1;

    /// <summary>
    /// Method ran whenever the GameObject's parameters are changed in Editor. Only runs logic when bRegenerate is flagged.
    /// </summary>
    void OnValidate()
    {
        if (bRegenerate && ValidPlacementObjects != null && ValidPlacementObjects.Length > 0)
        {
            bRegenerate = false;
            BoxCollider bc = GetComponent<BoxCollider>();
            GameObject container = new GameObject();
            container.name = "FoliageContainer";
            int numFoliage = (int)Mathf.Round((bc.size.x * bc.size.z)/1000 * PlacementDensity);
            for (int i = 0; i < numFoliage; i++)
            {
                PlaceFoliageOnGround(container.transform, bc);
            }
        }
    }

    /// <summary>
    /// Uses a series of raycasts to dynamically and randomly place foliage on the surface of the ground within the specified bounds
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="bounds"></param>
    private void PlaceFoliageOnGround(Transform parent, BoxCollider bounds)
    {
        Vector3 center = bounds.bounds.center;
        Vector3 extents = bounds.bounds.extents;

        RaycastHit hit;
        DownwardRaycast(center, extents, out hit);

        int MAX_ATTEMPTS = 100; //Safety to prevent accidental infinite loops
        while (!ValidPlacementObjects.Contains(hit.transform.gameObject) && MAX_ATTEMPTS > 0)
        {
            MAX_ATTEMPTS--;
            DownwardRaycast(center, extents, out hit);
        }

        Debug.Log(hit.transform.gameObject);
        GameObject newFoliage = Instantiate(FoliagePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.Euler(0, Random.Range(0, 3), 0));
        newFoliage.transform.parent = parent;
    }

    /// <summary>
    /// Generates a location at a random location within the bounding box at the conceptual max height
    /// </summary>
    /// <param name="center"></param>
    /// <param name="extents"></param>
    /// <returns></returns>
    private Vector3 GenerateRandomRaycastStart(Vector3 center, Vector3 extents)
    {
        return new Vector3(
            Random.Range(center.x - extents.x, center.x + extents.x),
            float.MaxValue,
            Random.Range(center.z - extents.z, center.z + extents.z)
            );
    }

    /// <summary>
    /// Performs a raycast from the conceptual top of the world returning the first blocking hit
    /// </summary>
    /// <param name="center"></param>
    /// <param name="extents"></param>
    /// <param name="hit"></param>
    private void DownwardRaycast(Vector3 center, Vector3 extents, out RaycastHit hit)
    {
        Vector3 randLocation = GenerateRandomRaycastStart(center, extents);
        Physics.Raycast(randLocation, Vector3.down, out hit, Mathf.Infinity);
    }
}
