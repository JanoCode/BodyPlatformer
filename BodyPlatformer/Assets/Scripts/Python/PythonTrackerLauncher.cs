using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonTrackerLauncher : MonoBehaviour
{
    private Process trackerProcess;

    private void Start()
    {
        StartTracker();
    }

    private void StartTracker()
    {
        // Application.dataPath apunta a la carpeta Assets
        // dentro del proyecto de Unity en el Editor.
        string unityProjectPath =
            Directory.GetParent(Application.dataPath).FullName;

        string rootPath =
            Directory.GetParent(unityProjectPath).FullName;

        string trackerFolder =
            Path.Combine(
                rootPath,
                "BodyPlatformerTracking",
                "dist"
            );

        string trackerExecutable =
            Path.Combine(
                trackerFolder,
                "pose_tracking.exe"
            );

        if (!File.Exists(trackerExecutable))
        {
            UnityEngine.Debug.LogError(
                "No se encontró pose_tracking.exe en: " +
                trackerExecutable
            );

            return;
        }

        ProcessStartInfo startInfo =
            new ProcessStartInfo
            {
                FileName = trackerExecutable,

                // Muy importante porque pose_tracking.exe
                // busca pose_landmarker_lite.task
                // usando una ruta relativa.
                WorkingDirectory = trackerFolder,

                UseShellExecute = false,

                CreateNoWindow = false
            };

        trackerProcess = new Process();
        trackerProcess.StartInfo = startInfo;

        try
        {
            trackerProcess.Start();

            UnityEngine.Debug.Log(
                "Body Tracker iniciado automáticamente."
            );
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError(
                "No se pudo iniciar Body Tracker: " +
                e.Message
            );
        }
    }

    private void OnApplicationQuit()
    {
        StopTracker();
    }

    private void OnDestroy()
    {
        StopTracker();
    }

    private void StopTracker()
    {
        if (trackerProcess == null)
            return;

        try
        {
            if (!trackerProcess.HasExited)
            {
                trackerProcess.Kill();
            }
        }
        catch
        {
            // Evitamos errores durante el cierre.
        }

        trackerProcess.Dispose();
        trackerProcess = null;
    }
}