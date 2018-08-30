using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Mel.Util
{
    public class ExecuteBat : MonoBehaviour
    {
        //[MenuItem("MEL/Try Some bats")]
        //static void TrySomeBats()
        //{
        //    UnityEngine.Debug.Log("Nothing ");
        //    ExecuteCommand("nothing");

        //    UnityEngine.Debug.Log("echo testing");
        //    ExecuteCommand("echo testing");
        //}

        static void ExecuteCommand(string command)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            UnityEngine.Debug.Log("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            UnityEngine.Debug.Log("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            UnityEngine.Debug.Log("ExitCode: " + exitCode.ToString() +  "ExecuteCommand");
            process.Close();
        }
    }
}
#endif
