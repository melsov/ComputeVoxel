using Mel.ChunkManagement;
using Mel.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mel.FakeData;
using System.Collections.Concurrent;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mel.VoxelGen
{
    public static class SerializedChunkGenData
    {

        static ConcurrentDictionary<IntVector3, object> locks = new ConcurrentDictionary<IntVector3, object>();

        #region Get-paths

        public static string GenDataFullPath(IntVector3 pos) {
            return string.Format("{0}.CGD", SerializedChunk.GetFileFullPath(pos));
        }

        static string[] GenDataPaths(string fullPathBase, int count = 3)
        {
            string[] result = new string[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = string.Format("{0}.{1}", fullPathBase, i);
            }
            return result;
        }

        #endregion

        #region tests
        public static void TestWriteCGD()
        {
            var cgd = FakeChunkData.StairsGenData(IntVector3.zero, new IntVector3(64));
            Debug.Log("aboud to write");
            DBUGFirstFew(cgd);
            WriteAsyncDebug(cgd, GenDataFullPath(cgd.chunkPos), () =>
            {
                var reCGD = Read(cgd.chunkPos);
                DBUGFirstFew(reCGD);
                bool eq = DBUGEqual(reCGD, cgd);
                Debug.Log("Are they equal (read and written)? : " + eq);
            });
            Debug.Log("This gets called on main thread");
        }

        static bool DBUGEqual(ChunkGenData a, ChunkGenData b)
        {
            for(int i = 0; i < ChunkGenData.LODLevels; ++i)
            {
                if(a[i].Length != b[i].Length) { return false; }
                for(int j = 0; j < a[i].Length; ++j)
                {
                    if(a[i][j] != b[i][j]) { return false; }
                }
            }
            return true;
        }

        static void DBUGFirstFew(ChunkGenData cgd)
        {
            StringBuilder sb = new StringBuilder();
            for(int i=0; i < 10; ++i)
            {
                sb.Append(cgd[1][i].voxel);
                sb.Append(",");
            }
            Debug.Log(sb.ToString());
        }

        #endregion

        #region Write

        public static async void WriteAsyncDebug(ChunkGenData data, string fullPath, Action OnCompleted)
        {
            Task t = _WriteAsync(data, fullPath);
            await t;
            OnCompleted();
        }

        public static async Task<IntVector3> WriteAsync(ChunkGenData data, string fullPath)
        {
            Task t = _WriteAsync(data, fullPath);
            await t;
            return data.chunkPos;
        }

        static async Task _WriteAsync(ChunkGenData dat, string fullPath)
        {
            await new WaitForBackgroundThread();
            lock (locks.GetOrAdd(dat.chunkPos, new object()))
            {
                var paths = GenDataPaths(fullPath, ChunkGenData.LODLevels);
                for(int i = 0; i < paths.Length; ++i)
                {
                    WriteGenDataToFile(dat, paths[i], i);
                }

            }
            object o;
            locks.TryRemove(dat.chunkPos, out o);
            await new WaitForUpdate(); //Is this the way????
        }

        static void WriteGenDataToFile(ChunkGenData data, string FilePath, int lodLevel)
        {
            //dbug 
            if(data[lodLevel] == null)
            {
                Debug.Log("null at: " + lodLevel);
                return;
            }

            using (BinaryWriter w = new BinaryWriter(File.Open(FilePath, FileMode.Create)))
            {
                int len = data[lodLevel].Length;
                int i;
                for (i = 0; i < len; ++i)
                {
                    w.Write(data[lodLevel][i].voxel);
                }
            }
        }

        #endregion

        #region Read

        public static ChunkGenData Read(IntVector3 chunkPos)
        {
            var data = new ChunkGenData();
            lock (locks.GetOrAdd(chunkPos, new object()))
            {
                var paths = GenDataPaths(GenDataFullPath(chunkPos), ChunkGenData.LODLevels);
                for(int i = 0; i < paths.Length; ++i)
                {
                    data[i] = ReadGenVoxelsFromFile(paths[i]);
                }
            }
            object o;
            locks.TryRemove(chunkPos, out o);
            data.chunkPos = chunkPos;
            return data;
        }


        static VoxelGenDataMirror[] ReadGenVoxelsFromFile(string FilePath)
        {
            FileStream file = File.Open(FilePath, FileMode.Open);
            VoxelGenDataMirror[] voxels;

            using (BinaryReader br = new BinaryReader(file))
            {
                int pos = 0;
                int length = (int)br.BaseStream.Length;
                int sizeUint = System.Runtime.InteropServices.Marshal.SizeOf(new VoxelGenDataMirror());
                voxels = new VoxelGenDataMirror[length / sizeUint];

                while (pos * sizeUint < length)
                {
                    var voxel = new VoxelGenDataMirror();
                    voxel.voxel = br.ReadUInt32();
                    voxels[pos++] = voxel;
                }
            }

            return voxels;
        }

        #endregion

    }
}
