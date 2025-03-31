// MeshGenerator.cs
// GAM713 Prototype 1
//
// Created by Avi Virendra Parmar
// On 1st February 2025
//

using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uv;
    Color[] vertexColors;

    [Header("Grid size settings")]
    [SerializeField][Range(1f, 100f)] private int xGridSize;
    [SerializeField][Range(1f, 100f)] private int zGridSize;

    [Header("HeightMap settings")]
    [SerializeField] private string loadHeightMapFilePath = "/HeightMap.png";
    [SerializeField] private float maxHeight = 1f;

    [Header("Terrain Color Settings")]
    [SerializeField] private Gradient terrainGradient;

    [Header("Debug settings")]
    [SerializeField] bool drawVertexGizmos = false;

    private float minTerrainHeight;
    private float maxTerrainHeight;


    private void Start() {
        mesh = new Mesh();
        mesh = GetComponent<MeshFilter>().mesh;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape() {
        //CREATING VERTICES
        vertices = new Vector3[(xGridSize+1) * (zGridSize + 1)];
        Texture2D heightMap = LoadHeightMap(loadHeightMapFilePath);

        for (int i = 0, z = 0; z <= zGridSize; z++) {
            for ( int x = 0; x <= xGridSize; x++) {
                float y = SampleHeightFromTexture(heightMap, x, z); //load heightmap
                Debug.Log(y);
                vertices[i] = new Vector3(x, y, z);

                if (y > maxTerrainHeight) {
                    maxTerrainHeight = y;
                }

                if (y < minTerrainHeight) {
                    minTerrainHeight = y;
                }

                i++; 
            }
        }

        //CREATING TRAINGLES AND MESH
        triangles = new int[xGridSize * zGridSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zGridSize; z++){
            for (int x = 0; x < xGridSize; x++) {
            
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xGridSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xGridSize + 1;
                triangles[tris + 5] = vert + xGridSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    
        // CREATING UV
        uv = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= zGridSize; z++) {
            for ( int x = 0; x <= xGridSize; x++) {
                uv[i] = new Vector2((float)x / xGridSize, (float)z / zGridSize);
                i++; 
            }
        }

        // CREATING VERTEX COLORS
        vertexColors = new Color[vertices.Length];

        for (int i = 0, z = 0; z <= zGridSize; z++) {
            for ( int x = 0; x <= xGridSize; x++) {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                vertexColors[i] = terrainGradient.Evaluate(height);
                i++; 
            }
        }
    }

    void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.colors = vertexColors;

        mesh.RecalculateNormals();
    }

    void OnDrawGizmos() {

        if (vertices == null) 
        return;

        if (drawVertexGizmos) {
            for (int i = 0; i < vertices.Length; i++) {
                Gizmos.DrawSphere(vertices[i], 0.1f);
            }
        }
    }

    Texture2D LoadHeightMap(string filePath) {
        string path = Application.persistentDataPath + loadHeightMapFilePath;

        byte[] fileData = System.IO.File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2048, 2048);
        texture.LoadImage(fileData);

        return texture;
    }

    float SampleHeightFromTexture(Texture2D heightMap, int x, int z) {
        // Ensure x and z are within texture bounds
        int tex_X = Mathf.Clamp((int)((x / (float)xGridSize) * heightMap.width), 0, heightMap.width - 1);
        int tex_Z = Mathf.Clamp((int)((z / (float)zGridSize) * heightMap.height), 0, heightMap.height - 1);

        // Get pixel colour and convert to height (Assuming greyscale)
        Color pixel = heightMap.GetPixel(tex_X, tex_Z);
        return pixel.r * maxHeight * 10; // Use red channel
    }
}
