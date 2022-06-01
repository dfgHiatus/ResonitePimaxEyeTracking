using System;
using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using BaseX;
using Pimax.EyeTracking;

namespace NeosPimaxIntegration
{
	public class NeosPimaxIntegration : NeosMod
	{
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> ALPHA = new ModConfigurationKey<float>("eye_swing_alpha", "Eye Swing Alpha (X)", () => 2f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> BETA = new ModConfigurationKey<float>("eye_swing_beta", "Eye Swing Beta (Y)", () => 2f);

		// Cheeky eye tests
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> lerpSpeed = new ModConfigurationKey<float>("lerpSpeed", "lerpSpeed", () => 24f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> leftEyeX = new ModConfigurationKey<float>("leftEyeX", "leftEyeX", () => 0f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> leftEyeY = new ModConfigurationKey<float>("leftEyeY", "leftEyeY", () => 0f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> rightEyeX = new ModConfigurationKey<float>("rightEyeX", "rightEyeX", () => 0f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> rightEyeY = new ModConfigurationKey<float>("rightEyeY", "rightEyeY", () => 0f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> leftOpen = new ModConfigurationKey<float>("leftOpen", "leftOpen", () => 1f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<float> rightOpen = new ModConfigurationKey<float>("rightOpen", "rightOpen", () => 1f);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<bool> pimaxActive = new ModConfigurationKey<bool>("pimaxActive", "pimaxActive", () => true);
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<bool> eyeTrackingActive = new ModConfigurationKey<bool>("eyeTrackingActive", "eyeTrackingActive", () => true);

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
			public Eyes eyes;
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
/*				if (!config.GetValue(pimaxActive))
				{
					if (!pimaxEyeTracker.Start())
					{
						Warn("Could not connect to Pimax Eye Tracking Service");
						return;
					}
				}*/
				eyes = new Eyes(inputInterface, "Pimax Eye Tracking");
			}

			public void UpdateInputs(float deltaTime)
			{
				eyes.IsEyeTrackingActive = config.GetValue(eyeTrackingActive);

				eyes.LeftEye.Direction = new float3(MathX.Tan(config.GetValue(ALPHA) * config.GetValue(leftEyeX)),
														  MathX.Tan(config.GetValue(BETA) * config.GetValue(leftEyeY) * -1),
														  1f).Normalized;
				eyes.LeftEye.RawPosition = float3.Zero;
				lerp = config.GetValue(leftOpen) == 1f ? config.GetValue(lerpSpeed) : -1f * config.GetValue(lerpSpeed);
				eyes.LeftEye.Openness = MathX.SmoothLerp(eyes.LeftEye.Openness, config.GetValue(leftOpen), ref lerp, deltaTime);
				eyes.LeftEye.PupilDiameter = constPupilSize;
				eyes.LeftEye.IsTracking = config.GetValue(pimaxActive);
				eyes.LeftEye.IsDeviceActive = config.GetValue(eyeTrackingActive);
				eyes.LeftEye.Widen = MathX.Clamp01(config.GetValue(leftEyeY));
				eyes.LeftEye.Squeeze = MathX.Remap(MathX.Clamp(config.GetValue(leftEyeY), -1f, 0f), -1f, 0f, 0f, 1f);

				eyes.RightEye.Direction = new float3(MathX.Tan(config.GetValue(ALPHA) * config.GetValue(rightEyeX)),
														  MathX.Tan(config.GetValue(BETA) * config.GetValue(rightEyeY) * -1),
														  1f).Normalized;
				eyes.RightEye.RawPosition = float3.Zero;
				lerp = config.GetValue(rightOpen) == 1f ? config.GetValue(lerpSpeed) : -1f * config.GetValue(lerpSpeed);
				eyes.RightEye.Openness = MathX.SmoothLerp(eyes.RightEye.Openness, config.GetValue(rightOpen), ref lerp, deltaTime);
				eyes.RightEye.PupilDiameter = constPupilSize;
				eyes.RightEye.IsTracking = config.GetValue(pimaxActive);
				eyes.RightEye.IsDeviceActive = config.GetValue(eyeTrackingActive);
				eyes.RightEye.Widen = MathX.Clamp01(config.GetValue(rightEyeY));
				eyes.RightEye.Squeeze = MathX.Remap(MathX.Clamp(config.GetValue(rightEyeY), -1f, 0f), -1f, 0f, 0f, 1f);

				eyes.CombinedEye.Direction = new float3(MathX.Average(MathX.Tan(config.GetValue(ALPHA) * config.GetValue(leftEyeX)),
																		   MathX.Tan(config.GetValue(ALPHA) * config.GetValue(rightEyeX))),
													    MathX.Average(MathX.Tan(config.GetValue(BETA) * config.GetValue(leftEyeY)),
																		   MathX.Tan(config.GetValue(BETA) * config.GetValue(rightEyeY) * -1)),
													    1f).Normalized;
				eyes.CombinedEye.RawPosition = float3.Zero;
				lerp = config.GetValue(leftOpen) == 1f || config.GetValue(rightOpen) == 1f ? 
					config.GetValue(lerpSpeed) : -1f * config.GetValue(lerpSpeed);
				eyes.CombinedEye.Openness = MathX.SmoothLerp(
					eyes.CombinedEye.Openness,
					MathX.Max(pimaxEyeTracker.LeftEye.Openness, pimaxEyeTracker.RightEye.Openness),
					ref lerp,
					deltaTime);
				eyes.CombinedEye.PupilDiameter = constPupilSize;
				eyes.CombinedEye.IsTracking = config.GetValue(pimaxActive);
				eyes.CombinedEye.IsDeviceActive = config.GetValue(eyeTrackingActive);
				eyes.CombinedEye.Widen = MathX.Average(MathX.Clamp01(config.GetValue(leftEyeX)), MathX.Clamp01(config.GetValue(leftEyeY)));
				eyes.CombinedEye.Squeeze = MathX.Average(MathX.Remap(MathX.Clamp(config.GetValue(leftEyeY), -1f, 0f), -1f, 0f, 0f, 1f),
																MathX.Remap(MathX.Clamp(config.GetValue(rightEyeY), -1f, 0f), -1f, 0f, 0f, 1f));

				// Vive Pro Eye Style.
				eyes.Timestamp += deltaTime;
			}
		}
	}
}
