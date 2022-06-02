using System;
using System.Collections.Generic;
using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using BaseX;
using Pimax.EyeTracking;
using FrooxEngine.UIX;

namespace NeosPimaxIntegration
{
	public class NeosPimaxIntegration : NeosMod
	{
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> ALPHA = new ModConfigurationKey<float>("eye_swing_alpha", "Eye Swing Alpha (X)", () => 2f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> BETA = new ModConfigurationKey<float>("eye_swing_beta", "Eye Swing Beta (Y)", () => 2f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> lerpSpeed = new ModConfigurationKey<float>("lerpSpeed", "Lerp Speed", () => 24f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<bool> offsetbutton = new ModConfigurationKey<bool>("offsetButton", "If true, reset Gaze Offset", () => false);

		private static Dictionary<EyeSide, float3> offsetDictionary = new Dictionary<EyeSide, float3>();
		private bool offsetButtonValue
        {
			get { return offsetButtonValue; }
			set 
			{ 
				if (offsetButtonValue == true)
                {
					PimaxEyeInputDevice.calculateEyeOffset();

				}
				offsetButtonValue = value; 
			}
        }

        public override string Name => "PimaxEyeTracking";
		public override string Author => "dfgHiatus";
		public override string Version => "1.0.1-Irix";
		public override string Link => "https://github.com/dfgHiatus/NeosPimaxEyeTracking/";

		public static ModConfiguration config;
		public override void OnEngineInit()
		{
			// Harmony.DEBUG = true;
			config = GetConfiguration();
			Harmony harmony = new Harmony("net.dfg.PimaxEyeTracking");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(InputInterface), MethodType.Constructor)]
		[HarmonyPatch(new[] { typeof(Engine)})]
		public class InputInterfaceCtorPatch
		{
			public static void Postfix(InputInterface __instance)
			{
				try
				{
					PimaxEyeInputDevice pi = new PimaxEyeInputDevice();
					__instance.RegisterInputDriver(pi);
				}
				catch (Exception e)
				{
					Warn("PimaxEyeTracking failed to initiallize.");
					Warn(e.ToString());
				}
			}
		}
		public class PimaxEyeInputDevice : IInputDriver
		{
			public static Eyes eyes;
			public EyeTracker pimaxEyeTracker = new EyeTracker();
			public int UpdateOrder => 100;
			// Requires a license to track
			private const float constPupilSize = 0.003f;
			public float lerp;

			public void CollectDeviceInfos(DataTreeList list)
			{
				DataTreeDictionary dataTreeDictionary = new DataTreeDictionary();
				dataTreeDictionary.Add("Name", "Pimax Eye Tracking");
				dataTreeDictionary.Add("Type", "Eye Tracking");
				dataTreeDictionary.Add("Model", "Droolon Pi1");
				list.Add(dataTreeDictionary);
			}

			public void RegisterInputs(InputInterface inputInterface)
			{
				if (!pimaxEyeTracker.Active)
				{
					if (!pimaxEyeTracker.Start())
					{
						Warn("Could not connect to Pimax Eye Tracking Service");
						return;
					}
				}
				eyes = new Eyes(inputInterface, "Pimax Eye Tracking");
			}

			public static void calculateEyeOffset()
			{
				offsetDictionary[EyeSide.Left] = eyes.LeftEye.Direction;
				offsetDictionary[EyeSide.Right] = eyes.RightEye.Direction;
				offsetDictionary[EyeSide.Combined] = eyes.CombinedEye.Direction;
			}

			public void UpdateInputs(float deltaTime)
			{
				eyes.IsEyeTrackingActive = pimaxEyeTracker.Active & Engine.Current.InputInterface.VR_Active;

				eyes.LeftEye.Direction = new float3(MathX.Tan(config.GetValue(ALPHA) * pimaxEyeTracker.LeftEye.PupilCenter.X),
														  MathX.Tan(config.GetValue(BETA) * pimaxEyeTracker.LeftEye.PupilCenter.Y * -1),
														  1f).Normalized - offsetDictionary[EyeSide.Left];
				eyes.LeftEye.RawPosition = float3.Zero;
				lerp = pimaxEyeTracker.LeftEye.Openness == 1f ? config.GetValue(lerpSpeed) : -1f * config.GetValue(lerpSpeed);
				eyes.LeftEye.Openness = MathX.SmoothLerp(eyes.LeftEye.Openness, pimaxEyeTracker.LeftEye.Openness, ref lerp, deltaTime);
				eyes.LeftEye.PupilDiameter = constPupilSize;
				eyes.LeftEye.IsTracking = pimaxEyeTracker.Active;
				eyes.LeftEye.IsDeviceActive = pimaxEyeTracker.Active;
				eyes.LeftEye.Widen = MathX.Clamp01(pimaxEyeTracker.LeftEye.PupilCenter.Y);
				eyes.LeftEye.Squeeze = MathX.Abs(MathX.Clamp(pimaxEyeTracker.LeftEye.PupilCenter.Y, -1f, 0f));

				eyes.RightEye.Direction = new float3(MathX.Tan(config.GetValue(ALPHA) * pimaxEyeTracker.RightEye.PupilCenter.X),
														  MathX.Tan(config.GetValue(BETA) * pimaxEyeTracker.RightEye.PupilCenter.Y * -1),
														  1f).Normalized - offsetDictionary[EyeSide.Right];
				eyes.RightEye.RawPosition = float3.Zero;
				lerp = pimaxEyeTracker.RightEye.Openness == 1f ? config.GetValue(lerpSpeed) : -1f * config.GetValue(lerpSpeed);
				eyes.RightEye.Openness = MathX.SmoothLerp(eyes.RightEye.Openness, pimaxEyeTracker.RightEye.Openness, ref lerp, deltaTime);
				eyes.RightEye.PupilDiameter = constPupilSize;
				eyes.RightEye.IsTracking = pimaxEyeTracker.Active;
				eyes.RightEye.IsDeviceActive = pimaxEyeTracker.Active;
				eyes.RightEye.Widen = MathX.Clamp01(pimaxEyeTracker.RightEye.PupilCenter.Y);
				eyes.RightEye.Squeeze = MathX.Abs(MathX.Clamp(pimaxEyeTracker.RightEye.PupilCenter.Y, -1f, 0f));

				eyes.CombinedEye.Direction = MathX.Average(eyes.LeftEye.Direction, eyes.RightEye.Direction) - offsetDictionary[EyeSide.Combined];
				eyes.CombinedEye.RawPosition = float3.Zero;
				lerp = pimaxEyeTracker.LeftEye.Openness == 1f || pimaxEyeTracker.RightEye.Openness == 1f ? 
					config.GetValue(lerpSpeed) : -1f * config.GetValue(lerpSpeed);
				eyes.CombinedEye.Openness = MathX.SmoothLerp(
					eyes.CombinedEye.Openness,
					MathX.Max(pimaxEyeTracker.LeftEye.Openness, pimaxEyeTracker.RightEye.Openness),
					ref lerp,
					deltaTime);
				eyes.CombinedEye.PupilDiameter = constPupilSize;
				eyes.CombinedEye.IsTracking = pimaxEyeTracker.Active;
				eyes.CombinedEye.IsDeviceActive = pimaxEyeTracker.Active;
				eyes.CombinedEye.Widen = MathX.Average(eyes.LeftEye.Widen, eyes.RightEye.Widen);
				eyes.CombinedEye.Squeeze = MathX.Average(eyes.LeftEye.Squeeze, eyes.RightEye.Squeeze);

				// Vive Pro Eye Style.
				eyes.Timestamp += deltaTime;
			}
		}
	}
}
