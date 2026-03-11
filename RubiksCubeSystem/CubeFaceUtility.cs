using Godot;

public class CubeFaceUtility
{
    public static Transform3D GetFaceTransform(CubeFaceDirection dir)
    {
        Vector3 normal = GetDirectionVector(dir);
        Basis basis = GetFaceBasis(dir);
        Vector3 offset = normal * 0.5f;
        return new Transform3D(basis, offset);
    }

    static Vector3 GetDirectionVector(CubeFaceDirection dir)
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

    static Basis GetFaceBasis(CubeFaceDirection dir)
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
}