using UnityEngine;


public static class VectorExtensions
{
   
    public static Vector3 NoY(this Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }
    

    public static Vector3 OnlyY(this Vector3 vector)
    {
        return new Vector3(0f, vector.y, 0f);
    }
}