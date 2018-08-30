using Mel.ChunkManagement;
using Mel.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mel.VoxelGen
{
    [System.Serializable]
    public struct GenAreaMetaData
    {
        public enum BuildStatus
        {
            NeverTouched, Computed, NeighborFormatted
        }

        public int _buildStatus;
        public BuildStatus buildStatus {
            get { return (BuildStatus)_buildStatus; }
            set { _buildStatus = (int)value; }
        }

        static string FilePath(IntVector3 origin)
        {
            return string.Format("{0}.{1}", SerializedChunk.GetFileFullPath(origin), "GenAreaMD");
        }

        public static GenAreaMetaData Read(IntVector3 origin)
        {
            if(!File.Exists(FilePath(origin))) { return default(GenAreaMetaData); }
            return XMLOp.Deserialize<GenAreaMetaData>(FilePath(origin));
        }

        public static void Write(GenAreaMetaData gamd, IntVector3 origin)
        {
            XMLOp.Serialize(gamd, FilePath(origin));
        }

        public void Write(IntVector3 origin)
        {
            Write(this, origin);
        }

    }
}
