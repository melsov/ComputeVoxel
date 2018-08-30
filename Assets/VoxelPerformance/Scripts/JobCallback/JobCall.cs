using UnityEngine;
using Unity.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using System.Collections;
using System.Threading.Tasks;

namespace Mel.JobCallback 
{
    public class JobCall : MonoBehaviour 
    {
        public JobHandle Schedule<J>(J job, Action onComplete = null) where J : struct, IJob
        {
            return Schedule(job, default(JobHandle), onComplete);
        }

        public JobHandle Schedule<J>(J job, JobHandle inputDeps = default(JobHandle), Action onComplete = null) where J : struct, IJob
        {
            JobHandle outDeps = job.Schedule(inputDeps);
            StartCoroutine(waitForComplete(outDeps, onComplete));
            return outDeps;
        }

        public IEnumerator ScheduleAwaitable<J>(J job, JobHandle inputeDeps = default(JobHandle)) where J : struct, IJob
        {
            JobHandle handle = job.Schedule(inputeDeps);
            int frameCount = 0;
            while (!handle.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
                if (++frameCount == 3)
                {
                    Debug.Log("hit 3 frames. calling complete so that Unity doesn't get mad");
                    break;
                }
            }
            handle.Complete();
        }

        public IEnumerator ScheduleParallelAwaitable<J>(
            J job,
            int arrayLength,
            int innerLoopBatchCount,
            Allocator allo = Allocator.Temp,
            JobHandle inputeDeps = default(JobHandle)) where J : struct, IJobParallelFor
        {
            JobHandle handle = job.Schedule(arrayLength, innerLoopBatchCount, inputeDeps);
            int frameCount = 0;
            while (!handle.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
                if (allo != Allocator.Persistent && ++frameCount == 3)
                {
                    Debug.Log("hit 3 frames. calling complete so that Unity doesn't get mad");
                    break;
                }
            }
            handle.Complete();
            yield return new object();
        }

        private IEnumerator waitForComplete(JobHandle handle, Action callback)
        {
            int frameCount = 0;
            while (!handle.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
                if(++frameCount == 3)
                {
                    Debug.Log("hit 3 frames. calling complete so that Unity doesn't get mad");
                    break;
                }
            }
            handle.Complete();
            if(callback != null)
            {
                callback();
            }
        }
    }
}
