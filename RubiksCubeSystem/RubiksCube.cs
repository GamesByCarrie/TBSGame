using Godot;
using System;

public partial class RubiksCube : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		public const int cubeSize = 3; // CHANGE LATER TO BE BASED ON USER INPUTT
		public Cubelet[,,] cubelets = new Cubelet[cubeSize, cubeSize, cubeSize];
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
