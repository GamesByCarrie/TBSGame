using System.Collections.Generic;
using Godot;

public static class CubeFaceUtility
{
    // The innermost directions directly correlate with the order faces are constructed in CreateFaceArrays()
    private static Dictionary<CubeFaceDirection, Dictionary<CubeFaceDirection, CubeFaceDirection>> cubeFaceAdjacencies = new()
    {
        { 
            CubeFaceDirection.up,
            new()
            {
                {CubeFaceDirection.left, CubeFaceDirection.left},
                {CubeFaceDirection.right, CubeFaceDirection.right},
                {CubeFaceDirection.forward, CubeFaceDirection.down},
                {CubeFaceDirection.back, CubeFaceDirection.up},
            }
        },
        { 
            CubeFaceDirection.down,
            new()
            {
                {CubeFaceDirection.left, CubeFaceDirection.left},
                {CubeFaceDirection.right, CubeFaceDirection.right},
                {CubeFaceDirection.forward, CubeFaceDirection.up},
                {CubeFaceDirection.back, CubeFaceDirection.down},
            }
        },
        { 
            CubeFaceDirection.left,
            new()
            {
                {CubeFaceDirection.up, CubeFaceDirection.up},
                {CubeFaceDirection.down, CubeFaceDirection.down},
                {CubeFaceDirection.forward, CubeFaceDirection.right},
                {CubeFaceDirection.back, CubeFaceDirection.left},
            }
        },
        { 
            CubeFaceDirection.right,
            new()
            {
                {CubeFaceDirection.up, CubeFaceDirection.up},
                {CubeFaceDirection.down, CubeFaceDirection.down},
                {CubeFaceDirection.forward, CubeFaceDirection.left},
                {CubeFaceDirection.back, CubeFaceDirection.right},
            }
        },
        { 
            CubeFaceDirection.forward,
            new()
            {
                {CubeFaceDirection.up, CubeFaceDirection.up},
                {CubeFaceDirection.down, CubeFaceDirection.down},
                {CubeFaceDirection.left, CubeFaceDirection.left},
                {CubeFaceDirection.right, CubeFaceDirection.right},
            }
        },
        { 
            CubeFaceDirection.back,
            new()
            {
                {CubeFaceDirection.up, CubeFaceDirection.up},
                {CubeFaceDirection.down, CubeFaceDirection.down},
                {CubeFaceDirection.left, CubeFaceDirection.right},
                {CubeFaceDirection.right, CubeFaceDirection.left},
            }
        },
    };

    public static Transform3D GetFaceTransform(CubeFaceDirection dir)
    {
        Vector3 normal = GetDirectionVector(dir);
        Basis basis = GetFaceBasis(dir);
        Vector3 offset = normal * 0.5f;
        return new Transform3D(basis, offset);
    }

    private static Vector3 GetDirectionVector(CubeFaceDirection dir)
    {
        return dir switch
        {
            CubeFaceDirection.up => Vector3.Up,
            CubeFaceDirection.down => Vector3.Down,
            CubeFaceDirection.left => Vector3.Left,
            CubeFaceDirection.right => Vector3.Right,
            CubeFaceDirection.forward => Vector3.Forward,
            CubeFaceDirection.back => Vector3.Back,
            _ => Vector3.Zero
        };
    }

    private static Basis GetFaceBasis(CubeFaceDirection dir)
    {
        return dir switch
        {
            CubeFaceDirection.up => Basis.Identity,
            CubeFaceDirection.down => Basis.FromEuler(new Vector3(Mathf.Pi, 0, 0)),
            CubeFaceDirection.left => Basis.FromEuler(new Vector3(0, 0, Mathf.Pi / 2)),
            CubeFaceDirection.right => Basis.FromEuler(new Vector3(0, 0, -Mathf.Pi / 2)),
            CubeFaceDirection.forward => Basis.FromEuler(new Vector3(-Mathf.Pi / 2, 0, 0)),
            CubeFaceDirection.back => Basis.FromEuler(new Vector3(Mathf.Pi / 2, 0, 0)),
            _ => Basis.Identity
        };
    }

    /// <summary>
    /// Returns the 4 faces that are orthogonally adjacent to cubeletFace.
    /// </summary>
    public static CubeletFace[] FindAdjacentFaces(CubeletFace cubeletFace)
    {
        CubeletFace[] adjacentFaces = new CubeletFace[4];
        List<Cubelet> currentCubeFace = RubiksCube.cubeletsByFace[cubeletFace.direction];
        int positionIndex = currentCubeFace.IndexOf(cubeletFace.cubelet);
        Vector2I positionCoordinate = new Vector2I(positionIndex % RubiksCube.cubeSize, positionIndex / RubiksCube.cubeSize);
        CubeFaceDirection direction = CubeFaceDirection.down;

        // Cubelet faces that are on the same cube face
        for (int i = -1; i <= 1; i++)
        {
            if (positionCoordinate.Y + i < 0 || positionCoordinate.Y + i >= RubiksCube.cubeSize)
            {
                continue;
            }

            for (int j = -1; j <= 1; j++)
            {
                if (Mathf.Abs(i) == Mathf.Abs(j) || positionCoordinate.X + j < 0 || positionCoordinate.X + j >= RubiksCube.cubeSize)
                {
                    continue;
                }

                if (i == 0)
                {
                    if (j == -1)
                    {
                        direction = CubeFaceDirection.left;
                    }
                    else
                    {
                        direction = CubeFaceDirection.right;
                    }
                }
                else if (i == 1)
                {
                    direction = CubeFaceDirection.up;
                }

                adjacentFaces[(int)direction] = currentCubeFace[positionIndex + i * RubiksCube.cubeSize + j].activeFaces[cubeletFace.direction];
            }
        }

        // Cubelet faces that go over the edge to a different cube face
        foreach (CubeletFace face in cubeletFace.cubelet.activeFaces.Values)
        {
            if (face == cubeletFace) continue;

            adjacentFaces[(int)cubeFaceAdjacencies[cubeletFace.direction][face.direction]] = face;
        }

        return [..adjacentFaces];
    }
}