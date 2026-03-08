using Godot;
using System;

public partial class Cubelet : Node3D
{
	public Vector3I gridPosition;
	public List<CubeFaceDirection> activeFaces = new();
	
	public void InitializeCubelet(Vector3I position){
		gridPosition = position;
		activeFaces = FindActiveFaces();
	}
	
	public void FindActiveFaces(){
		activeFaces.clear();
		int size = RubiksCube.cubeSize;
		if(gridPosition.x == 0)
			activeFaces.add(CubeFaceDirection.left);
		if(gridPosition.x == size - 1)
			activeFaces.add(CubeFaceDirection.right);
		if(gridPosition.y == 0)
			activeFaces.add(CubeFaceDirection.top);
		if(gridPosition.y == size - 1)
			activeFaces.add(CubeFaceDirection.bottom);
		if(gridPosition.z == 0)
			activeFaces.add(CubeFaceDirection.back);
		if(gridPosition.z == size - 1)
			activeFaces.add(CubeFaceDirection.front);
	}
}
