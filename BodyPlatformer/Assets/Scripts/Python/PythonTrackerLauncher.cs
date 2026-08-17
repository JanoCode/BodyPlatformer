using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonTrackerLauncher : MonoBehaviour
{
    [Header("Rutas")]
    [SerializeField] private string pythonExecutablePath;
    [SerializeField] private string trackerScriptPath;

    private Process trackerProcess;

    private void Start()
    {
        StartTracker();
    }

    private void StartTracker()
    {
        if (!File.Exists(pythonExecutablePath))
        {
            UnityEngine.Debug.LogError(
                "No se encontró Python en: " + pythonExecutablePath
            );
            return;
        }

        if (!File.Exists(trackerScriptPath))
        {
            UnityEngine.Debug.LogError(
                "No se encontró pose_tracking.py en: " + trackerScriptPath
            );
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutablePath,
            Arguments = $"\"{trackerScriptPath}\"",
            WorkingDirectory = Path.GetDirectoryName(trackerScriptPath),
            UseShellExecute = false,
            CreateNoWindow = false
        };

        trackerProcess = new Process();
        trackerProcess.StartInfo = startInfo;

        try
        {
            trackerProcess.Start();

            UnityEngine.Debug.Log(
                "Tracker de MediaPipe iniciado."
            );
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError(
                "No se pudo iniciar el tracker: " + e.Message
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
            // Evitamos errores al cerrar Unity.
        }

        trackerProcess.Dispose();
        trackerProcess = null;
    }
}