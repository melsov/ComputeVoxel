using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mel.Math;

namespace Mel.VoxelGen
{
    public class TestCBuffer : MonoBehaviour
    {
        ComputeBuffer cBuff;
        ComputeBuffer outBuff;

        [SerializeField] ComputeShader CSTestCBuffer;
        int kernel;

        static IntVector3 groupSize = new IntVector3(8);

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            CallKernel();
            PrintOutBuff();

        }

        private void Init()
        {
            kernel = CSTestCBuffer.FindKernel("TestCBuffer");

            cBuff = new ComputeBuffer(groupSize.Area, sizeof(uint));
            populateCBuff();
            outBuff = new ComputeBuffer(groupSize.Area, sizeof(uint));

            CSTestCBuffer.SetBuffer(kernel, "cBuff", cBuff);
            CSTestCBuffer.SetBuffer(kernel, "outBuff", outBuff);
        }

        uint[] GetTestVals()
        {
            uint[] vals = new uint[groupSize.Area];
            foreach(var vi in groupSize.IteratorXYZ)
            {
                vals[vi.index] = (uint)vi.index;
            }
            return vals;
        }

        private void populateCBuff()
        {
            uint[] vals = GetTestVals();
            cBuff.SetData(vals);
            float[] fvals = new float[vals.Length];
            for(int i=0; i<vals.Length; ++i) { fvals[i] = vals[i] * 2f; }
            CSTestCBuffer.SetFloats("farray", fvals);
        }

        public void CallKernel()
        {
            CSTestCBuffer.Dispatch(kernel, 1, 1, 1);
        }

        public uint[] GetOutBuffValues()
        {
            uint[] vals = new uint[outBuff.count];
            outBuff.GetData(vals);
            return vals;
        }

        public void PrintOutBuff()
        {
            foreach(uint ui in GetOutBuffValues())
            {
                Debug.Log(ui);
            }
        }
    }
}
