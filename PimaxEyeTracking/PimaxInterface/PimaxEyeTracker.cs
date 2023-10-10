using System.Runtime.InteropServices;
using Elements.Core;

namespace Pimax.EyeTracking;

public enum EyeParameter {
	GazeX, // Gaze point on the X axis (not working!)
	GazeY, // Gaze point on the Y axis (not working!)
	GazeRawX, // Gaze point on the X axis before smoothing is applied (not working!)
	GazeRawY, // Gaze point on the Y axis before smoothing is applied (not working!)
	GazeSmoothX, // Gaze point on the X axis after smoothing is applied (not working!)
	GazeSmoothY, // Gaze point on the Y axis after smoothing is applied (not working!)
	GazeOriginX, // Pupil gaze origin on the X axis
	GazeOriginY, // Pupil gaze origin on the Y axis
	GazeOriginZ, // Pupil gaze origin on the Z axis
	GazeDirectionX, // Gaze vector on the X axis (not working!)
	GazeDirectionY, // Gaze vector on the Y axis (not working!)
	GazeDirectionZ, // Gaze vector on the Z axis (not working!)
	GazeReliability, // Gaze point reliability (not working!)
	PupilCenterX, // Pupil center on the X axis, normalized between 0 and 1
	PupilCenterY, // Pupil center on the Y axis, normalized between 0 and 1
	PupilDistance, // Distance between pupil and camera lens, measured in millimeters
	PupilMajorDiameter, // Pupil major axis diameter, normalized between 0 and 1
	PupilMajorUnitDiameter, // Pupil major axis diameter, measured in millimeters
	PupilMinorDiameter, // Pupil minor axis diameter, normalized between 0 and 1
	PupilMinorUnitDiameter, // Pupil minor axis diameter, measured in millimeters
	Blink, // Blink state (not working!)
	Openness, // How open the eye is - 100 (closed), 50 (partially open, unreliable), 0 (open)
        UpperEyelid, // Upper eyelid state (not working!)
	LowerEyelid // Lower eyelid state (not working!)
}

public enum EyeExpression {
	PupilCenterX, // Pupil center on the X axis, smoothed and normalized between -1 (looking left) ... 0 (looking forward) ... 1 (looking right)
	PupilCenterY, // Pupil center on the Y axis, smoothed and normalized between -1 (looking up) ... 0 (looking forward) ... 1 (looking down)
	Openness, // How open the eye is, smoothed and normalized between 0 (fully closed) ... 1 (fully open)
	Blink // Blink, 0 (not blinking) or 1 (blinking)
}

public enum Eye {
	Any,
	Left,
	Right
}

public enum CallbackType {
	Start,
	Stop,
	Update
}

public struct EyeExpressionState
{
    public Eye Eye { get; private set; }
    public float2 PupilCenter { get; private set; }
    public float Openness { get; private set; }
    public bool Blink { get; private set; }

    public EyeExpressionState(Eye eyeType, EyeTracker eyeTracker)
    {
        Eye         = eyeType;
        PupilCenter = new float2(eyeTracker.GetEyeExpression(Eye, EyeExpression.PupilCenterX), eyeTracker.GetEyeExpression(Eye, EyeExpression.PupilCenterY));
        Openness    = eyeTracker.GetEyeExpression(Eye, EyeExpression.Openness);
        Blink       = eyeTracker.GetEyeExpression(Eye, EyeExpression.Blink) != 0.0f;
    }
}

public struct EyeState {
	public Eye Eye { get; private set; }
	public float2 Gaze { get; private set; }
	public float2 GazeRaw { get; private set; }
	public float2 GazeSmooth { get; private set; }
	public float3 GazeOrigin { get; private set; }
	public float3 GazeDirection { get; private set; }
	public float GazeReliability { get; private set; }
	public float2 PupilCenter { get; private set; }
	public float PupilDistance { get; private set; }
	public float PupilMajorDiameter { get; private set; }
	public float PupilMajorUnitDiameter { get; private set; }
	public float PupilMinorDiameter { get; private set; }
	public float PupilMinorUnitDiameter { get; private set; }
	public float Blink { get; private set; }
	public float Openness { get; private set; }
	public float UpperEyelid { get; private set; }
	public float LowerEyelid { get; private set; }
        public EyeExpressionState Expression { get; private set; }

