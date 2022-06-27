using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sample code for accessing MeshFilter data.
/// </summary>
/*public class MassSpringCloth : MonoBehaviour
{
    public WindZone[] winds;
    public GameObject[] fixer;
    private Vector3[] vertices;
    private List<Edge> sortedEdges;
    public List<GameObject> collideableSpheres;
    /// <summary>
    /// Default constructor. Zero all. 
    /// </summary>
    public MassSpringCloth()
    {
        this.Paused = true;
        this.TimeStep = 0.02f;
        this.Gravity = new Vector3(0.0f, -9.81f, 0.0f);
        this.IntegrationMethod = Integration.Symplectic;
        this.Mass = 10.0f;
        this.StiffnessTraction = 5.0f;
        this.StiffnessFlexion = 1.0f;
        this.AlphaDamping = 0.1f;
        this.BetaDamping = 0.1f;
        this.setWind = false;
        this.windIntensity = 10;
        this.substeps = 1;
    }


    /// <summary>
    /// Integration method.
    /// </summary>
    public enum Integration
    {
        Explicit = 0,
        Symplectic = 1
    };

    #region InEditorVariables
    public bool Paused;
    public float TimeStep;
    public float Mass;
    public float StiffnessTraction;
    public float StiffnessFlexion;
    public float AlphaDamping;
    public float BetaDamping;
    public bool setWind;
    public float windIntensity;
    [Range(.0f, 1f)]
    public float penaltyStiffnes;
    public float substeps;


    public Vector3 Gravity;
    public Integration IntegrationMethod;
    public List<Node> nodes;
    public List<Spring> springs;
    Mesh mesh;
    #endregion

    #region OtherVariables

    #endregion

    #region MonoBehaviour

    public void Start()
    {
        mesh = this.GetComponent<MeshFilter>().mesh;
        nodes = new List<Node>();
        springs = new List<Spring>();
        sortedEdges = new List<Edge>();

        vertices = mesh.vertices;
        float massPerNode = Mass / vertices.Length;
        int[] triangles = mesh.triangles;

        //For simulation purposes, transform the points to global coordinates
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 pos = transform.TransformPoint(vertices[i]);
            nodes.Add(new Node(this, pos, massPerNode, AlphaDamping));
        }
        // Add fixed points
        for (int i = 0; i < fixer.Length; i++)
        {
            Bounds bounds = fixer[i].GetComponent<Collider>().bounds;

            foreach (Node node in nodes)
            {
                if (bounds.Contains(node.Pos))
                {
                    node.Fixed = true;
                }
            }
        }
        // Calculate Nodes-To-Fixer Offsets and set fixers
        for (int i = 0; i < fixer.Length; i++)
        {
            Bounds bounds = fixer[i].GetComponent<Collider>().bounds;

            for (int j = 0; j < nodes.Count; j++)
            {
                Vector3 offset;
                if (bounds.Contains(nodes[j].Pos))
                {
                    // Se calcula el desfase de cada nodo dentro del fixer respecto al centro de este, y este desfase tendrá que respetarse siempre en adelante.
                    //offset = fixer[i].transform.position - nodes[j].Pos;  // LINEA DE CODIGO ANTIGUO QUE NO PERMITE ROTACIONES IMICIALES
                    offset = fixer[i].transform.InverseTransformPoint(nodes[j].Pos); // LINEA DE CODIGO NUEVO QUE PERMITE ROTACIONES IMICIALES

                    nodes[j].Offset = offset;
                    nodes[j].SetFixer(fixer[i]);
                }
            }
        }



        // -------------- CREACIÓN DE ARISTAS Y CREACION DE MUELLES DE FLEXIÓN Y TRACCIÓN -------------- //
        for (int i = 0; i < triangles.Length; i += 3)
        {
            sortedEdges.Add(new Edge(triangles[i], triangles[i + 1], triangles[i + 2]));
            sortedEdges.Add(new Edge(triangles[i + 1], triangles[i + 2], triangles[i]));
            sortedEdges.Add(new Edge(triangles[i + 2], triangles[i], triangles[i + 1]));
        }

        // Los ordenamos segun una clase comparadora que junta las asristas repetidas
        EdgeComparer comparer = new EdgeComparer();
        sortedEdges.Sort(comparer);

        // Para las aristas repetidas se crea un muelle con un stiffnes de flexión entre los vertexOther
        for (int i = 0; i < sortedEdges.Count - 1; i++)
        {
            //En caso de ser una arista repetida añadimos el muelle de flexión
            if ((sortedEdges[i].vertexA == sortedEdges[i + 1].vertexA) && (sortedEdges[i].vertexB == sortedEdges[i + 1].vertexB))
            {
                springs.Add(new Spring(this, nodes[sortedEdges[i].vertexOther], nodes[sortedEdges[i + 1].vertexOther], StiffnessFlexion, BetaDamping, false));
            }
            else // En caso contrario el de traccion
            {
                springs.Add(new Spring(this, nodes[sortedEdges[i].vertexA], nodes[sortedEdges[i].vertexB], StiffnessTraction, BetaDamping, true));
            }
        }
        // Añadimos el ultimo muelle ya que con el bucle anterior no se puede
        springs.Add(new Spring(this, nodes[sortedEdges[sortedEdges.Count - 1].vertexA], nodes[sortedEdges[sortedEdges.Count - 1].vertexB], StiffnessTraction, BetaDamping, true));
        // ---------------------------------------------------------------------------------------------- //
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
            this.Paused = !this.Paused;

        if (Input.GetKeyDown(KeyCode.R))
            RefreshValues();

        //Procedure to update vertex positions
        mesh = this.GetComponent<MeshFilter>().mesh;
        vertices = new Vector3[mesh.vertexCount];


        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 pos;
            if (nodes[i].Fixed)
            {
                pos = Vector3.zero;
                for (int j = 0; j < fixer.Length; j++)
                {
                    if (nodes[i].Fixer == fixer[j])
                    {
                        // ---Versión antigua sin soportar rotaciones. Solo traslaciones---
                        //pos = fixer[j].transform.position - nodes[i].Offset;
                        //nodes[i].Pos = pos;

                        // ---Version nueva que soporta rotaciones del fixer---
                        // La posicion corresponde a la transformada que tiene el nodo si se le aplica la rotación del fixer sobre el centro de este.
                        // La rotación a aplicar es el cuaternion. La diferencia de posiciones es el offset, ya calculado. Despues, se le suma la posición global del fixer
                        // para que aparezca en el lugar adecuado en coordenadas globales.
                        //pos = Quaternion.Euler(fixer[j].transform.rotation.eulerAngles) * -nodes[i].Offset + fixer[j].transform.position; // LINEA DE CODIGO ANTIGUO QUE NO PERMITE ROTACIONES IMICIALES
                        pos = fixer[j].transform.TransformPoint(nodes[i].Offset); // LINEA DE CODIGO NUEVO QUE PERMITE ROTACIONES IMICIALES
                        
                        nodes[i].Pos = pos;
                    }
                }
            }
            else
            {
                pos = nodes[i].Pos;
            }
            vertices[i] = transform.InverseTransformPoint(pos);
        }

        mesh.vertices = vertices;
    }

    public void FixedUpdate()
    {
        if (this.Paused)
            return; // Not simulating

        // Substeps
        for (int i = 0; i < substeps; i++)
        {
            if (setWind)
                ComputeWind();

            // Select integration method
            switch (this.IntegrationMethod)
            {
                case Integration.Explicit: this.eulerIntegrationMethod();  break;
                case Integration.Symplectic: this.eulerIntegrationMethod();  break;
                default:
                    throw new System.Exception("[ERROR] Should never happen!");
            }
        }
    }

    private void eulerIntegrationMethod()
    {
        foreach (Node node in nodes)
        {
            node.Force = Vector3.zero;

            // ---- Non realistic Wind ---- Metodo antiguo
            //Vector3 windDirection;
            //if (setWind) {
            //windDirection = computeWindDirection() * windIntensity;
            //}
            //else { 
            //windDirection = Vector3.zero;
            //}
            //node.ComputeForces(windDirection);

            // More realistic wind Method
            node.ComputeForces(node.WindForce);

            // Check for Sphere collision
            node.Force = CheckSpheresCollision(node);

        }
        foreach (Spring spring in springs)
        {
            spring.ComputeForces();
        }

        // Se calculan las posiciones
        foreach (Node node in nodes)
        {
            if (!node.Fixed)
            {
                // Si primero calculamos la velocidad y luego la posicion es simpléctico. Si lo hacemos al revés es explícito.
                if (Integration.Explicit == this.IntegrationMethod)
                {
                    node.Pos += (TimeStep) * node.Vel;
                    node.Vel += (TimeStep) / node.Mass * node.Force;
                }
                else if (Integration.Symplectic == this.IntegrationMethod)
                {
                    node.Vel += (TimeStep) / node.Mass * node.Force;
                    node.Pos += (TimeStep) * node.Vel;
                }
            }
        }

        foreach (Spring spring in springs)
        {
            spring.UpdateLength();
        }
    }
    #endregion

    private Vector3 computeWindDirection()
    {
        Vector3 windDirection = new Vector3();
        for (int i = 0; i < winds.Length; i++)
        {
            windDirection += winds[i].transform.forward.normalized * winds[i].windMain;
        }
        return windDirection;
    }

    private void RefreshValues()
    {
        // Mass
        float massPerNode = Mass / nodes.Count;
        foreach (Node node in nodes)
        {
            node.RefreshValues(massPerNode);
        }
        // Damping
        foreach (Spring spring in springs)
        {
            if (spring.Traction)
                spring.RefreshValues(StiffnessTraction);
            else
                spring.RefreshValues(StiffnessFlexion);
        }

        //TODO:
        //Implementar más valores
    }

    private Vector3 CheckSpheresCollision(Node node)
    {
        Vector3 force = node.Force;
        // En caso de colisionar con un objeto se cambia la fuerza. En caso contrario se devuelve la misma que ya teía.
        foreach (GameObject collideable in collideableSpheres)
        {
            if (collideable.GetComponent<Collider>().bounds.Contains(node.Pos))
            {
                float radius = collideable.GetComponent<SphereCollider>().radius;
                Vector3 forceDir = node.Pos - collideable.transform.position;
                float porcentajePenetration = (radius - forceDir.sqrMagnitude) / radius;
                force = (-1) * forceDir * porcentajePenetration * penaltyStiffnes;
            }
        }
        return force;
    }

    private void ComputeWind()
    {
        // Calculamos primero la direccion del viento resultante
        Vector3 windVelocity = computeWindDirection();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 v = nodes[mesh.triangles[i]].Pos - nodes[mesh.triangles[i + 1]].Pos;
            Vector3 w = nodes[mesh.triangles[i]].Pos - nodes[mesh.triangles[i + 2]].Pos;
            float area = Vector3.Cross(v, w).magnitude / 2;  // TODO: PREGUNTAR JORGE. POR QUÉ VA TAN MAL CON SQRMAGNITUDE O QUÉ ES MÁS CORRECTO

            Vector3 triangleVelocity = (nodes[mesh.triangles[i]].Vel + nodes[mesh.triangles[i + 1]].Vel + nodes[mesh.triangles[i + 2]].Vel) / 3;
            Vector3 triangleNormal = Vector3.Cross(v, w);

            Vector3 force = 0.01f * area * (Vector3.Dot(triangleNormal, windVelocity - triangleVelocity) * triangleNormal);

            // Les atribuimos la fuerza resultante a los nodos
            nodes[mesh.triangles[i]].WindForce = force / 3;
            nodes[mesh.triangles[i + 1]].WindForce = force / 3;
            nodes[mesh.triangles[i + 2]].WindForce = force / 3;
        }
    }

}*/