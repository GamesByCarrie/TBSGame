using Godot;

public partial class CameraInput : Node3D
{
	private Camera3D camera = null;

	[ExportCategory("Rotation Settings")]
	[Export(PropertyHint.Range, "0.1, 1.0")] private float mouseSensitivity = 0.5f;
	[Export(PropertyHint.Range, "0.05, 1.0")] private float rotationSpeed = 0.1f;
	[Export(PropertyHint.Range, "30, 90, 15, radians_as_degrees")] private float buttonRotateStep = Mathf.Pi / 4f;
	private Vector2 mouseDirection = Vector2.Zero;
	private bool altControlsOn = false;
	private bool canRotate = false;
	private bool canRoll = false;
	private Quaternion targetRotation = Quaternion.Identity;
	private float currentRotationAmount = 0f;

	[ExportCategory("Zoom Settings")]
	[Export(PropertyHint.Range, "0.05, 1.0")] private float zoomSpeed = 0.1f;
	[Export(PropertyHint.Range, "0.5, 2.0, suffix:m")] private float zoomStep = 1f;
	[Export(PropertyHint.None, "suffix:m")] private float minZoom = 4f;
	[Export(PropertyHint.None, "suffix:m")] private float maxZoom = 7.5f;
	private Vector3 targetPositionLocal = Vector3.Zero;
	private float currentZoomAmount = 0f;

	// Initializing variables
    public override void _Ready()
	{
		camera = GetNode<Camera3D>("Camera3D");
	}

	// Applying inputs
    public override void _PhysicsProcess(double delta)
	{
		// Rotate the camera via mouse movement
		Rotate(GlobalBasis.X.Normalized(), -mouseDirection.Y * (float)delta);

		// Rotate or roll the camera
		if (canRotate)
		{
			Rotate(GlobalBasis.Y.Normalized(), -mouseDirection.X * (float)delta);	
		}
		else if (canRoll)
		{
			Rotate(GlobalBasis.Z.Normalized(), mouseDirection.X * (float)delta);
		}

		mouseDirection = Vector2.Zero;

		// Rotate the camera via button press
		if (targetRotation != Quaternion.Identity)
		{
			// Slerp the rotation for smooth movement
			currentRotationAmount = Mathf.Min(currentRotationAmount + rotationSpeed, 1f);
			GlobalBasis = new Basis(GlobalBasis.GetRotationQuaternion().Slerp(targetRotation, currentRotationAmount));

			// Reset when the rotation is done
			if (currentRotationAmount == 1f)
			{
				currentRotationAmount = 0f;
				targetRotation = Quaternion.Identity;
			}
		}

		// Zoom the camera
		if (targetPositionLocal.Z > 0)
		{
			// Lerp the camera position for smooth movement
			currentZoomAmount = Mathf.Min(currentZoomAmount + zoomSpeed, 1f);
			camera.Position = camera.Position.Lerp(targetPositionLocal, currentZoomAmount);

			// Reset when the movement is done
			if (currentZoomAmount == 1f)
			{
				currentZoomAmount = 0f;
				targetPositionLocal = Vector3.Zero;
			}
		}
	}

	/// <summary>
	/// Handles input from the camera rotation Buttons. All Buttons are
	/// connected here via Signals and can be viewed from the editor.
	/// This function should NEVER be called directly.
	/// </summary>
	/// <param name="direction">The direction the camera should rotate</param>
	private void GUIInput(Vector3 direction)
	{
		// Determine which axis and direction to use
		bool shouldRotateOpposite = direction.Abs() != direction;
		Vector3 rotationAxis = GlobalBasis.Z;

		if (direction.X != 0f)
		{
			rotationAxis = GlobalBasis.X;
		}
		else if (direction.Y != 0f)
		{
			rotationAxis = GlobalBasis.Y;
		}

		// Set the target rotation and reset current rotation progress.
		targetRotation = GlobalBasis.Rotated(rotationAxis.Normalized(), shouldRotateOpposite ? -buttonRotateStep : buttonRotateStep).GetRotationQuaternion();
		currentRotationAmount = 0f;
	}

	// Input handling
    public override void _UnhandledInput(InputEvent inputEvent)
    {
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

		// Zoom in or out. Sets target position in local space and resets current zooming
		if (inputEvent.IsActionReleased("zoom_in_cam"))
		{
			targetPositionLocal = new Vector3(camera.Position.X, camera.Position.Y, Mathf.Max(camera.Position.Z - zoomStep, minZoom));
			currentZoomAmount = 0f;
		}
		else if (inputEvent.IsActionReleased("zoom_out_cam"))
		{
			targetPositionLocal = new Vector3(camera.Position.X, camera.Position.Y, Mathf.Min(camera.Position.Z + zoomStep, maxZoom));
			currentZoomAmount = 0f;
		}
    }
}