using System.Collections.Generic;
using Godot;

public partial class CubeletFace : MeshInstance3D
{
	public Cubelet cubelet;
	public Vector3I cubeletPosition;
	public CubeFaceDirection direction;
	public Transform3D faceTransform;
	public readonly Dictionary<CubeFaceDirection, CubeletFace> adjacentFaces = new()
    {
		{CubeFaceDirection.up, null},
		{CubeFaceDirection.right, null},
		{CubeFaceDirection.down, null},
		{CubeFaceDirection.left, null},
	};
	public Entity Occupant { get; set; } = null;

	/// <summary>
	/// Returns the adjacent CubeletFace that is in dir direction from this one.
	/// </summary>
	/// <param name="dir">The direction of the requested face</param>
	public CubeletFace GetAdjacentFace(CubeFaceDirection dir)
	{
		return adjacentFaces[dir];
	}

	/// <summary>
	/// Updates adjacentFaces to contain the 4 orthogonally adjacent faces to it.
	/// </summary>
	public void SetAdjacentFaces()
	{
		CubeletFace[] adjacentFaceArr = CubeFaceUtility.FindAdjacentFaces(this);

		foreach (CubeFaceDirection dir in adjacentFaces.Keys)
		{
			adjacentFaces[dir] = adjacentFaceArr[(int)dir];
		}
	}

	/// <summary>
	/// Changes this <c>CubeletFace</c>'s Material Overlay to <c>indicatorMaterial</c>.
	/// </summary>
	/// <param name="pathType">The type of indicator to use</param>
	/// <param name="indicatorMaterial">The indicator material</param>
	/// <param name="moveDirection">The direction being moved in</param>
	public void SetIndicator(PathType pathType, StandardMaterial3D indicatorMaterial, CubeFaceDirection moveDirection)
	{
		Image indicatorImage = (Image)indicatorMaterial.AlbedoTexture.GetImage().DuplicateDeep();

		// The indicator texture may need to be rotated based on the direction this CubeletFace
		// is facing and the direction the indicator moved in to arrive at this face.
		switch (pathType)
		{
			case PathType.Straight:
				switch (direction)
				{
					case CubeFaceDirection.left:
					case CubeFaceDirection.right:
						indicatorImage.Rotate90(ClockDirection.Clockwise);
						break;
				}

				switch (moveDirection)
				{
					case CubeFaceDirection.left:
					case CubeFaceDirection.right:
						indicatorImage.Rotate90(ClockDirection.Clockwise);
						break;
				}	
				break;
			case PathType.Turn:
				switch (direction)
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

				switch (moveDirection)
				{
					case CubeFaceDirection.down:
						indicatorImage.Rotate90(ClockDirection.Clockwise);
						break;
					case CubeFaceDirection.right:
						indicatorImage.Rotate90(ClockDirection.Counterclockwise);
						break;
				}
				break;
			case PathType.Fork:
				break;
			case PathType.Cross:
				break;
			case PathType.End:
				switch (direction)
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

				switch (moveDirection)
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
				break;
		}

		indicatorMaterial.AlbedoTexture = ImageTexture.CreateFromImage(indicatorImage);
		MaterialOverlay = indicatorMaterial;
	}

	/// <summary>
	/// An overload for <c>SetIndicator</c> used for a final adjustment to <c>PathType.Turn</c> indicators.
	/// </summary>
	/// <param name="indicatorMaterial">The indicator material</param>
	/// <param name="moveDirection">The direction being moved in</param>
	/// <param name="prevDirection">The direction that was moved in to arrive at this face</param>
	public void SetIndicator(StandardMaterial3D indicatorMaterial, CubeFaceDirection moveDirection, CubeFaceDirection prevDirection)
	{
		Image indicatorImage = (Image)indicatorMaterial.AlbedoTexture.GetImage().DuplicateDeep();

		switch (moveDirection)
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
		MaterialOverlay = indicatorMaterial;
	}

	public void ColorFace()
	{
		switch (direction)
		{
			case CubeFaceDirection.up:
				SetColor(Colors.Red);
				break;
			case CubeFaceDirection.down:
				SetColor(Colors.Blue);
				break;
			case CubeFaceDirection.left:
				SetColor(Colors.Green);
				break;
			case CubeFaceDirection.right:
				SetColor(Colors.Yellow);
				break;
			case CubeFaceDirection.forward:
				SetColor(Colors.Purple);
				break;
			case CubeFaceDirection.back:
				SetColor(Colors.Orange);
				break;
		}
	}
	
	private void SetColor(Color color)
	{
		var material = new StandardMaterial3D();
		material.AlbedoColor = color;
		material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
		MaterialOverride = material;
	}
}
