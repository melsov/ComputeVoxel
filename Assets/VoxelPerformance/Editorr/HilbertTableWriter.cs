using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HilbertExtensions;
using Mel.Math;
using Mel.VoxelGen;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;

namespace Mel.Editorr
{
    [CustomEditor(typeof(HilbertTableWriter))]
    public class HTWEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var htw = (HilbertTableWriter)target;
            if(GUILayout.Button("Write to file"))
            {
                htw.Write();
            }

            if(GUILayout.Button("Chunk Dim To File"))
            {
                var vGenConfig = FindObjectOfType<VGenConfig>();
                htw.Write(new uint[] { (uint)vGenConfig.ChunkDimension });
            }
        }
    }

    public class HilbertTableWriter : MonoBehaviour
    {
        [SerializeField]
        uint[] CubeSizes = new uint[] { 4 };

        [SerializeField]
        string outFileName = "HilbertTables.cs";

        [SerializeField, TextArea]
        string CSharpClassStartString = @"
namespace Mel.VoxelGen.HilbertTable { 
\t public static class HilbertTables { 
";

        [SerializeField, TextArea]
        string CSharpClassEndString = @"\n\t} 
}";

        [SerializeField]
        string outFileNameHLSL = "HilbertTables.cginc";

        [SerializeField, TextArea]
        string StartStringHLSL = @"
#ifndef HILBERT_TABLES
#define HILBERT_TABLES
";

        [SerializeField, TextArea]
        string EndStringHLSL = @"#endif";

        string outPath {
            get {
                return string.Format("{0}/VoxelPerformance/Scripts/HilbertTables/{1}", Application.dataPath, outFileName);
            }
        }

        string outPathHLSL {
            get {
                return string.Format("{0}/VoxelPerformance/Shaders/{1}", Application.dataPath, outFileNameHLSL);
            }
        }

        public void Write() { Write(CubeSizes); }

        internal void Write(params uint[] sizes)
        {
            WriteCSharp(sizes);
            WriteHLSL(sizes);
        }

        private void WriteCSharp(uint[] sizes)
        {
            string result = CSharpClassStartString;
            foreach (uint size in sizes)
            {
                result = string.Format("{0} \n{1}", result, WriteCSharpTables(size));
            }
            result += Environment.NewLine + CSharpClassEndString;
            File.WriteAllText(outPath, result);
        }

        private void WriteHLSL(uint[] sizes)
        {
            string result = StartStringHLSL;
            foreach (uint size in sizes)
            {
                result = string.Format("{0} {1} {2}", result, Environment.NewLine, WriteHLSLTables(size));
            }
            result += Environment.NewLine + EndStringHLSL;
            File.WriteAllText(outPathHLSL, result);
        }

        private string WriteCSharpTables(uint size)
        {

            string XToHilbert = "public static uint[] XYZToHilbertIndex = new uint[]"; // {";
            XToHilbert += @"{ " + Environment.NewLine;
            string HilbertIndexToXYZ = "public static uint[] HilbertIndexToXYZ = new uint[]"; // {";
            HilbertIndexToXYZ += @"{ " + Environment.NewLine;


            return appendValues(size, XToHilbert, HilbertIndexToXYZ);// XToHilbert;
        }

        private string WriteHLSLTables(uint size)
        {
            string XToHilbert = "static const uint XYZToHilbertIndex[" + (size * size * size) + "] = ";
            XToHilbert += @"{ " + Environment.NewLine;
            string HilbertIndexToXYZ = "static const uint HilbertIndexToXYZ[" + (size * size * size) + "] = ";
            HilbertIndexToXYZ += @"{ " + Environment.NewLine;

            return appendValues(size, XToHilbert, HilbertIndexToXYZ);
        }

        private string appendValues(uint size, string XToHilbert, string HilbertIndexToXYZ)
        {
            List<IntVector3.IndexedIntVector3> xyzs = new IntVector3(size).IteratorXYZ.ToList();
            int area = xyzs.Count; // (int)(size * size * size);
            int bits = VGenConfig.GetHilbertBits(area);


            uint[] hilbertToXYZ = new uint[area];

            StringBuilder XtoHBuilder = new StringBuilder();
            StringBuilder HilToXBuilder = new StringBuilder();

            XtoHBuilder.Append(XToHilbert);
            HilToXBuilder.Append(HilbertIndexToXYZ);

            for (int i = 0; i < area; ++i)
            {
                int hindex = xyzs[i].v.ToUint3().CoordsToFlatHilbertIndex(bits);
                hilbertToXYZ[hindex] = (uint)i;

                XtoHBuilder.Append(string.Format(" {0},", hindex));
                //XToHilbert = string.Format("{0} {1},", XToHilbert, hindex);
                if (i % 16 == 15)
                {
                    XtoHBuilder.AppendLine();
                    //XToHilbert = string.Format("{0} {1}", XToHilbert, Environment.NewLine);
                }
            }

            for (int j = 0; j < hilbertToXYZ.Length; ++j)
            {
                HilToXBuilder.Append(string.Format(" {0},", hilbertToXYZ[j]));
                //HilbertIndexToXYZ = string.Format("{0} {1},", HilbertIndexToXYZ, hilbertToXYZ[j]);
                if (j % 16 == 15)
                {
                    HilToXBuilder.AppendLine();
                    //HilbertIndexToXYZ = string.Format("{0} {1}", HilbertIndexToXYZ, Environment.NewLine);
                }
            }

            XtoHBuilder.Append("};");
            //XToHilbert += "};";
            HilToXBuilder.Append("};");
            //HilbertIndexToXYZ += "};"; 

            XtoHBuilder.AppendLine();
            XtoHBuilder.Append(HilToXBuilder);
            return XtoHBuilder.ToString();
            //XToHilbert += "\n\n" + HilbertIndexToXYZ;
            //return XToHilbert;
        }

        //TODO: test by showing the the reverse lookup table reverses
    }
}

#endif
