using System;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace NeosPimaxEyeTracker
{

    public static class NeosPimaxIntegration
    {
        public static WebSocketServer wssv;

        public class WebSocketClass : WebSocketBehavior
        {
            // Empty string from Neos
            protected override void OnMessage(MessageEventArgs e)
            {
                Console.WriteLine("[Network Message] Recieved: " +
                    ((String.IsNullOrEmpty(e.Data)) ? "(Nothing.)" : e.Data));
                Console.WriteLine("[Network Message] Sent: " +
                    UpdateExpressionParameters());
                Send(UpdateExpressionParameters());
            }

            public static void MakeServer()
            {
                wssv = new WebSocketServer("ws://localhost:4649");
                wssv.AddWebSocketService<WebSocketClass>("/WebSocketClass");
                wssv.Start();
            }

            public void SendNeosMessage()
            {
                Send(UpdateExpressionParameters());
            }
        }

        public static Pimax.EyeTracking.EyeTracker eyeTracker;
        public static bool isChangingActiveStatus = false;
        public static bool needsExpressionUpdate = false;
        public static DateTime changeActiveStateTimer;
        public static String message = "";

        private const float TIMER_CHANGE_ACTIVE_STATE = 3.0f;

        public static void OnApplicationStart()
        {
            Console.WriteLine("[Info] Starting...");
            eyeTracker = new Pimax.EyeTracking.EyeTracker();
            eyeTracker.OnUpdate += OnEyeTrackerUpdate;
            eyeTracker.OnStart += OnEyeTrackerStart;
            eyeTracker.OnStop += OnEyeTrackerStop;
            // Start returns true...
            Console.WriteLine("[Info] Eye Tracking Initialized: " + eyeTracker.Start());
            Console.WriteLine("[Current Tracking Status] " + (eyeTracker?.Active ?? false).ToString());
            Console.WriteLine("[Info] Starting Websocket Server...");
            WebSocketClass.MakeServer();
            Console.WriteLine("[Info] Websocket Server created.");
            Console.WriteLine("");
        }

        public static void OnApplicationQuit()
        {
            Console.WriteLine("[Info] Quitting...");
            if (eyeTracker?.Active ?? false)
                eyeTracker.Stop();
        }

        public static void OnEyeTrackerStart()
        {
            Console.WriteLine("[Info] Eye Tracker Started");
            isChangingActiveStatus = false;
        }

        public static void OnEyeTrackerStop()
        {
            Console.WriteLine("[Info] Eye Tracker Stopped");
            isChangingActiveStatus = false;
        }

        public static void OnEyeTrackerUpdate()
        {
            Console.WriteLine("[Info] Eye Tracker Updated");
            Console.WriteLine(UpdateExpressionParameters());
            // Send to websocket here?
            needsExpressionUpdate = true;
        }

        public static string UpdateExpressionParameters()
        {
            Console.WriteLine("[Current Tracking Status] " + NeosPimaxIntegration.eyeTracker?.Active.ToString());

            if (NeosPimaxIntegration.eyeTracker?.Active ?? false)
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
                message += "[" + NeosPimaxIntegration.eyeTracker.LeftEye.Expression.PupilCenter.Item1.ToString() + ";";
                // LeftEyeYGaze
                message +=       NeosPimaxIntegration.eyeTracker.LeftEye.Expression.PupilCenter.Item2.ToString() + "],";

                // RightEyeXGaze
                message += "[" + NeosPimaxIntegration.eyeTracker.RightEye.Expression.PupilCenter.Item1.ToString() + ";";
                // RightEyeYGaze
                message +=       NeosPimaxIntegration.eyeTracker.RightEye.Expression.PupilCenter.Item2.ToString() + "],";

                // EyesXCombinedGaze
                message += "[" + (NeosPimaxIntegration.eyeTracker.RightEye.Expression.Blink ?
                                    NeosPimaxIntegration.eyeTracker.LeftEye.Expression.PupilCenter.Item1.ToString() :
                                    NeosPimaxIntegration.eyeTracker.RightEye.Expression.PupilCenter.Item1.ToString()) + "],";
                // EyesYCombinedGaze
                message += "[" + (NeosPimaxIntegration.eyeTracker.RightEye.Expression.Blink ?
                                    NeosPimaxIntegration.eyeTracker.LeftEye.Expression.PupilCenter.Item2.ToString() :
                                    NeosPimaxIntegration.eyeTracker.RightEye.Expression.PupilCenter.Item2.ToString()) + "],";
                
                return message;
            }

            else
            {
                return "EyeTrackerDisabled";
            }
            
        }

        public class NeosMain
        {
            public void Main(string[] args)
            {
                // Console.WriteLine();
                Console.WriteLine("Press Esc to terminate, Press Enter to view Eye Tracking Data.");
                AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
                OnApplicationStart();

                // Moved Websocket stuff to

                while (wssv.IsListening)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                    {
                        // Will close program    
                        wssv.Stop();
                    }
                    else
                    {

                        // Otherwise output our values
                        Console.WriteLine("[Info] Values: " +
                            UpdateExpressionParameters());
                    }
                    Thread.Sleep(200);

                }
            }

            public void OnProcessExit(object sender, EventArgs e)
            {
                OnApplicationQuit();
                if (eyeTracker?.Active ?? false)
                    eyeTracker.Stop();
                if (wssv.IsListening)
                    wssv.Stop();
            }
        }
    }

}
