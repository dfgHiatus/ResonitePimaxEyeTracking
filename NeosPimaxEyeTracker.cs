using NeosPimaxEyeTracker;
using System;
using System.Threading;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Diagnostics;

namespace NeosPimaxEyeTracker
{
    public static class DependencyManager
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        public static bool Init() => LoadDLL("VRCPimaxEyeTracker.PimaxEyeTracker", "PimaxEyeTracker.dll");

        private static bool LoadDLL(string resourcePath, string dllName)
        {
            var dllPath = Path.Combine(Path.GetTempPath(), dllName);

            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath + "." + dllName))
            {
                if (resource == null) return false;

                using (Stream dllFile = File.Create(dllPath))
                {
                    resource.Seek(0, SeekOrigin.Begin);
                    resource.CopyTo(dllFile);
                }
            }

            return LoadLibrary(dllPath) != IntPtr.Zero;
        }
    }

    public static class NeosPimaxIntegration
    {
        public static Pimax.EyeTracking.EyeTracker eyeTracker;
        public static bool isChangingActiveStatus = false;
        public static bool needsExpressionUpdate = false;
        public static DateTime changeActiveStateTimer;
        public static String message = "";

        private const float TIMER_CHANGE_ACTIVE_STATE = 3.0f;

        public static void OnApplicationStart()
        {
            DependencyManager.Init();
            eyeTracker = new Pimax.EyeTracking.EyeTracker();
            eyeTracker.OnUpdate += OnEyeTrackerUpdate;
            eyeTracker.OnStart += OnEyeTrackerStart;
            eyeTracker.OnStop += OnEyeTrackerStop;
            // UpdateExpressionParameters();
        }

        public static void OnApplicationQuit()
        {
            if (eyeTracker?.Active ?? false) eyeTracker.Stop();
        }

        public static void OnEyeTrackerStart()
        {
            Console.Write("Eye Tracker Started");
            isChangingActiveStatus = false;
        }

        public static void OnEyeTrackerStop()
        {
            Console.Write("Eye Tracker Stopped");
            isChangingActiveStatus = false;
        }

        public static void OnEyeTrackerUpdate()
        {
            needsExpressionUpdate = true;
        }

        public static string UpdateExpressionParameters()
        {
            if (eyeTracker?.Active ?? false)
            {
                message = "";

                // LeftEyeBlink
                message += "[" + NeosPimaxIntegration.eyeTracker.LeftEye.Expression.Blink.ToString() + "],";
                // RightEyeBlink
                message += "[" + NeosPimaxIntegration.eyeTracker.RightEye.Expression.Blink.ToString() + "],";

                // LeftEyeLid / Openness / Raise
                message += "[" + NeosPimaxIntegration.eyeTracker.LeftEye.Expression.Openness.ToString() + "],";
                // RightEyeLid / Openness / Raise
                message += "[" + NeosPimaxIntegration.eyeTracker.RightEye.Expression.Openness.ToString() + "],";

                // LeftEyeXGaze
                message += "[" + NeosPimaxIntegration.eyeTracker.LeftEye.Expression.PupilCenter.Item1.ToString() + "],";
                // LeftEyeYGaze
                message += "[" + NeosPimaxIntegration.eyeTracker.LeftEye.Expression.PupilCenter.Item2.ToString() + "],";

                // RightEyeXGaze
                message += "[" + NeosPimaxIntegration.eyeTracker.RightEye.Expression.PupilCenter.Item1.ToString() + "],";
                // RightEyeYGaze
                message += "[" + NeosPimaxIntegration.eyeTracker.RightEye.Expression.PupilCenter.Item2.ToString() + "],";

                // EyesXCombinedGaze
                message += "[" + (NeosPimaxIntegration.eyeTracker.RightEye.Expression.Blink ?
                                    eyeTracker.LeftEye.Expression.PupilCenter.Item1.ToString() :
                                    eyeTracker.RightEye.Expression.PupilCenter.Item1.ToString()) + "],";
                // EyesYCombinedGaze
                message += "[" + (NeosPimaxIntegration.eyeTracker.RightEye.Expression.Blink ? 
                                    eyeTracker.LeftEye.Expression.PupilCenter.Item2.ToString() : 
                                    eyeTracker.RightEye.Expression.PupilCenter.Item2.ToString()) + "],";
                
                return message;
            }

            else
            {
                return "";
            }
            
        }
    }

    public class NeosMain : WebSocketBehavior
    {
        public static WebSocket ws;

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            NeosPimaxIntegration.OnApplicationStart();

            using (var ws = new WebSocket("ws://localhost:4649/NeosMain"))
            {
                ws.OnOpen += (sender, e) => ws.Send("Pimax Connected\n");
                ws.OnClose += (sender, e) => ws.Send("Pimax Disconnected\n");
                ws.Connect();
                while (true)
                {
                    Thread.Sleep(1000);
                    ws.Send(NeosPimaxIntegration.UpdateExpressionParameters());
                }
            };
        }

        public static void OnProcessExit(object sender, EventArgs e)
        {
            if (NeosPimaxIntegration.eyeTracker?.Active ?? false) NeosPimaxIntegration.eyeTracker.Stop();
            ws.Close();
        }
    }


}
