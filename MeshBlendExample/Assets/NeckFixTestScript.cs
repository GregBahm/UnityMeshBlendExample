using UnityEngine;

public class NeckFixTestScript : MonoBehaviour
{
    [SerializeField]
    private Material headMat;

    [SerializeField]
    private Mesh neckMesh;
    [SerializeField]
    private Mesh outputMesh;
    [SerializeField]
    private Transform neckBoneTransform;

    // For each point in OutputMesh, neckPointsBuffer contains the 3D position corrisponding to the same UV position on the neck
    private ComputeBuffer neckPointsBuffer;

    private void Start()
    {
        neckPointsBuffer = GetNeckPointsBuffer();
    }

    private ComputeBuffer GetNeckPointsBuffer()
    {
        int bufferLength = outputMesh.vertices.Length;
        int bufferStride = sizeof(float) * 4;
        Vector4[] buffer = new Vector4[bufferLength];
        ComputeBuffer ret = new ComputeBuffer(bufferLength, bufferStride);

        for (int i = 0; i < bufferLength; i++)
        {

            Vector2 uv = outputMesh.uv[i];
            buffer[i] = GetPositionFromUV(neckMesh, uv);
        }

        ret.SetData(buffer);
        return ret;
    }

    private void Update()
    {
        headMat.SetMatrix("_NeckBoneTransform", neckBoneTransform.localToWorldMatrix);
        headMat.SetBuffer("_NeckPointsBuffer", neckPointsBuffer);
    }

    private void OnDestroy()
    {
        neckPointsBuffer.Dispose();
    }


    const float tolerance = -0.1f;

    // ChatGTP is horrifyingly powerful...
    private static Vector4 GetPositionFromUV(Mesh mesh, Vector2 uv)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvCoords = mesh.uv;
        int[] triangles = mesh.triangles;

        // Find the triangle that contains the UV coordinate
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector2 uv1 = uvCoords[triangles[i]];
            Vector2 uv2 = uvCoords[triangles[i + 1]];
            Vector2 uv3 = uvCoords[triangles[i + 2]];

            Vector3 barycentric = GetBarycentric(uv, uv1, uv2, uv3);

            // Check if the UV coordinate is inside this triangle
            if (barycentric.x > tolerance && barycentric.y > tolerance && barycentric.z > tolerance)
            {
                // Use barycentric coordinates to interpolate the position in 3D
                Vector3 p1 = vertices[triangles[i]];
                Vector3 p2 = vertices[triangles[i + 1]];
                Vector3 p3 = vertices[triangles[i + 2]];
                Vector3 ret = barycentric.x * p1 + barycentric.y * p2 + barycentric.z * p3;

                return new Vector4(ret.x, ret.y, ret.z, 1);
            }
        }

        // If the UV coordinate is not found, return Vector3.zero
        return Vector4.zero;
    }

    private static Vector3 GetBarycentric(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 v0 = b - a;
        Vector2 v1 = c - a;
        Vector2 v2 = p - a;
        float d00 = Vector2.Dot(v0, v0);
        float d01 = Vector2.Dot(v0, v1);
        float d11 = Vector2.Dot(v1, v1);
        float d20 = Vector2.Dot(v2, v0);
        float d21 = Vector2.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;
        return new Vector3(u, v, w);
    }
}