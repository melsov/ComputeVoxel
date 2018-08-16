using UnityEngine;
//using Unity.Jobs;
//using System;
using System.Collections.Generic;
using g3;
using Mel.Math;
//using Unity.Collections;

namespace Mel.VoxelGen
{
    public struct FlatBounds3
    {
        Vector3 min, max;
    }

    public static class CamGeometry
    {


        public static Vector3 NearClipLowerLeftCorner(Camera cam)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);
            var lowerLeftDir = Vector3.Cross(planes[0].normal, planes[2].normal);
            float mag = (Vector3.one * cam.nearClipPlane).DivideBy(lowerLeftDir).MinAbs();
            return cam.transform.position + lowerLeftDir * mag;
        }

        public static Vector3 NearClipUpperRightCorner(Camera cam)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);
            var upperRightDir = Vector3.Cross(planes[1].normal, planes[3].normal);
            float mag = (Vector3.one * cam.nearClipPlane).DivideBy(upperRightDir).MinAbs();
            return cam.transform.position + upperRightDir * mag;
        }

        public static IEnumerable<Ray> NearClipRays(Camera cam, int yDivisions = 10)
        {
            int xDivisions = (int)(cam.aspect * yDivisions);
            // left, right, down, up, near, far
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);

            var lowerLeft = Vector3.Cross(planes[0].normal, planes[2].normal);
            // var offset = (Vector3.one * cam.nearClipPlane).DivideBy(lowerLeft).MinAbs();

            var upperRight = Vector3.Cross(planes[1].normal, planes[3].normal);


            var upperLeft = lowerLeft;
            upperLeft.y = upperRight.y;

            //yield return new Ray(cam.transform.position + upperLeft * offset, upperLeft);

            var lowerRight = upperRight;
            lowerRight.y = lowerLeft.y;

            //yield return new Ray(cam.transform.position + lowerRight * offset, lowerRight);

            Vector3 rowLeft, rowRight;
            for(int y = 0; y < yDivisions; ++y)
            {
                rowLeft = Vector3.Lerp(lowerLeft, upperLeft, y / (float)yDivisions);
                rowRight = Vector3.Lerp(lowerRight, upperRight, y / (float)yDivisions);

                for(int x = 0; x < xDivisions; ++x)
                {
                    var dir = Vector3.Lerp(rowLeft, rowRight, x / (float)xDivisions);
                    float mag = (Vector3.one * cam.nearClipPlane).DivideBy(dir).MinAbs();
                    yield return new Ray(cam.transform.position + dir * mag, dir);
                }
            }



            //var dims = new Vector3f(1f, 1f, cam.nearClipPlane + 3f);

            //var start = new Vector2f(-1f, -1f);
            //var camMatrix = cam.cameraToWorldMatrix;
            //Vector3 o;
            //Vector3 d = camMatrix.MultiplyPoint(Vector3.forward);


            //foreach(var near in dims.IteratorXY(new Vector2f(1f/(float)xDivisions, 1f/(float)yDivisions), start))
            //{
            //    o = camMatrix.MultiplyPoint(near);
            //    yield return new Ray(o, d);
            //}

        }

    }

}
