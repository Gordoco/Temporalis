using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class FoliagePlacer : MonoBehaviour
{
    [SerializeField] private bool bRegenerate = false;
    [SerializeField] GameObject[] ValidPlacementObjects;
    [SerializeField] GameObject FoliagePrefab;
    [SerializeField] float PlacementDensity = 1;

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

    private void PlaceFoliageOnGround(Transform parent, BoxCollider bounds)
    {
        Vector3 center = bounds.bounds.center;
        Vector3 extents = bounds.bounds.extents;

        Vector3 randLocation = new Vector3(
            Random.Range(center.x - extents.x, center.x + extents.x),
            99999,
            Random.Range(center.z - extents.z, center.z + extents.z)
            );
        RaycastHit hit;
        Physics.Raycast(randLocation, Vector3.down, out hit, Mathf.Infinity);

        int MAX_ATTEMPTS = 100; //Safety to prevent accidental infinite loops
        while (!ValidPlacementObjects.Contains(hit.transform.gameObject) && MAX_ATTEMPTS > 0)
        {
            MAX_ATTEMPTS--;
            randLocation = new Vector3(
            Random.Range(center.x - extents.x, center.x + extents.x),
            99999,
            Random.Range(center.z - extents.z, center.z + extents.z)
            );
            Physics.Raycast(randLocation, Vector3.down, out hit, Mathf.Infinity);
        }

        Debug.Log(hit.transform.gameObject);
        GameObject newFoliage = Instantiate(FoliagePrefab, new Vector3(randLocation.x, hit.point.y, randLocation.z), Quaternion.Euler(0, Random.Range(0, 3), 0));
        newFoliage.transform.parent = parent;
    }
}
