using Godot;
using System;

public partial class Entity : Node3D
{
	protected CubeletFace[] occupiedFaces = null;
	// Alternates between horizontal and vertical movement, starting with vertical
	protected int[] movementSteps = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
