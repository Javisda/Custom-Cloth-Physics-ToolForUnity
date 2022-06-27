using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class Spring
{
    public MassSpringCloth Manager;
    public Node nodeA, nodeB;

    public float Length0;
    public float Length;

    public float Stiffness;
    public float Damping;

    public bool Traction; // True if traction spring and false if Flexion type spring

    public Spring(MassSpringCloth m, Node a, Node b, float stiffness, float damping, bool edgeType)
    {
        Manager = m;
        nodeA = a;
        nodeB = b;
        Stiffness = stiffness;
        Damping = damping;
        Length0 = Length = (nodeA.Pos - nodeB.Pos).magnitude;
        Traction = edgeType;
    }


    public void UpdateLength()
    {
        Length = (nodeA.Pos - nodeB.Pos).magnitude;
    }

    public void ComputeForces()
    {
        Vector3 u = nodeA.Pos - nodeB.Pos;
        u.Normalize();
        float modeloDeAmortiguamiento = Manager.AlphaDamping * Vector3.Dot(u, nodeA.Vel - nodeB.Vel);
        float stress = Stiffness * (Length - Length0) + modeloDeAmortiguamiento;
        Vector3 force = -stress * u;
        nodeA.Force += force;
        nodeB.Force -= force;
    }

    public void RefreshValues(float d)
    {
        Damping = d;
    }
}*/