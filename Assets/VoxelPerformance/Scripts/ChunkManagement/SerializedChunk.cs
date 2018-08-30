using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Mel.Math;
using VoxelPerformance;
using Mel.VoxelGen;
using Unity.Jobs;
using Unity.Collections;
using Mel.JobCallback;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mel.ChunkManagement
{
    public class SerializedChunk
    {
        public uint[] Voxels;
        public IntVector3 ChunkPos;
        public MapDisplay.LODBuffers DBuffers;
        private Chunk.MetaData metaData;

        public string FileFullPath { get { return GetFileFullPath(ChunkPos); } }

        public static string GetDBufferFullPath(IntVector3 ChunkPos) { return GetFileFullPath(ChunkPos) + ".DISBUFF"; }

        public static string GetFileName(IntVector3 ChunkPos) { return string.Format("SerChunk{0}_{1}_{2}.txt", ChunkPos.x, ChunkPos.y, ChunkPos.z); }

        public static string GetFileFullPath(IntVector3 ChunkPos) {  return Application.dataPath + "/SerData~/" + GetFileName(ChunkPos); }

        public static string GetMetaDataFullPath(IntVector3 ChunkPos) { return GetFileFullPath(ChunkPos) + ".METADATA.txt"; }

        public SerializedChunk(IntVector3 ChunkPos, MapDisplay.LODBuffers DBuffers, uint[] Voxels, Chunk.MetaData metaData )
        {
            this.ChunkPos = ChunkPos;
            this.Voxels = Voxels;
            this.DBuffers = DBuffers;
            this.metaData = metaData;
        }

        public void Write()
        {
            //Write dbuffer
            SerializedDisplayBuffers.WriteBuffer(DBuffers, GetDBufferFullPath(ChunkPos));

            //Write meta data
            XMLOp.Serialize(metaData, GetMetaDataFullPath(ChunkPos));

            //Write chunk
            WriteUintsToFile(Voxels, FileFullPath);
 
            
        }

        public static void WriteVoxelGeomDataToFile(VoxelGeomDataMirror[] data, string FilePath)
        {
            using (BinaryWriter w = new BinaryWriter(File.Open(FilePath, FileMode.Create)))
            {
                int len = data.Length;
                int i;
                for (i = 0; i < len; ++i)
                {
                    w.Write(data[i].voxel);
                    w.Write(data[i].extras);
                }
            }
        }

        public static void WriteUintsToFile(uint[] uints, string FilePath)
        {
            using (BinaryWriter w = new BinaryWriter(File.Open(FilePath, FileMode.Create)))
            {
                int len = uints.Length;
                int i;
                for (i = 0; i < len; ++i)
                {
                    w.Write(uints[i]);
                }
            }
        }

        public static uint[] ReadUintsFromFile(string FilePath)
        {
            FileStream file = File.Open(FilePath, FileMode.Open);
            uint[] voxels;

            using (BinaryReader br = new BinaryReader(file))
            {
                int pos = 0;
                int length = (int)br.BaseStream.Length;
                int sizeUint = sizeof(uint);
                voxels = new uint[length / sizeUint];

                while (pos * sizeUint < length)
                {
                    voxels[pos++] = br.ReadUInt32();
                }
            }

            file.Close();

            return voxels;
        }

        public static VoxelGeomDataMirror[] ReadGeomDataFromFile(string FilePath)
        {
            FileStream file = File.Open(FilePath, FileMode.Open);
            VoxelGeomDataMirror[] voxels;

            using (BinaryReader br = new BinaryReader(file))
            {
                int pos = 0;
                int length = (int)br.BaseStream.Length;
                int sizeUint = System.Runtime.InteropServices.Marshal.SizeOf(new VoxelGeomDataMirror());
                voxels = new VoxelGeomDataMirror[length / sizeUint];

                while (pos * sizeUint < length)
                {
                    VoxelGeomDataMirror vg = new VoxelGeomDataMirror();
                    vg.voxel = br.ReadUInt32();
                    vg.extras = br.ReadUInt32();
                    voxels[pos++] = vg;
                }
            }

            file.Close();

            return voxels;
        }

        public static int VoxelStorageSize {
            get {
                return sizeof(uint);
            }
        }

        public Chunk ToChunk(MapDisplay display)
        {
            var chunk = new Chunk();
            display.initialize(DBuffers);
            chunk.Init(ChunkPos, display, metaData);
            return chunk;
        }


        public static void ReadAsync(IntVector3 chunkPos, Action<SerializedChunk> callback)
        {
            /*
            if (!File.Exists(GetFileFullPath(chunkPos)))
            {
                Debug.Log("No file at" + chunkPos);
                callback(null);
                return;
            }
            */
            if (!Chunk.MetaData.SavedChunkExists(chunkPos))
            {
                callback(null);
                return;
            }

            GameObject jcbGO = new GameObject();
            SerializedDisplayBuffers.DeserJob dsbuffJob = default(SerializedDisplayBuffers.DeserJob);
            MapDisplay.LODBuffers dbuffers = null;
            try
            {
                Chunk.MetaData metaData = new Chunk.MetaData() { Dirty = true };
                metaData = XMLOp.Deserialize<Chunk.MetaData>(GetMetaDataFullPath(chunkPos));

                if(metaData.isInvalid)
                {
                    Debug.Log("invalid meta data at: " + chunkPos);
                    callback(null);
                    return;
                }

                dsbuffJob = new SerializedDisplayBuffers.DeserJob();

                var all = Allocator.TempJob;
                dsbuffJob.path = new NativeArray<byte>(StringToBytes.ToBytes(GetDBufferFullPath(chunkPos)), all);
                dsbuffJob.rLOD0 = new NativeArray<VoxelGeomDataMirror>(metaData.LODBufferLengths[0], all);
                dsbuffJob.rLOD1 = new NativeArray<VoxelGeomDataMirror>(metaData.LODBufferLengths[1], all);
                dsbuffJob.rLOD2 = new NativeArray<VoxelGeomDataMirror>(metaData.LODBufferLengths[2], all);

                var jcb = jcbGO.AddComponent<JobCall>(); 
                jcb.Schedule(dsbuffJob, () =>
                {
                    dbuffers = new MapDisplay.LODBuffers();
                    dbuffers[0] = CVoxelMapFormat.BufferCountArgs.CreateBuffer(dsbuffJob.rLOD0);
                    dbuffers[1] = CVoxelMapFormat.BufferCountArgs.CreateBuffer(dsbuffJob.rLOD1);
                    dbuffers[2] = CVoxelMapFormat.BufferCountArgs.CreateBuffer(dsbuffJob.rLOD2);

                    dsbuffJob.DisposeNArrays();


                    uint[] voxelsFakeEmpty = new uint[1];
                    callback(new SerializedChunk(chunkPos, dbuffers, voxelsFakeEmpty, metaData));

                    /*
                    FileStream chunkFile = File.Open(GetFileFullPath(chunkPos), FileMode.Open);
                    //
                    // Maybe read the full voxel array later, as needed
                    //
                    using (BinaryReader br = new BinaryReader(chunkFile))
                    {
                        int pos = 0;
                        int length = (int)br.BaseStream.Length;
                        uint[] voxels = new uint[length / VoxelStorageSize];
                        while (pos * VoxelStorageSize < length)
                        {
                            voxels[pos++] = br.ReadUInt32();
                        }
                        callback(new SerializedChunk(chunkPos, dbuffers, voxels, metaData));
                    }
                    */
                    GameObject.Destroy(jcbGO);

                });


            }
            catch (Exception e)
            {
                Debug.LogWarning("Excptn while reading: " + e.ToString());
                dsbuffJob.DisposeNArrays();
                if(dbuffers != null) { dbuffers.release(); }
                GameObject.Destroy(jcbGO);
                callback(null);
            }
        }

        public static SerializedChunk Read(IntVector3 chunkPos)
        {
            if(!File.Exists(GetFileFullPath(chunkPos)))
            {
                return null;
            }
            try
            {
                MapDisplay.LODBuffers dbuffers = null;
                Chunk.MetaData metaData = new Chunk.MetaData() { Dirty = true };
                metaData = XMLOp.Deserialize<Chunk.MetaData>(GetMetaDataFullPath(chunkPos));

                try
                {
                    dbuffers = SerializedDisplayBuffers.ReadBufferAt(GetDBufferFullPath(chunkPos)); // SerializedDisplayBuffers.Deserialize(GetDBufferFullPath(chunkPos)).ToDisplayBuffers();
                } catch (Exception e)
                {
                    Debug.LogWarning("Excptn while reading dbuffers " + e.ToString());
                    return null;
                }

                FileStream chunkFile = File.Open(GetFileFullPath(chunkPos), FileMode.Open);
                using (BinaryReader br = new BinaryReader(chunkFile))
                {
                    int pos = 0;
                    int length = (int)br.BaseStream.Length;
                    uint[] voxels = new uint[length / VoxelStorageSize];
                    while(pos * VoxelStorageSize < length)
                    {
                        voxels[pos++] = br.ReadUInt32(); 
                    }

                    return new SerializedChunk(chunkPos, dbuffers, voxels, metaData);
                }
            } catch(Exception e)
            {
                Debug.LogWarning("Excptn while reading: " + e.ToString());
                return null;
            }
        }

        public static void TestSerDisplayBuffers()
        {
  

            //var sdb = new SerializedDisplayBuffers();
            //sdb.FakeInit();
            //string path = Application.dataPath + "/" + GetFileName(new IntVector3(0)) + "SDB.txt";
            //sdb.Serialize(path);
            //var deserSDB = SerializedDisplayBuffers.Deserialize(path);
            //Debug.Log(deserSDB.ToString());
        }


        //public static string IntsToBinaryString(int[] ints)
        //{
        //    string result = new string(' ', ints.Length * 4);
        //    byte[] bytes = Encoding.ASCII.GetBytes(result);
        //    MemoryStream memStream = new MemoryStream(bytes);
        //    using (BinaryWriter w = new BinaryWriter(memStream))
        //    {
        //        foreach(int i in ints)
        //        {
        //            w.Write(i);
        //        }
                
        //    }
        //    throw new NotImplementedException("Broken");
        //    //return result;
        //}

        //public static uint[] BinaryStringToUints(string binString)
        //{
        //    byte[] bytes = Encoding.ASCII.GetBytes(binString);
        //    uint[] result;
        //    MemoryStream memStream = new MemoryStream(bytes);
        //    using (BinaryReader r = new BinaryReader(memStream))
        //    {
        //        int pos = 0;
        //        int uintSize = sizeof(uint);
        //        int len = (int)r.BaseStream.Length;
        //        result = new uint[len/uintSize];
        //        while(pos * uintSize < len)
        //        {
        //            result[pos++] = r.ReadUInt32();
        //        }
        //    }
        //    return result;
        //}


        //public static string IntArrayToString(int[] ints)
        //{
        //    //return IntsToBinaryString(ints);
        //    return string.Join(",", ints.Select(x => x.ToString()).ToArray());
        //}

        //public static uint[] StringToUIntArray(string values)
        //{

        //    //return BinaryStringToUints(values);

        //    string[] tokens = values.Split(',');
        //    int len = tokens.Length;
        //    uint[] convertedItems = new uint[len];
        //    int y;
        //    for(int x = 0; x < len; ++x)
        //    {
        //        y = 0;
        //        for (int i = 0; i < tokens[x].Length; ++i)
        //            y = y * 10 + (tokens[x][i] - '0');

        //        convertedItems[x] = (uint) y;
        //    }
        //    return convertedItems;
        //}

        //public static string rawRay(uint[] ints)
        //{
        //    char[] result = new char[ints.Length * 4];
        //    for(int i = 0; i < ints.Length; ++i)
        //    {
        //        for(int j = 0; j < 4; ++j)
        //        {
        //            result[i * 4 + j] = (char)((((int)ints[i]) >> (j * 4)) & 0xFF);
        //        }
        //    }
        //    return new string(result);
        //}

        //public static uint[] fromRawRay(string r)
        //{
        //    uint[] ints = new uint[r.Length / 4];
        //    for(int i=0; i<ints.Length; ++i)
        //    {
        //        for(int j = 0; j < 4; ++j)
        //        {
        //            ints[i] = (uint)((int)ints[i] | (r[i * 4 + j] << (j * 4))); 
        //        }
        //    }
        //    return ints;
        //}

        //static void testRawRays()
        //{
        //    var uints = new uint[] { 1, 2, 3, 4, 5, 6, 7 };

        //    string s = rawRay(uints);
        //    var iout = fromRawRay(s);

        //    foreach(var i in iout) { Debug.Log(i); }

        //}

#if UNITY_EDITOR
        [MenuItem("MEL/Test raw ray")]
        static void TestRR()
        {
            
        }
#endif

        public static class SerializedDisplayBuffers
        {
            public static MapDisplay.LODBuffers ReadBufferAt(string path)
            {
                var dbuffers = new MapDisplay.LODBuffers();
                for(int i = 0; i < 3; ++i)
                {
                    dbuffers[i] = CVoxelMapFormat.BufferCountArgs.CreateBuffer(ReadUintsFromFile(FilePathForBuffer(path, i)));
                }
                return dbuffers;
            }

            public static void WriteLODArrays(MapDisplay.LODArrays lodArrays, IntVector3 chunkPos)
            {
                string path = GetDBufferFullPath(chunkPos);
                for(int i = 0; i < ChunkGenData.LODLevels; ++i)
                {
                    WriteVoxelGeomDataToFile(lodArrays.lodArrays[i], FilePathForBuffer(path, i));
                }
            }

            public static void WriteBuffer(MapDisplay.LODBuffers dbuffers, string path)
            {
                for(int i = 0; i < ChunkGenData.LODLevels; ++i)
                {
                    WriteBuffAtIndex(dbuffers, path, i);
                }
            }

            static void WriteBuffAtIndex(MapDisplay.LODBuffers dbuffers, string path, int index)
            {
                WriteUintsToFile(CVoxelMapFormat.BufferCountArgs.GetData<uint>(dbuffers[index]), FilePathForBuffer(path, index));
            }

            public static string FilePathForBuffer(string basePath, int index)
            {
                return string.Format("{0}.{1}", basePath, index);
            }

            public struct DeserJob : IJob
            {
                [ReadOnly] public NativeArray<byte> path;
                //
                // TODO: convert to VoxelGeomDataMirror
                //
                public NativeArray<VoxelGeomDataMirror> rLOD0;
                public NativeArray<VoxelGeomDataMirror> rLOD1;
                public NativeArray<VoxelGeomDataMirror> rLOD2;

                public void Execute()
                {
                    var _path = StringToBytes.FromBytes(path.ToArray());
                    if(string.IsNullOrEmpty(_path)) { return; }

                    StreamReader reader = null;
                    try
                    {
                        rLOD0.CopyFrom(ReadGeomDataFromFile (FilePathForBuffer(_path, 0))); 
                        rLOD1.CopyFrom(ReadGeomDataFromFile(FilePathForBuffer(_path, 1)));
                        rLOD2.CopyFrom(ReadGeomDataFromFile(FilePathForBuffer(_path, 2)));
                    } catch (Exception)

                    {
                    } finally
                    {
                        if(reader != null)
                            reader.Close();
                    }
                }

                public void DisposeNArrays()
                {
                    if (path.IsCreated)
                        path.Dispose();
                    if (rLOD0.IsCreated)
                        rLOD0.Dispose();
                    if (rLOD1.IsCreated)
                        rLOD1.Dispose();
                    if (rLOD2.IsCreated)
                        rLOD2.Dispose();
                }
            }
        }
    }


    public class XMLOp
    {
        public static void Serialize(object item, string path)
        {
            XmlSerializer serializer = new XmlSerializer(item.GetType());
            StreamWriter writer = new StreamWriter(path);
            serializer.Serialize(writer.BaseStream, item);
            writer.Close();
        }

        public static T Deserialize<T>(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            StreamReader reader = new StreamReader(path);
            T deserialized = (T)serializer.Deserialize(reader.BaseStream);
            reader.Close();
            return deserialized;
        }
    }
}


