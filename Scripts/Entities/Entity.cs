using Godot;

public abstract partial class Entity : Node3D
{
	[Export] protected bool isPlayerControlled = false;
	[Export] protected int maxHealth = 0;
	/// <summary>
	/// If damage is negative, the target heals that much.
	/// </summary>
	[Export] protected int damage = 0;
	/// <summary>
	/// Higher initiative moves earlier in the turn order.
	/// </summary>
	[Export] protected int initiative = 0;

	/// <summary>
	/// Lower turn order moves first, starting with 0.
	/// </summary>
	protected uint turnOrder = 0;
	protected int currentHealth = 0;
	protected CubeletFace occupiedFace = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		occupiedFace = RubiksCube.cubeletsByFace[CubeFaceDirection.up][4].activeFaces[CubeFaceDirection.up];
		GlobalPosition = occupiedFace.GlobalPosition;
		currentHealth = maxHealth;
	}

	/// <summary>
	/// Performs this entity's actions for the turn.
	/// </summary>
	protected abstract void Act();
}
