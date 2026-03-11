using System.Collections.Generic;
using Godot;

public static class CubeFaceUtility
{
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
            CubeFaceDirection.forward => Basis.FromEuler(new Vector3(Mathf.Pi / 2, 0, 0)),
            CubeFaceDirection.back => Basis.FromEuler(new Vector3(-Mathf.Pi / 2, 0, 0)),
            _ => Basis.Identity
        };
    }

    /// <summary>
    /// Returns the 4 faces that are cardinally adjacent to currentCubeletFace.
    /// This function will have to be updated to support more types of movement
    /// than just 1 space in the cardinal directions.
    /// </summary>
    /// <param name="currentCubeletFace">The currently occupied CubeletFace</param>
    /// <param name="currentCubeFace">The set of Cubelets on the currently occupied face of the Rubik's Cube</param>
    /// <param name="cubeSize">The dimension of the Rubik's Cube</param>
    public static List<CubeletFace> GetAdjacentFaces(CubeletFace currentCubeletFace, List<Cubelet> currentCubeFace, int cubeSize)
    {
        List<CubeletFace> adjacentFaces = new(4);
        int positionIndex = currentCubeFace.IndexOf(currentCubeletFace.cubelet);
        Vector2 positionCoordinate = new Vector2(positionIndex % cubeSize, positionIndex / cubeSize);

        // Cubelet faces that go over the edge to a different cube face
        foreach (CubeletFace face in currentCubeletFace.cubelet.activeFaces.Values)
        {
            if (face == currentCubeletFace) continue;

            adjacentFaces.Add(face);
        }

        // Cubelet faces that are on the same cube face
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (Mathf.Abs(i) == Mathf.Abs(j) || positionCoordinate.Y + i < 0 || positionCoordinate.Y + i >= cubeSize || positionCoordinate.X + j < 0 || positionCoordinate.X + j >= cubeSize)
                    continue;
                        
                adjacentFaces.Add(currentCubeFace[positionIndex + i * cubeSize + j].activeFaces[currentCubeletFace.direction]);
            }
        }

        return adjacentFaces;
    }
}