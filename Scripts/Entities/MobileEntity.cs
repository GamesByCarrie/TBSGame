using Godot;
using System;
using System.Threading.Tasks;

public abstract partial class MobileEntity : Entity
{
	[Export] protected StandardMaterial3D moveLineMaterial = null;
	[Export] protected StandardMaterial3D moveElbowMaterial = null;
	[Export] protected StandardMaterial3D moveEndMaterial = null;

	/// <summary>
	/// The steps required to move to the correct destination.
	/// After a complete step, the movement rotates 90 degrees clockwise.
	/// Movement can be negative to rotate counter-clockwise.
	/// However, the first step should never be negative.
	/// </summary>
	protected int[] movementSteps = null;
	private int pathLength = 0;
	protected CubeletFace[][] movementPaths = null;
	private int numStartDirections = 4;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

		movementPaths = new CubeletFace[numStartDirections][];
		movementSteps = [3, -1, 1, -1, 2];
		foreach (int step in movementSteps)
		{
			pathLength += Mathf.Abs(step);
		}
	}

    public override void _Process(double delta)
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();

		if (turnComplete)
		{
			turnComplete = false;
			CalculateMoves();
			Move(movementPaths[rng.RandiRange(0, numStartDirections - 1)]);
		}
    }

	/// <summary>
	/// Moves the Entity from <c>occupiedFace</c> to the <c>CubeletFace</c> at the
	/// end of <c>path</c>.
	/// </summary>
	/// <param name="path">The path to follow to reach the destination</param>
	protected void Move(CubeletFace[] path)
	{
		// Begin some async subroutine to move along the path
		TweenPath(path);
	}

	/// <summary>
	/// Tweens the Entity across its entire path.
	/// </summary>
	private async Task TweenPath(CubeletFace[] path)
	{
		foreach(CubeletFace step in path)
		{
			occupiedFace = step;

			Tween moveTween = CreateTween();
			moveTween.TweenProperty(this, "position", occupiedFace.cubelet.Position + occupiedFace.Position, 0.5f);

			await ToSignal(moveTween, Tween.SignalName.Finished);
		}

		turnComplete = true;
	}


	/// <summary>
	/// Using <c>Entity.movementSteps</c>, calculates the Entity's
	/// full movement path starting in the 4 cardinal directions.
	/// </summary>
	protected virtual void CalculateMoves()
	{
		// TEMP SOLUTION TO CLEARING OUT OLD INDICATORS
		// IN THE FUTURE THIS WILL BE HANDLED BY A TURN
		// MANAGER OR SOME SIMILAR MANAGER CLASS
		foreach(CubeletFace[] path in movementPaths)
		{
			if (path == null) continue;

			foreach(CubeletFace face in path)
			{
				face.MaterialOverlay = null;
			}
		}

		// Calculate a path for each direction the Entity can move
		for (int i = 0; i < numStartDirections; i++)
		{
			CubeFaceDirection direction = (CubeFaceDirection)i;
			CubeFaceDirection prevDirection = direction;
			CubeletFace adjacentFace = occupiedFace;
			StandardMaterial3D indicatorMaterial = null;
			PathType pathType = PathType.Straight;
			int totalMovementPerformed = 0;
			movementPaths[i] = new CubeletFace[pathLength];

			// The full movement is broken into directed steps
			for (int j = 0; j < movementSteps.Length; j++)
			{
				int moveAmt = movementSteps[j];

				if (j > 0)
				{
					// Negative direction means we flip the direction
					if (moveAmt < 0)	
					{
						moveAmt *= -1;
						direction = FlipMoveDirection(direction);
					}

					// Final adjustment that is made for 'PathType.Turn' segments
					adjacentFace.SetIndicator(indicatorMaterial, direction, prevDirection);
				}

				// Each step of the full movement can be a different length
                for (int k = 0; k < moveAmt; k++)
				{
					CubeletFace prevFace = adjacentFace;
					adjacentFace = adjacentFace.GetAdjacentFace(direction);
					movementPaths[i][k + totalMovementPerformed] = adjacentFace;

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

				totalMovementPerformed += moveAmt;

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
		}
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
		// faces changes the local movement direction to horizontal. Moving
		// vertically from the up or down faces onto the back face or vice
		// versa reverses the local movement direction.
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
			else if (startDirection == CubeFaceDirection.back || startDirection == moveDirection)
			{
				return FlipMoveDirection(moveDirection);
			}
		}

		return moveDirection;
	}
}
