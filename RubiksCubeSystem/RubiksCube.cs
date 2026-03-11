using Godot;
using System.Collections.Generic;

public partial class RubiksCube : Node3D
{
	public const int cubeSize = 3; // CHANGE LATER TO BE BASED ON USER INPUTT
	public const float cubeletSize = 0.9f;
	public const float cubeletSpacing = 1.05f;
	public Cubelet[,,] cubelets = new Cubelet[cubeSize, cubeSize, cubeSize];
	public Dictionary<CubeFaceDirection, CubeletFace[]> faceTiles = new Dictionary<CubeFaceDirection, CubeletFace[]>();
	public List<RotationalPlane> planes = new();

	public Node3D PlaneContainer;
	public Node3D CubeletContainer;

	PackedScene CubeletScene = GD.Load<PackedScene>("res://CarrieTest/Cubelet.tscn");
	
	public override void _Ready()
	{
		PlaneContainer = GetNode<Node3D>("Planes");
		CubeletContainer = GetNode<Node3D>("Cubelets");
		CreateFaceArrays();
		CreatePlanes();
		CreateCubelets();
		CallDeferred(nameof(ColorFaces));
	}

	bool IsInside(int x, int y, int z)
	{
		return x > 0 && x < cubeSize - 1 && y > 0 && y < cubeSize - 1 && z > 0 && z < cubeSize - 1;
	}

	void CreateFaceArrays()
	{
		foreach(CubeFaceDirection dir in System.Enum.GetValues(typeof(CubeFaceDirection)))
			faceTiles[dir] = new CubeletFace[cubeSize * cubeSize];
	}

	void CreateCubelets()
	{
		for(int x = 0; x < cubeSize; x++)
		{
			for(int y = 0; y < cubeSize; y++)
			{
				for(int z = 0; z < cubeSize; z++)
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

	void ColorFaces()
	{
		foreach(Cubelet cubelet in cubelets)
		{
			if(cubelet == null)
				continue;
				
			foreach(CubeletFace cubeletFace in cubelet.activeFaces.Values)
			{
				switch (cubeletFace.direction)
				{
                    case CubeFaceDirection.up:
						cubeletFace.SetColor(Colors.Red);
						break;
					case CubeFaceDirection.down:
						cubeletFace.SetColor(Colors.Blue);
						break;
					case CubeFaceDirection.left:
						cubeletFace.SetColor(Colors.Green);
						break;
					case CubeFaceDirection.right:
						cubeletFace.SetColor(Colors.Yellow);
						break;
					case CubeFaceDirection.forward:
						cubeletFace.SetColor(Colors.Purple);
						break;
					case CubeFaceDirection.back:
						cubeletFace.SetColor(Colors.Orange);
						break;
				}
			}
		}
	}

}
