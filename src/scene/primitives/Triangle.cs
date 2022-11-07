using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray){
            // Implementation inspired by https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle

            // Calculate normal
            Vector3 edge0 = v1 - v0;
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v0 - v2;
            Vector3 normal = (edge0).Cross(-edge2).Normalized();

            double d = -normal.Dot(this.v0);
            double t = -(normal.Dot(ray.Origin) + d) / normal.Dot(ray.Direction);
        
            if (Math.Abs(normal.Dot(ray.Direction)) < 0.0001){
                // Ray is parallel to triangle
                return null;
            } else if (t < 0){
                // Triangle is behind ray
                return null;
            }

            Vector3 position = ray.Origin + t*ray.Direction;

            Vector3 c0 = edge0.Cross(position - v0);
            Vector3 c1 = edge1.Cross(position - v1);
            Vector3 c2 = edge2.Cross(position - v2);

            // Check if point is outside the triangle
            if (normal.Dot(c0) < 0 || normal.Dot(c1) < 0 || normal.Dot(c2) < 0){
                return null;
            }

            return new RayHit(position, normal, ray.Direction, Material);
        }

        /// <summary>
        /// Determine if a ray intersects with a back face of the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit IntersectBackFace(Ray ray)
        {
            Vector3 edge0 = v1 - v0;
            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v0 - v2;
            Vector3 normal = (edge0).Cross(-edge2).Normalized();

            if (!(ray.Direction.Dot(normal) > 0)){
                return null;
            }

            return Intersect(ray);

        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
