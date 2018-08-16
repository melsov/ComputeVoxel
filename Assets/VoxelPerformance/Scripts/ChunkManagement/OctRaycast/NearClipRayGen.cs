using Mel.VoxelGen;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearClipRayGen : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        foreach (var ray in CamGeometry.NearClipRays(Camera.main, 3))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ray.origin, ray.GetPoint(5f));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ray.GetPoint(5f), ray.GetPoint(6f));
        }
    }
}
