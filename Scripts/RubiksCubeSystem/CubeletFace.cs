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

	public void SetIndicator(Material indicatorMaterial)
	{
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
