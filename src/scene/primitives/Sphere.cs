using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a sphere in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray){
            // Implementation inspired by https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
            
            // Compute values of quadratic equation
            Vector3 centerToOrigin = ray.Origin-center;
            double a = ray.Direction.Dot(ray.Direction);
            double b = 2*ray.Direction.Dot(centerToOrigin);
            double c = centerToOrigin.Dot(centerToOrigin) - radius*radius;
            double t, t0, t1;

            // Check value of the discriminant
            double discriminant = b*b-4*a*c;
            if (discriminant<0){
                // No intersection
                return null;
            } else if (discriminant==0){
                // One point of intersection
                t0 = -b/(2*a);
            } else {
                // Two points of intersection
                t0 = (-b + Math.Sqrt(discriminant))/(2*a);
                t1 = (-b - Math.Sqrt(discriminant))/(2*a);

                if (t0 > t1){
                    double temp;
                    temp = t0;
                    t0 = t1;
                    t1 = temp;
                }

                if (t0 < 0){
                    t0  = t1;
                    if (t0 < 0){
                        return null;
                    }
                }
            }

            t = t0;

            Vector3 position = ray.Origin + ray.Direction*t;
            Vector3 normal = (position - center).Normalized();

            return new RayHit(position, normal, ray.Direction, Material);
        }

         /// <summary>
        /// Determine if a ray intersects with a back face of the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit IntersectBackFace(Ray ray){
            return null;
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
