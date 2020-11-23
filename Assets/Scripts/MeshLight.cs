using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshLight : MonoBehaviour
{
    public Transform[]  origins;
    public float        radius = 50.0f;
    public float        fov = 45;
    public int          subdivisions = 10;
    public Material     material;
    public LayerMask    obstacleLayer;

    Character character;
    Mesh      mesh;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<Character>();
        if (character == null)
        {
            character = GetComponentInParent<Character>();
        }

        mesh = new Mesh();
        mesh.name = "LightMesh";
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLightMesh();   
    }

    void UpdateLightMesh()
    {
        var         direction = character.GetDirection();
        var         baseAngle = 0;
        Transform   origin = origins[(int)direction];

        switch (direction)
        {
            case Character.Direction.North:
                baseAngle = 90;
                break;
            case Character.Direction.East:
                baseAngle = 0;
                break;
            case Character.Direction.South:
                baseAngle = -90;
                break;
            case Character.Direction.West:
                baseAngle = 180;
                break;
            default:
                break;
        }

        float angle = Mathf.Deg2Rad * (baseAngle - fov * 0.5f);
        float angleInc = Mathf.Deg2Rad * (fov / (float)subdivisions);

        List<Vector3> positions = new List<Vector3>();
        List<int>     triangles = new List<int>();

        positions.Add(origin.position);

        for (int i = 0; i <= subdivisions; i++)
        {
            var delta = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f);

            var hitInfo = Physics2D.Raycast(origin.position, delta, radius, obstacleLayer);

            if (hitInfo)
            {
                positions.Add(hitInfo.point);
            }
            else
            {
                positions.Add(origin.position + delta * radius);
            }

            angle += angleInc;
        }

        for (int i = 0; i <= subdivisions; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        mesh.SetVertices(positions);
        mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
        mesh.UploadMeshData(false);
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100000.0f);

        Graphics.DrawMesh(mesh, new Vector3(0,0,-1), Quaternion.identity, material, gameObject.layer);
    }
}
