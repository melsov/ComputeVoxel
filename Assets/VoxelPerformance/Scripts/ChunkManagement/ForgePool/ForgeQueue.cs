using Mel.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mel.VoxelGen
{
    public class ChunkGenForgeQueue : MonoBehaviour
    {
        public enum ForgeProcessStatus
        {
            NOT_STARTED, IN_PROCESS, FINISHED
        }
        public struct ForgeJob
        {
            public IntVector3 pos;
            public ForgeProcessStatus status;
            public List<Action<ChunkGenData>> callbacks;

            public ForgeJob(IntVector3 pos, params Action<ChunkGenData>[] callback)
            {
                this.pos = pos;
                status = ForgeProcessStatus.NOT_STARTED;
                callbacks = new List<Action<ChunkGenData>>();
                callbacks.AddRange(callback);
            }

            public bool Start(ChunkForge forge)
            {
                if(forge.Busy) { return false; }
                if(status > ForgeProcessStatus.NOT_STARTED) { return false; }
                status = ForgeProcessStatus.IN_PROCESS;
                forge.ForgeChunkGenData(pos, UpperCallback);
                return true;
            }

            void UpperCallback(ChunkGenData cdata)
            {
                status = ForgeProcessStatus.FINISHED;
                foreach(var act in callbacks) { act(cdata); }
            }
        }

        Dictionary<IntVector3, ForgeJob> jobs = new Dictionary<IntVector3, ForgeJob>();

        ChunkForge _cf;
        ChunkForge forge {
            get {
                if(!_cf)
                {
                    _cf = FindObjectOfType<ChunkForge>();
                }
                return _cf;
            }
        }

        public void Enqueue(IntVector3 pos, Action<ChunkGenData> callback)
        {
            if (!jobs.ContainsKey(pos))
            {
                ForgeJob job = new ForgeJob(pos, OnComplete, callback);
                jobs.Add(pos, job);
            }
            else
            {
                var job = jobs[pos];
                job.callbacks.Add(callback);
            }
        }

        void OnComplete(ChunkGenData cdata)
        {
            jobs.Remove(cdata.chunkPos);
        }

        private void Update()
        {
            if(forge.Busy) { return; }
            if(jobs.Count == 0) { return; }
            foreach(var key in jobs.Keys)
            {
                if(jobs[key].Start(forge))
                {
                    break;
                }
            }

        }



    }
}
