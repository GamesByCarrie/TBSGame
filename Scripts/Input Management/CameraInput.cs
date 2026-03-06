using Godot;

public partial class CameraInput : Node3D
{
	[ExportCategory("Rotation Settings")]
	[Export(PropertyHint.Range, "0.1, 1.0")] private float mouseSensitivity = 0.5f;
	private Vector2 mouseDirection = Vector2.Zero;
	private bool canRotate = false;
	private Node3D cameraPivot = null;

	[ExportCategory("Zoom Settings")]
	[Export(PropertyHint.Range, "0.05, 1.0")] private float zoomSpeed = 0.1f;
	[Export(PropertyHint.Range, "0.5, 2.0, suffix:m")] private float zoomStep = 1f;
	[Export(PropertyHint.None, "suffix:m")] private float minZoom = 4f;
	[Export(PropertyHint.None, "suffix:m")] private float maxZoom = 7.5f;
	private Vector3 targetPositionLocal = Vector3.Zero;
	private Camera3D camera = null;
	private float currentZoomAmount = 0f;

	// Initializing variables
    public override void _Ready()
	{
		cameraPivot = GetNode<Node3D>("%CameraPivot");
		camera = GetNode<Camera3D>("%Camera3D");
	}

	// Applying inputs
    public override void _PhysicsProcess(double delta)
	{
		// Rotate the camera
		cameraPivot.GlobalRotate(GlobalBasis.X, -mouseDirection.Y * (float)delta);
		cameraPivot.GlobalRotate(GlobalBasis.Y, -mouseDirection.X * (float)delta);

		mouseDirection = Vector2.Zero;

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


	// Input handling
    public override void _UnhandledInput(InputEvent inputEvent)
    {
		// Are we holding the rotate button?
        if (inputEvent.IsActionPressed("rotate_cam"))
		{
			canRotate = true;
		}
		else if (inputEvent.IsActionReleased("rotate_cam"))
		{
			canRotate = false;
		}

		// The camera can rotate ONLY if we are both moving the mouse and holding the rotate button
		if (inputEvent is InputEventMouseMotion && canRotate)
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