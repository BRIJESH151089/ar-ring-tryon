using UnityEngine;

namespace ArRingTryOn.Rendering
{
    /// <summary>Generates a torus around the local Y axis.</summary>
    public static class TorusMesh
    {
        /// <param name="majorRadius">Distance from the centre to the middle of the tube.</param>
        /// <param name="tubeRadius">Radius of the tube.</param>
        /// <param name="ringSegments">Segments around the ring.</param>
        /// <param name="tubeSegments">Segments around the tube.</param>
        public static Mesh Build(float majorRadius, float tubeRadius, int ringSegments, int tubeSegments)
        {
            var columns = tubeSegments + 1;
            var vertexCount = (ringSegments + 1) * columns;
            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];

            for (var ring = 0; ring <= ringSegments; ring++)
            {
                var ringAngle = ring * Mathf.PI * 2f / ringSegments;
                var outward = new Vector3(Mathf.Cos(ringAngle), 0f, Mathf.Sin(ringAngle));

                for (var tube = 0; tube <= tubeSegments; tube++)
                {
                    var tubeAngle = tube * Mathf.PI * 2f / tubeSegments;
                    var normal = outward * Mathf.Cos(tubeAngle) + Vector3.up * Mathf.Sin(tubeAngle);
                    var index = ring * columns + tube;
                    vertices[index] = outward * majorRadius + normal * tubeRadius;
                    normals[index] = normal;
                }
            }

            var triangles = new int[ringSegments * tubeSegments * 6];
            var next = 0;
            for (var ring = 0; ring < ringSegments; ring++)
            {
                for (var tube = 0; tube < tubeSegments; tube++)
                {
                    var a = ring * columns + tube;
                    var b = a + columns;
                    var c = a + 1;
                    var d = b + 1;

                    triangles[next++] = a;
                    triangles[next++] = c;
                    triangles[next++] = b;
                    triangles[next++] = b;
                    triangles[next++] = c;
                    triangles[next++] = d;
                }
            }

            var mesh = new Mesh { name = "Torus" };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
