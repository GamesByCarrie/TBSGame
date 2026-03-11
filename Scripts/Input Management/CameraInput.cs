using Godot;

public partial class CameraInput : Node3D
{
	[Export]
	private Camera3D camera = null;

	[ExportCategory("Rotation Settings")]
	[Export(PropertyHint.Range, "0.1, 1.0")] private float mouseSensitivity = 0.5f;
	[Export(PropertyHint.Range, "0.05, 1.0, suffix:s")] private float rotationDuration = 0.1f;
	[Export(PropertyHint.Range, "30, 90, 15, radians_as_degrees")] private float buttonRotateStep = Mathf.Pi / 4f;
	private Vector2 mouseDirection = Vector2.Zero;
	private bool altControlsOn = false;
	private bool canRotate = false;
	private bool canRoll = false;
	private Tween rotationTween = null;

	[ExportCategory("Zoom Settings")]
	[Export(PropertyHint.Range, "0.05, 1.0, suffix:s")] private float zoomDuration = 0.1f;
	[Export(PropertyHint.Range, "0.5, 2.0, suffix:m")] private float zoomStep = 1f;
	[Export(PropertyHint.None, "suffix:m")] private float minZoom = 4f;
	[Export(PropertyHint.None, "suffix:m")] private float maxZoom = 7.5f;
	private Tween zoomTween = null;

	// Applying inputs to the cube
    public override void _PhysicsProcess(double delta)
	{
		// Rotating the cube
		Rotate(Vector3.Right, mouseDirection.Y * (float)delta);

		if (canRotate)
		{
			Rotate(Vector3.Up, mouseDirection.X * (float)delta);	
		}
		else if (canRoll)
		{
			Rotate(Vector3.Forward, mouseDirection.X * (float)delta);
		}

		mouseDirection = Vector2.Zero;
	}

	/// <summary>
	/// Handles input from the cube rotation Buttons. All Buttons are
	/// connected here via Signals and can be viewed from the editor.
	/// This function should NEVER be called directly.
	/// </summary>
	/// <param name="axis">The axis the cube should rotate about</param>
	private void GUIInput(Vector3 axis)
	{
		// Get necessary data for the rotation
		Quaternion startingQuaternion = GlobalBasis.GetRotationQuaternion();
		Quaternion targetQuaternion = GlobalBasis.Rotated(axis, buttonRotateStep).GetRotationQuaternion();
		Vector3 originalScale = GlobalBasis.Scale;

		// Slerp the rotation in a Tween for smooth movement
		rotationTween?.Kill();
		rotationTween = null;

		rotationTween = CreateTween();
		// Slerp has to be manually called in the Tween because the default functionality for rotation in Tweens uses Euler angles
		rotationTween.TweenMethod(Callable.From((float weight) => GlobalBasis = new Basis(startingQuaternion.Slerp(targetQuaternion, weight)).Scaled(originalScale)), 0.0f, 1.0f, rotationDuration);
	}

	// Input handling
    public override void _UnhandledInput(InputEvent inputEvent)
    {
		Vector3 targetPositionLocal = Vector3.Zero;

		// Toggles alternate camera controls
		if (inputEvent.IsActionPressed("toggle_alt_controls"))
		{
			altControlsOn = true;

			if (canRotate)
			{
				canRotate = false;
				canRoll = true;
			}
		}
		else if (inputEvent.IsActionReleased("toggle_alt_controls"))
		{
			altControlsOn = false;

			if (canRoll)
			{
				canRoll = false;
				canRotate = true;
			}
		}

		// Are we holding the rotate button?
        if (inputEvent.IsActionPressed("rotate_cam"))
		{
			if (altControlsOn)
			{
				canRoll = true;
			}
			else
			{
				canRotate = true;
			}
		}
		else if (inputEvent.IsActionReleased("rotate_cam"))
		{
			canRotate = false;
			canRoll = false;
		}

		// The camera can rotate ONLY if we are both moving the mouse and holding the rotate button
		if (inputEvent is InputEventMouseMotion && (canRotate || canRoll))
		{
			mouseDirection = (inputEvent as InputEventMouseMotion).ScreenRelative * mouseSensitivity;
		}

		// Set target position in local space
		if (inputEvent.IsActionReleased("zoom_in_cam"))
		{
			targetPositionLocal = camera.Position - camera.Basis.Z.Normalized() * zoomStep;
			targetPositionLocal = new Vector3(targetPositionLocal.X, Mathf.Max(targetPositionLocal.Y, minZoom), Mathf.Max(targetPositionLocal.Z, minZoom));
		}
		else if (inputEvent.IsActionReleased("zoom_out_cam"))
		{
			targetPositionLocal = camera.Position + camera.Basis.Z.Normalized() * zoomStep;
			targetPositionLocal = new Vector3(targetPositionLocal.X, Mathf.Min(targetPositionLocal.Y, maxZoom), Mathf.Min(targetPositionLocal.Z, maxZoom));
		}

		// Zoom the camera in or out with a Tween for smooth movement
		if (inputEvent.IsActionReleased("zoom_in_cam") || inputEvent.IsActionReleased("zoom_out_cam"))
		{
			zoomTween?.Kill();
			zoomTween = null;

			zoomTween = CreateTween();
			zoomTween.TweenProperty(camera, "position", targetPositionLocal, zoomDuration);
		}
    }
}