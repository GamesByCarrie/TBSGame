using Godot;
using System.Collections.Generic;

public partial class RubiksCube : Node3D
{
	public const int cubeSize = 3; // CHANGE LATER TO BE BASED ON USER INPUTT
	public const float cubeletSize = 0.9f;
	public const float cubeletSpacing = 1.05f;
	public Cubelet[,,] cubelets = new Cubelet[cubeSize, cubeSize, cubeSize];
	public static Dictionary<CubeFaceDirection, List<Cubelet>> cubeletsByFace = new Dictionary<CubeFaceDirection, List<Cubelet>>();
	public List<RotationalPlane> planes = new();

	public Node3D PlaneContainer;
	public Node3D CubeletContainer;

	PackedScene CubeletScene = GD.Load<PackedScene>("res://Levels/CarrieTest/Cubelet.tscn");

	private PackedScene testAgentScene = GD.Load<PackedScene>("res://Prefabs/TEST_AGENT.tscn");
	
	public override void _Ready()
	{
		PlaneContainer = GetNode<Node3D>("CubeMesh/Planes");
		CubeletContainer = GetNode<Node3D>("CubeMesh/Cubelets");
		CreateCubelets();
		CreateFaceArrays();
		CreatePlanes();
		CallDeferred(nameof(InitCubeletFaces));
	}

	bool IsInside(int x, int y, int z)
	{
		return x > 0 && x < cubeSize - 1 && y > 0 && y < cubeSize - 1 && z > 0 && z < cubeSize - 1;
	}

	/// <summary>
	/// Populates <c>cubeletsByFace</c> with lists of <c>Cubelets</c> corresponding
	/// to each face of the greater cube.
	/// <para>
	/// <b>Note:</b> Each Cubelet List is ordered and is directly connected to the
	/// <c>cubeFaceAdjacencies</c> Dictionary. Any changes to the structure of any of
	/// the Lists must be reflected in the corresponding location in the Dictionary.
	/// </para>
	/// </summary>
	void CreateFaceArrays()
	{
		foreach(CubeFaceDirection dir in System.Enum.GetValues(typeof(CubeFaceDirection)))
		{
			cubeletsByFace[dir] = new List<Cubelet>(cubeSize * cubeSize);

			int xStart = 0;
			int xEnd = cubeSize;
			int xStep = 1;
			int yStart = 0;
			int yEnd = cubeSize;
			int yStep = 1;
			int zStart = 0;
			int zEnd = cubeSize;
			int zStep = 1;
			switch (dir)
			{
				case CubeFaceDirection.up:
					yStart = cubeSize - 1;
					break;
				case CubeFaceDirection.down:
					yEnd = 1;
					zStart = cubeSize - 1;
					zEnd = -1;
					zStep = -1;
					break;
				case CubeFaceDirection.left:
					zEnd = 1;
					xStart = cubeSize - 1;
					xEnd = -1;
					xStep = -1;
					break;
				case CubeFaceDirection.right:
					zStart = cubeSize - 1;
					break;
				case CubeFaceDirection.forward:
					zEnd = 1;
					break;
				default:
					zStart = cubeSize - 1;
					xStart = cubeSize - 1;
					xEnd = -1;
					xStep = -1;
					break;
			}

			for (int z = zStart; z != zEnd; z += zStep)
			{
				for (int y = yStart; y != yEnd; y += yStep)
				{
					for (int x = xStart; x != xEnd; x += xStep)
					{
						bool leftOrRight = dir == CubeFaceDirection.left || dir == CubeFaceDirection.right;
						Cubelet cubelet = leftOrRight ? cubelets[z, y, x] : cubelets[x, y, z];

						cubeletsByFace[dir].Add(cubelet);
					}
				}
			}
		}
	}

	void CreateCubelets()
	{
		for(int z = 0; z < cubeSize; z++)
		{
			for(int y = 0; y < cubeSize; y++)
			{
				for(int x = 0; x < cubeSize; x++)
				{
					if(IsInside(x, y, z))
						continue;
					
					Cubelet cubelet = CubeletScene.Instantiate<Cubelet>();
					CubeletContainer.AddChild(cubelet);

					Vector3I position = new Vector3I(x, y, z);
					cubelet.InitializeCubelet(position);
					Vector3 centeredCubelet = new Vector3(x - (cubeSize - 1) / 2.0f, y - (cubeSize - 1) / 2.0f, z - (cubeSize - 1) / 2.0f);
					cubelet.Position = centeredCubelet * cubeletSpacing;

					cubelets[x, y, z] = cubelet;
				}
			}
		}
	}

	void CreatePlanes()
	{
		for(int i = 0; i < cubeSize; i++)
		{
			CreatePlane(Vector3.Right, i);
			CreatePlane(Vector3.Up, i);
			CreatePlane(Vector3.Forward, i);
		}
	}

	void CreatePlane(Vector3 axis, int layer)
	{
		RotationalPlane plane = new RotationalPlane();

		plane.axis = axis;
		plane.layerIndex = layer;
		plane.parentCube = this;

		PlaneContainer.AddChild(plane);
	}

	/// <summary>
	/// Performs some setup on all cubelet faces.
	/// </summary>
	private void InitCubeletFaces()
	{
		foreach(Cubelet cubelet in cubelets)
		{
			if (cubelet == null) continue;

			foreach(CubeletFace cubeletFace in cubelet.activeFaces.Values)
			{
				cubeletFace.ColorFace();
				cubeletFace.SetAdjacentFaces();
			}
		}

		Entity entity = testAgentScene.Instantiate<Entity>();
		GetNode<Node3D>("CubeMesh").AddChild(entity);
	}
}
