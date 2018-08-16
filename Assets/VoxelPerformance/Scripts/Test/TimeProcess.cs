using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Mel.VoxelGen
{
    public static class TimeProcess
    {
        public static bool Verbose = false;

        static void Log(string s)
        {
            if(Verbose) { Debug.Log(s); }
        }

        public static double GetExecutionTime(Func<long> f, bool logFinalResult = true)
        {
            var d = new System.Diagnostics.Stopwatch(); //.StartNew();

            //warm up
            long seed = Environment.TickCount;
            long result = 0;
            //int count = 100000000;
            Log("20 Test without correct preparation");
            int testCount = 20;
            double avgMillis = 0d;
            for (int rep = 0; rep < testCount; ++rep)
            {
                d.Reset();
                d.Start();
                result ^= f();
                d.Stop();
                Log("W/o prep: Elapsed Ticks: " + d.ElapsedTicks + " ms: " + d.ElapsedMilliseconds);
                avgMillis += d.ElapsedMilliseconds;
            }
            Log("Avg w/o prep: " + avgMillis / testCount);

            System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2); // uses the second core or processor for the test

            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High; // prevent 'normal'processes from interupting this thread
            Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            Log("Warm up");

            d.Reset();
            d.Start();
            while(d.ElapsedMilliseconds < 1200)
            {
                result = f();
            }
            d.Stop();

            avgMillis = 0;
            for(int rep = 0; rep < testCount; ++rep)
            {
                d.Reset();
                d.Start();
                result ^= f();
                d.Stop();
                Log("REAL: Elapsed Ticks: " + d.ElapsedTicks + " ms: " + d.ElapsedMilliseconds);
                avgMillis += d.ElapsedMilliseconds;
            }
            Log("prevent compiler from optimizing this away: " + result);
            avgMillis /= testCount;
            if (logFinalResult)
                Debug.Log("avg Millis: " + avgMillis);
            else
                Log("avg Millis: " + avgMillis);

            return avgMillis;
        }


    }
}
