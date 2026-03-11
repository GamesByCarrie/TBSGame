using Godot;
using System;
using System.Collections.Generic;

public partial class Cubelet : Node3D
{
	public Vector3I gridPosition;
	public Dictionary<CubeFaceDirection, CubeletFace> activeFaces = new();
	Node3D cubeletFaceContainer;
	PackedScene cubeletFaceScene = GD.Load<PackedScene>("res://Levels/CarrieTest/CubeletFace.tscn");

    public override void _Ready()
    {
        cubeletFaceContainer = GetNode<Node3D>("CubeletFaces");
    }
	
	public void InitializeCubelet(Vector3I position){
		gridPosition = position;
		SpawnActiveFaces();
	}

	void CreateFace(CubeFaceDirection dir)
	{
		CubeletFace cubeletFace = cubeletFaceScene.Instantiate<CubeletFace>();
		cubeletFaceContainer.AddChild(cubeletFace);
		cubeletFace.direction = dir;
		cubeletFace.cubelet = this;
		cubeletFace.Transform = CubeFaceUtility.GetFaceTransform(dir);
		activeFaces[dir] = cubeletFace;
	}
	
	public void SpawnActiveFaces(){
		activeFaces.Clear();
		int size = RubiksCube.cubeSize;

		if(gridPosition.X == 0)
			CreateFace(CubeFaceDirection.left);
		if(gridPosition.X == size - 1)
			CreateFace(CubeFaceDirection.right);
		if(gridPosition.Y == 0)
			CreateFace(CubeFaceDirection.down);
		if(gridPosition.Y == size - 1)
			CreateFace(CubeFaceDirection.up);
		if(gridPosition.Z == 0)
			CreateFace(CubeFaceDirection.forward);
		if(gridPosition.Z == size - 1)
			CreateFace(CubeFaceDirection.back);
	}
}
