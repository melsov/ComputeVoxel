using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mel.VoxelGen;
using Mel.Math;
using System.Linq;
using VoxelPerformance;
using UnityEngine.Assertions;

public static class DebugChunkData {

    static VGenConfig _vGenConfig;
    static VGenConfig vGenConfig {
        get {
            if (!_vGenConfig)
            {
                _vGenConfig = GameObject.FindObjectOfType<VGenConfig>();
            }
            return _vGenConfig;
        }
    }

    public static void CheckUniqueData(ComputeBuffer buff, string msg = "unique ? ")
    {
        var data = CVoxelMapFormat.BufferCountArgs.GetData<uint>(buff);
        //var vecs = new List<IntVector3>(data.Length);
        //foreach(var vox in data)
        //{
        //    vecs.Add(IntVector3.FromUint256((int)vox));
        //}
        var unique = data.Distinct().Count();
        Debug.Log(string.Format("{0} unique? {1} data: {2} unique {3} | args count dif: {4}",
            msg,
            data.Length == unique,
            data.Length, 
            unique,
            CVoxelMapFormat.BufferCountArgs.DebugArgsCountVsCount(buff)));

    }

    public static void CheckAllSamePositionOnAxis(ComputeBuffer buff, int axis, string msg = " same axis? ")
    {
        var data = CVoxelMapFormat.BufferCountArgs.GetData<int>(buff);

        var axCoords = new List<int>(data.Length);
        foreach(var vox in data)
        {
            axCoords.Add(IntVector3.FromUint256(vox)[axis]);
        }
        var unique = axCoords.Distinct().Count();
        Debug.Log(string.Format("{0} unique axis {1}? {2} data: {3} unique {4}",
            msg,
            axis,
            data.Length == unique,
            data.Length, 
            unique));

    }

}