        public EyeState(Eye eyeType, EyeTracker eyeTracker) {
		Eye = eyeType;
		Gaze = new float2(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeX), eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeY));
		GazeRaw = new float2(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeRawX), eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeRawY));
		GazeSmooth = new float2(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeSmoothX), eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeSmoothY));
		GazeOrigin = new float3(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeOriginX), eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeOriginY), eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeOriginZ));
		GazeDirection = new float3(eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeDirectionX), eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeDirectionY), eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeDirectionZ));
		GazeReliability = eyeTracker.GetEyeParameter(Eye, EyeParameter.GazeReliability);
		PupilDistance = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilDistance);
		PupilMajorDiameter = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilMajorDiameter);
		PupilMajorUnitDiameter = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilMajorUnitDiameter);
		PupilMinorDiameter = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilMinorDiameter);
		PupilMinorUnitDiameter = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilMinorUnitDiameter);
		Blink = eyeTracker.GetEyeParameter(Eye, EyeParameter.Blink);
		UpperEyelid = eyeTracker.GetEyeParameter(Eye, EyeParameter.UpperEyelid);
		LowerEyelid = eyeTracker.GetEyeParameter(Eye, EyeParameter.LowerEyelid);
		Openness = eyeTracker.GetEyeParameter(Eye, EyeParameter.Openness);
		PupilCenter = new float2(eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilCenterX), eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilCenterY));
            Expression = new EyeExpressionState(eyeType, eyeTracker);

            // Convert range from 0...1 to -1...1, defaulting eyes to center (0) when unavailable
            float x = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilCenterX);  
            float y = eyeTracker.GetEyeParameter(Eye, EyeParameter.PupilCenterY);
            PupilCenter = new float2(x <= float.Epsilon ? 0.0f : (x * 2.0f - 1.0f), y <= float.Epsilon ? 0.0f : (y * 2.0f - 1.0f));
            Openness = (x <= float.Epsilon && y <= float.Epsilon) ? 0.0f : 1.0f;
        }
}

public delegate void EyeTrackerEventHandler();

public class EyeTracker {
	[DllImport("PimaxEyeTracker", EntryPoint = "RegisterCallback")] private static extern void _RegisterCallback(CallbackType type, EyeTrackerEventHandler callback);
	[DllImport("PimaxEyeTracker", EntryPoint = "Start")] private static extern bool _Start();
	[DllImport("PimaxEyeTracker", EntryPoint = "Stop")] private static extern void _Stop();
	[DllImport("PimaxEyeTracker", EntryPoint = "IsActive")] private static extern bool _IsActive();
	[DllImport("PimaxEyeTracker", EntryPoint = "GetTimestamp")] private static extern System.Int64 _GetTimestamp();
	[DllImport("PimaxEyeTracker", EntryPoint = "GetRecommendedEye")] private static extern Eye _GetRecommendedEye();
	[DllImport("PimaxEyeTracker", EntryPoint = "GetEyeParameter")] private static extern float _GetEyeParameter(Eye eye, EyeParameter param);
	[DllImport("PimaxEyeTracker", EntryPoint = "GetEyeExpression")] private static extern float _GetEyeExpression(Eye eye, EyeExpression expression);

	public EyeTrackerEventHandler OnStart { get; set; }
	private EyeTrackerEventHandler _OnStartHandler = null;

	public EyeTrackerEventHandler OnStop { get; set; }
	private EyeTrackerEventHandler _OnStopHandler = null;

	public EyeTrackerEventHandler OnUpdate { get; set; }
	private EyeTrackerEventHandler _OnUpdateHandler = null;

	public EyeState LeftEye { get; private set; }
        public EyeState RightEye { get; private set; }
        public EyeState RecommendedEye { get; private set; }

        public long Timestamp => _GetTimestamp();
	//public Eye RecommendedEye => _GetRecommendedEye();

	public bool Active => _IsActive();

	public bool Start() {
		_OnStartHandler = _OnStart;
		_RegisterCallback(CallbackType.Start, _OnStartHandler);

		_OnStopHandler = _OnStop;
		_RegisterCallback(CallbackType.Stop, _OnStopHandler);

		_OnUpdateHandler = _OnUpdate;
		_RegisterCallback(CallbackType.Update, _OnUpdateHandler);

		return _Start();
	}

	public void Stop() => _Stop();

	public float GetEyeParameter(Eye eye, EyeParameter param) => _GetEyeParameter(eye, param);
	public float GetEyeExpression(Eye eye, EyeExpression expression) => _GetEyeExpression(eye, expression);

	private void _OnUpdate() {
		if(Active) {
			LeftEye = new EyeState(Eye.Left, this);
                RightEye = new EyeState(Eye.Right, this);
                RecommendedEye = new EyeState(_GetRecommendedEye(), this);
                OnUpdate?.Invoke();
		}
	}

	private void _OnStart() => OnStart?.Invoke();
	private void _OnStop() => OnStop?.Invoke();
}

public class PimaxEyeTracker
{
	public EyeTracker EyeTracker;

	void Awake() 
	{
		EyeTracker = new EyeTracker();
	}

	void OnDestroy() 
	{
		if (EyeTracker.Active) EyeTracker.Stop();
	}
}