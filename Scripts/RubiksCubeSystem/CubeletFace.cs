using System.Collections.Generic;
using Godot;

public partial class CubeletFace : MeshInstance3D
{
	public Cubelet cubelet;
	public Vector3I cubeletPosition;
	public CubeFaceDirection direction;
	public Transform3D faceTransform;
	private readonly Dictionary<CubeFaceDirection, CubeletFace> adjacentFaces = new()
    {
		{CubeFaceDirection.up, null},
		{CubeFaceDirection.right, null},
		{CubeFaceDirection.down, null},
		{CubeFaceDirection.left, null},
	};
	private Entity occupant = null;
	
	public void SetColor(Color color)
	{
		var material = new StandardMaterial3D();
		material.AlbedoColor = color;
		material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
		MaterialOverride = material;
	}
}
