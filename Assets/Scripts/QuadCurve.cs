using UnityEngine;

public class QuadCurve
{
    public QuadCurve(Vector3 position)
    {
        this.P0 = this.P1 = this.P2 = position;
    }
    public Vector3 P0 = Vector3.zero;
    public Vector3 P1 = Vector3.zero;
    public Vector3 P2 = Vector3.zero;

    public Vector3 GetPointAlong(float distAlong)
    {
        return Vector3.Lerp(Vector3.Lerp(P0, P1, distAlong), Vector3.Lerp(P1, P2, distAlong), distAlong);
    }

    public Vector3 GetDerivativeAlong(float distAlong)
    {
        return 2 * Vector3.Lerp(P1 - P0, P2 - P1, distAlong);
    }

    public float ArmLength
    {
        get
        {
            return ((P1 - P0).magnitude + (P2 - P1).magnitude);
        }
        set { }
    }
}
