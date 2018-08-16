using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mel.Math;

namespace Mel.VoxelGen
{

    public struct ONode
    {
        public int data;
        public byte childMask;
    }

    public class OctreeProto : MonoBehaviour
    {

        //[SerializeField] int maxDepth;
        //ONode[] nodes;



        //private void Awake()
        //{
        //    nodes = new ONode[(int)Mathf.Pow(8, maxDepth)];
        //}


    }
}
