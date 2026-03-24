using Godot;

public partial class Entity : Node3D
{
	[Export] protected StandardMaterial3D moveLineMaterial = null;
	[Export] protected StandardMaterial3D moveElbowMaterial = null;
	[Export] protected StandardMaterial3D moveEndMaterial = null;
	protected CubeletFace occupiedFace = null;
	protected CubeletFace[] adjacentFaces = new CubeletFace[4];
	protected int[] movementSteps = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		occupiedFace = RubiksCube.cubeletsByFace[CubeFaceDirection.up][4].activeFaces[CubeFaceDirection.up];
		GlobalPosition = occupiedFace.GlobalPosition;
		movementSteps = [3, -1, 1, -1, 2];
		CalculateMoves();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Flips <c>direction</c> to its opposite.
	/// </summary>
	private CubeFaceDirection FlipMoveDirection(CubeFaceDirection direction)
	{
		return direction switch
		{
			CubeFaceDirection.up => CubeFaceDirection.down,
			CubeFaceDirection.right => CubeFaceDirection.left,
			CubeFaceDirection.down => CubeFaceDirection.up,
			_ => CubeFaceDirection.right,
		};
	}

	/// <summary>
	/// Using <c>Entity.movementSteps</c>, calculates the Entity's
	/// full movement path starting in the 4 cardinal directions.
	/// </summary>
	private void CalculateMoves()
	{
		// Calculate a path for each direction the Entity can move
		for (int i = 0; i < adjacentFaces.Length; i++)
		{
			CubeFaceDirection direction = (CubeFaceDirection)i;
			CubeFaceDirection prevDirection = direction;
			CubeletFace adjacentFace = occupiedFace;
			StandardMaterial3D indicatorMaterial = null;
			PathType pathType = PathType.Straight;

			// The full movement is broken into directed steps
			for (int j = 0; j < movementSteps.Length; j++)
			{
				int moveAmt = movementSteps[j];
				// Negative direction means we flip the direction
				if (moveAmt < 0)	
				{
					moveAmt *= -1;
					direction = FlipMoveDirection(direction);
					if (prevDirection == CubeFaceDirection.right || prevDirection == CubeFaceDirection.left)
					{
						prevDirection = FlipMoveDirection(prevDirection);
					}
				}

				// The cube's back face is traversed backwards because of its orientation
				if (adjacentFace.direction == CubeFaceDirection.back)
				{
					direction = FlipMoveDirection(direction);
					if (prevDirection == CubeFaceDirection.up || prevDirection == CubeFaceDirection.down)
					{
						prevDirection = FlipMoveDirection(prevDirection);
					}
				}

				// Final adjustment that is made for 'PathType.Turn' segments
				if (j > 0)
				{
					adjacentFace.SetIndicator(indicatorMaterial, direction, prevDirection);
				}

				// Each step of the full movement can be a different length
                for (int k = 0; k < moveAmt; k++)
				{
					CubeletFace prevFace = adjacentFace;
					adjacentFace = adjacentFace.GetAdjacentFace(direction);

					// When moving to a new cube face, the local movement direction may change
					if (prevFace.cubelet == adjacentFace.cubelet)
					{
						direction = CalculateRotatedDirection(prevFace.direction, direction);
					}

					// What type of movement is occurring at this step?
					if (k < moveAmt - 1)
					{
						pathType = PathType.Straight;
						indicatorMaterial = (StandardMaterial3D)moveLineMaterial.DuplicateDeep();
					}
					else if (j == movementSteps.Length - 1)
					{
						pathType = PathType.End;
						indicatorMaterial = (StandardMaterial3D)moveEndMaterial.DuplicateDeep();
					}
					else
					{
						pathType = PathType.Turn;
						indicatorMaterial = (StandardMaterial3D)moveElbowMaterial.DuplicateDeep();
					}

					adjacentFace.SetIndicator(pathType, indicatorMaterial, direction);
				}

				// Turning to face the next movement direction
				prevDirection = direction;
				direction = direction switch
                {
                    CubeFaceDirection.up => CubeFaceDirection.right,
                    CubeFaceDirection.right => CubeFaceDirection.down,
                    CubeFaceDirection.down => CubeFaceDirection.left,
                    _ => CubeFaceDirection.up,
                };
			}

			// The endpoint of this calculation is considered 'adjacent' to the Entity
			adjacentFaces[i] = adjacentFace;
		}
	}

	/// <summary>
	/// Returns the new movement direction that results from moving in <c>moveDirection</c>
	/// onto a face whose normal vector does not point in <c>startDirection</c>.
	/// </summary>
	/// <param name="startDirection">The direction the original face is facing</param>
	/// <param name="moveDirection">The direction of the movement</param>
	private CubeFaceDirection CalculateRotatedDirection(CubeFaceDirection startDirection, CubeFaceDirection moveDirection)
	{
		// Moving horizontally from the up or down faces onto the left or right
		// faces changes the local movement direction to vertical
		if (moveDirection == CubeFaceDirection.right || moveDirection == CubeFaceDirection.left)
		{
			if (startDirection == CubeFaceDirection.up)
			{
				return CubeFaceDirection.down;
			}
			else if (startDirection == CubeFaceDirection.down)
			{
				return CubeFaceDirection.up;
			}
		}
		// Moving vertically from the left or right faces onto the up or down
		// faces changes the local movement direction to horizontal
		else
		{
			if (startDirection == CubeFaceDirection.right)
			{
				return CubeFaceDirection.left;
			}
			else if (startDirection == CubeFaceDirection.left)
			{
				return CubeFaceDirection.right;
			}
		}

		return moveDirection;
	}

	/// <summary>
	/// Temp function to manually move a character around the screen.
	/// This will be removed once the first playable character is implemented.
	/// </summary>
    public override void _UnhandledKeyInput(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("move_up"))
		{
			occupiedFace = occupiedFace.GetAdjacentFace(CubeFaceDirection.up);
			GlobalPosition = occupiedFace.GlobalPosition;
		}
		else if (inputEvent.IsActionPressed("move_down"))
		{
			occupiedFace = occupiedFace.GetAdjacentFace(CubeFaceDirection.down);
			GlobalPosition = occupiedFace.GlobalPosition;
		}
		else if (inputEvent.IsActionPressed("move_left"))
		{
			occupiedFace = occupiedFace.GetAdjacentFace(CubeFaceDirection.right);
			GlobalPosition = occupiedFace.GlobalPosition;
		}
		else if (inputEvent.IsActionPressed("move_right"))
		{
			occupiedFace = occupiedFace.GetAdjacentFace(CubeFaceDirection.left);
			GlobalPosition = occupiedFace.GlobalPosition;
		}
    }
}
