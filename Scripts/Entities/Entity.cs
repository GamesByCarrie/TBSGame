using Godot;
using System.Collections.Generic;

public partial class Entity : Node3D
{
	[Export] protected Image moveLineImage = null;
	[Export] protected Image moveElbowImage = null;
	[Export] protected Image moveEndImage = null;
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
		//movementSteps = [2, 1, -2, -2];
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
	/// Using <c>movementSteps</c>, calculates the entity's
	/// full movement path starting in the 4 cardinal directions.
	/// </summary>
	private void CalculateMoves()
	{
		for (int i = 0; i < adjacentFaces.Length; i++)
		{
			CubeFaceDirection direction = (CubeFaceDirection)i;
			CubeFaceDirection prevDirection = direction;
			CubeletFace adjacentFace = occupiedFace;
			StandardMaterial3D indicatorMaterial = null;
			Image indicatorImage = null;

			for (int j = 0; j < movementSteps.Length; j++)
			{
				int moveAmt = movementSteps[j];
				if (moveAmt < 0)	
				{
					moveAmt *= -1;
					direction = FlipMoveDirection(direction);
					if (prevDirection == CubeFaceDirection.right || prevDirection == CubeFaceDirection.left)
					{
						prevDirection = FlipMoveDirection(prevDirection);
					}
				}

				if (adjacentFace.direction == CubeFaceDirection.back)
				{
					direction = FlipMoveDirection(direction);
					if (prevDirection == CubeFaceDirection.up || prevDirection == CubeFaceDirection.down)
					{
						prevDirection = FlipMoveDirection(prevDirection);
					}
				}

				if (j > 0)
				{
					switch (direction)
					{
						case CubeFaceDirection.up:
							if (prevDirection == CubeFaceDirection.left)
							{
								indicatorImage.Rotate90(ClockDirection.Clockwise);
							}
							else
							{
								indicatorImage.Rotate90(ClockDirection.Counterclockwise);
							}
							break;
						case CubeFaceDirection.left:
							if (prevDirection == CubeFaceDirection.down)
							{
								indicatorImage.Rotate90(ClockDirection.Clockwise);
							}
							else
							{
								indicatorImage.Rotate90(ClockDirection.Counterclockwise);
							}
							break;
					}

					indicatorMaterial.AlbedoTexture = ImageTexture.CreateFromImage(indicatorImage);
					adjacentFace.SetIndicator(indicatorMaterial);
				}

                for (int k = 0; k < moveAmt; k++)
				{
					CubeletFace prevFace = adjacentFace;
					adjacentFace = adjacentFace.GetAdjacentFace(direction);

					if (prevFace.cubelet == adjacentFace.cubelet)
					{
						direction = CalculateRotatedDirection(prevFace.direction, direction);
					}

					if (k < moveAmt - 1)
					{
						indicatorMaterial = (StandardMaterial3D)moveLineMaterial.DuplicateDeep();
						indicatorImage = (Image)indicatorMaterial.AlbedoTexture.GetImage().DuplicateDeep();

						switch (adjacentFace.direction)
						{
							case CubeFaceDirection.left:
							case CubeFaceDirection.right:
								indicatorImage.Rotate90(ClockDirection.Clockwise);
								break;
						}

						switch (direction)
						{
							case CubeFaceDirection.left:
							case CubeFaceDirection.right:
								indicatorImage.Rotate90(ClockDirection.Clockwise);
								break;
						}	
					}
					else if (j == movementSteps.Length - 1)
					{
						indicatorMaterial = (StandardMaterial3D)moveEndMaterial.DuplicateDeep();
						indicatorImage = (Image)indicatorMaterial.AlbedoTexture.GetImage().DuplicateDeep();

						switch (adjacentFace.direction)
						{
							case CubeFaceDirection.back:
								indicatorImage.Rotate180();
								break;
							case CubeFaceDirection.left:
								indicatorImage.Rotate90(ClockDirection.Counterclockwise);
								break;
							case CubeFaceDirection.right:
								indicatorImage.Rotate90(ClockDirection.Clockwise);
								break;
						}

						switch (direction)
						{
							case CubeFaceDirection.up:
								indicatorImage.Rotate180();
								break;
							case CubeFaceDirection.left:
								indicatorImage.Rotate90(ClockDirection.Counterclockwise);
								break;
							case CubeFaceDirection.right:
								indicatorImage.Rotate90(ClockDirection.Clockwise);
								break;
						}
					}
					else
					{
						indicatorMaterial = (StandardMaterial3D)moveElbowMaterial.DuplicateDeep();
						indicatorImage = (Image)indicatorMaterial.AlbedoTexture.GetImage().DuplicateDeep();

						switch (adjacentFace.direction)
						{
							case CubeFaceDirection.back:
								indicatorImage.Rotate90(ClockDirection.Counterclockwise);
								break;
							case CubeFaceDirection.left:
								indicatorImage.Rotate90(ClockDirection.Counterclockwise);
								break;
							case CubeFaceDirection.right:
								indicatorImage.Rotate90(ClockDirection.Clockwise);
								break;
						}

						switch (direction)
						{
							case CubeFaceDirection.down:
								indicatorImage.Rotate90(ClockDirection.Clockwise);
								break;
							case CubeFaceDirection.right:
								indicatorImage.Rotate90(ClockDirection.Counterclockwise);
								break;
						}
					}

					indicatorMaterial.AlbedoTexture = ImageTexture.CreateFromImage(indicatorImage);
					adjacentFace.SetIndicator(indicatorMaterial);
				}

				prevDirection = direction;
				direction = direction switch
                {
                    CubeFaceDirection.up => CubeFaceDirection.right,
                    CubeFaceDirection.right => CubeFaceDirection.down,
                    CubeFaceDirection.down => CubeFaceDirection.left,
                    _ => CubeFaceDirection.up,
                };
			}

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
