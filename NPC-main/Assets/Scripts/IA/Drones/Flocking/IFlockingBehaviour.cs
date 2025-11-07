using System.Collections.Generic;
using UnityEngine;

public interface IFlockingBevaviour
{
    public Vector3 GetDir(List<Boid3D> boids);
}
