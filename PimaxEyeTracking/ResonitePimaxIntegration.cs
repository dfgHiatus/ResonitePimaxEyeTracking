﻿using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using Elements.Core;
using Pimax.EyeTracking;
using System;

namespace ResonitePimaxIntegration;

public class ResonitePimaxIntegration : ResoniteMod
{
	public override string Name => "PimaxEyeTracking";
	public override string Author => "dfgHiatus";
	public override string Version => "2.0.0";
	public override string Link => "https://github.com/dfgHiatus/ResonitePimaxEyeTracking/";

	private static ModConfiguration config;

	[AutoRegisterConfigKey]
	private readonly static ModConfigurationKey<float> Alpha = 
		new("alpha", "Eye X Sensitivity", () => 1.0f);

	[AutoRegisterConfigKey]
	private readonly static ModConfigurationKey<float> Beta = 
		new("beta", "Eye Y Sensitivity", () => 1.0f);

	[AutoRegisterConfigKey]
	private readonly static ModConfigurationKey<bool> SwapX = 
		new("SwapX", "Invert Eye X Movement", () => false);
	private static float _swappedX = 1f;

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> SwapY = 
		new("SwapY", "Invert Eye Y Movement", () => true);
	private static float _swappedY = -1f;

	[AutoRegisterConfigKey]
    private readonly static ModConfigurationKey<bool> SwapLeftBlink = 
		new("SwapLeftBlink", "Invert Left Eye Blink", () => false);
	private static bool _swappedLeftBlink = false;

	[AutoRegisterConfigKey]
	private readonly static ModConfigurationKey<bool> SwapRightBlink = 
		new("SwapRightBlink", "Invert Right Eye Blink", () => false);
	private static bool _swappedRightBlink = false;

	private readonly static EyeTracker eyeTracker = new();
	public override void OnEngineInit()
	{
		new Harmony("net.dfgHiatus.PimaxEyeTracking").PatchAll();
		config = GetConfiguration();
		config.OnThisConfigurationChanged += HandleChanges;
		Engine.Current.OnShutdown += () => eyeTracker.Stop();
	}

    private void HandleChanges(ConfigurationChangedEvent configurationChangedEvent)
    {
        switch (configurationChangedEvent.Key.Name)
        {
			case "SwapX":
				_swappedX = config.GetValue(SwapX) ? -1 : 1f;
				break;
			case "SwapY":
				_swappedY = config.GetValue(SwapY) ? -1 : 1f;
				break;
			case "SwapLeftBlink":
				_swappedLeftBlink = config.GetValue(SwapLeftBlink);
				break;
			case "SwapRightBlink":
				_swappedRightBlink = config.GetValue(SwapRightBlink);
				break;
			default:
				break;
		}
    }

	[HarmonyPatch(typeof(InputInterface), MethodType.Constructor)]
	[HarmonyPatch(new[] { typeof(Engine)})]
	public class InputInterfaceCtorPatch
	{
		public static void Postfix(InputInterface __instance)
		{
			try
			{
				PimaxEyeInputDevice pi = new();
				__instance.RegisterInputDriver(pi);
				Debug("Pimax Module Registered: " + pi.ToString());
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
		public int UpdateOrder => 100;
		private const float _defaultPupilSize = 0.0035f;

		public void CollectDeviceInfos(DataTreeList list)
		{
			DataTreeDictionary dataTreeDictionary = new();
			dataTreeDictionary.Add("Name", "Pimax Eye Tracking");
			dataTreeDictionary.Add("Type", "Eye Tracking");
			dataTreeDictionary.Add("Model", "Droolon Pi1");
			list.Add(dataTreeDictionary);
		}

		public void RegisterInputs(InputInterface inputInterface)
		{
			if (!eyeTracker.Active)
			{
				eyeTracker.Start();
			}
			eyes = new Eyes(inputInterface, "Pimax Eye Tracking");
		}

		public void UpdateInputs(float deltaTime)
		{
			eyes.IsEyeTrackingActive = eyeTracker.Active & Engine.Current.InputInterface.VR_Active;

			var leftEyeDirection = Project2DTo3D(_swappedX * eyeTracker.LeftEye.PupilCenter.X, _swappedY * eyeTracker.LeftEye.PupilCenter.Y);
			var leftEyeFakeWiden = MathX.Remap(MathX.Clamp01(eyeTracker.LeftEye.PupilCenter.Y), 0f, 1f, 0f, 0.33f);
			UpdateEye(leftEyeDirection, float3.Zero, eyeTracker.Active, _defaultPupilSize, ComputeOpeness(eyeTracker.LeftEye.Openness, EyeSide.Left),
				leftEyeFakeWiden, 0f, 0f, deltaTime, eyes.LeftEye) ;

			var rightEyeDirection = Project2DTo3D(_swappedX * eyeTracker.RightEye.PupilCenter.X, _swappedY * eyeTracker.RightEye.PupilCenter.Y);
			var rightEyeFakeWiden = MathX.Remap(MathX.Clamp01(eyeTracker.RightEye.PupilCenter.Y), 0f, 1f, 0f, 0.33f);
			UpdateEye(rightEyeDirection, float3.Zero, eyeTracker.Active, _defaultPupilSize, ComputeOpeness(eyeTracker.RightEye.Openness, EyeSide.Right),
				rightEyeFakeWiden, 0f, 0f, deltaTime, eyes.RightEye);

			var combinedEyeDirection = MathX.Average(eyes.LeftEye.Direction, eyes.RightEye.Direction);
			var combinedEyeFakeWiden = MathX.Remap(MathX.Clamp01(MathX.Average(
				eyeTracker.LeftEye.PupilCenter.Y, eyeTracker.RightEye.PupilCenter.Y)), 0f, 1f, 0f, 0.33f); // Should not need ComputeOpeness here
			UpdateEye(combinedEyeDirection, float3.Zero, eyeTracker.Active, _defaultPupilSize, MathX.Max(eyeTracker.LeftEye.Openness, eyeTracker.RightEye.Openness),
				combinedEyeFakeWiden, 0f, 0f, deltaTime, eyes.CombinedEye);

			eyes.ComputeCombinedEyeParameters();
			eyes.Timestamp = eyeTracker.Timestamp * 0.001;
			eyes.FinishUpdate();
		}

		private void UpdateEye(float3 gazeDirection, float3 gazeOrigin, bool status, float pupilSize, float openness,
			float widen, float squeeze, float frown, float deltaTime, FrooxEngine.Eye eye)
		{
			eye.IsDeviceActive = Engine.Current.InputInterface.VR_Active;
			eye.IsTracking = status;

			if (eye.IsTracking)
			{
				eye.UpdateWithDirection(gazeDirection);
				eye.RawPosition = gazeOrigin;
				eye.PupilDiameter = pupilSize;
			}

			eye.Openness = openness; // In this case will be 0 or 1.
			eye.Widen = widen;
			eye.Squeeze = squeeze;
			eye.Frown = frown;
		}

		private static float3 Project2DTo3D(float x, float y)
		{
			return new float3(MathX.Tan(config.GetValue(Alpha) * x),
							  MathX.Tan(config.GetValue(Beta) * y),
							  1f).Normalized;
		}

		private static float ComputeOpeness(float openess, EyeSide side)
        {
			if (_swappedLeftBlink && side == EyeSide.Left)
				return (-1 * openess) + 1;
			else if (_swappedRightBlink && side == EyeSide.Right)
				return (-1 * openess) + 1;
			return openess;
		}
	}
}
