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
		//movementSteps = [3, -1, 1, -1, 2];
		movementSteps = [3, 2];
		CalculateMoves();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Helper that flips the given direction to its opposite.
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

	private void CalculateMoves()
	{
		for (int i = 0; i < adjacentFaces.Length; i++)
		{
			CubeFaceDirection direction = (CubeFaceDirection)i;
			CubeletFace adjacentFace = occupiedFace;

			foreach (int move in movementSteps)
			{
				int moveAmt = move;
				if (move < 0)	
				{
					moveAmt *= -1;
					direction = FlipMoveDirection(direction);
				}

				if (adjacentFace.direction == CubeFaceDirection.back)
				{
					direction = FlipMoveDirection(direction);
				}

				if (move != movementSteps[0])
				{
					StandardMaterial3D indicatorMaterial = (StandardMaterial3D)moveElbowMaterial.DuplicateDeep();
					Image indicatorImage = (Image)indicatorMaterial.AlbedoTexture.GetImage().DuplicateDeep();
					switch (direction)
					{
						case CubeFaceDirection.down:
							indicatorImage.FlipY();
							break;
						case CubeFaceDirection.left:
							indicatorImage.FlipX();
							break;
					}
					indicatorMaterial.AlbedoTexture = ImageTexture.CreateFromImage(indicatorImage);
					adjacentFace.SetIndicator(indicatorMaterial);
				}

                for (int j = 0; j < moveAmt; j++)
				{
					CubeletFace prevFace = adjacentFace;
					adjacentFace = adjacentFace.GetAdjacentFace(direction);
					StandardMaterial3D indicatorMaterial = null;
					Image indicatorImage = null;

					if (j < moveAmt - 1)
					{
						indicatorMaterial = (StandardMaterial3D)moveLineMaterial.DuplicateDeep();
						indicatorImage = (Image)indicatorMaterial.AlbedoTexture.GetImage().DuplicateDeep();
						switch (direction)
						{
							case CubeFaceDirection.left:
							case CubeFaceDirection.right:
								indicatorImage.Rotate90(ClockDirection.Clockwise);
								break;
						}	
					}
					else if (move == movementSteps[^1])
					{
						indicatorMaterial = (StandardMaterial3D)moveEndMaterial.DuplicateDeep();
						indicatorImage = (Image)indicatorMaterial.AlbedoTexture.GetImage().DuplicateDeep();
						switch (direction)
						{
							case CubeFaceDirection.down:
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
						switch (direction)
						{
							case CubeFaceDirection.up:
								indicatorImage.FlipY();
								break;
							case CubeFaceDirection.left:
								indicatorImage.FlipX();
								break;
						}
					}

					indicatorMaterial.AlbedoTexture = ImageTexture.CreateFromImage(indicatorImage);
					adjacentFace.SetIndicator(indicatorMaterial);

					if (prevFace.cubelet == adjacentFace.cubelet)
					{
						direction = CalculateRotatedDirection(prevFace.direction, direction);
					}
				}

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
	/// Returns a new direction if a movement from the face pointing
	/// in startDir direction in the direction of moveDir would result
	/// in a change in local direction.
	/// </summary>
	/// <param name="startDir">The direction the original face is facing</param>
	/// <param name="moveDir">The direction of the movement</param>
	/// <returns>A new direction representing the corresponding rotation
	/// if the local direction would change.Otherwise, the original
	/// movement direction.</returns>
	private CubeFaceDirection CalculateRotatedDirection(CubeFaceDirection startDir, CubeFaceDirection moveDir)
	{
		if (moveDir == CubeFaceDirection.right || moveDir == CubeFaceDirection.left)
		{
			if (startDir == CubeFaceDirection.up)
			{
				return CubeFaceDirection.down;
			}
			else if (startDir == CubeFaceDirection.down)
			{
				return CubeFaceDirection.up;
			}
		}
		else
		{
			if (startDir == CubeFaceDirection.right)
			{
				return CubeFaceDirection.left;
			}
			else if (startDir == CubeFaceDirection.left)
			{
				return CubeFaceDirection.right;
			}
		}

		return moveDir;
	}

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
