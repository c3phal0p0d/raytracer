
namespace RayTracer
{
    /// <summary>
    /// Interface to represent an entity (object) in a ray traced scene. 
    /// All of the primitive types -- planes, triangles, spheres --
    /// implement this interface.
    /// </summary>
    public interface SceneEntity
    {
        /// <summary>
        /// Check whether a given ray intersects with this entity.
        /// If so, return hit data. Otherwise, return null.
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no intersection</returns>
        RayHit Intersect(Ray ray);

        /// <summary>
        /// Check whether a given ray intersects with a back face of this entity.
        /// If so, return hit data. Otherwise, return null.
        /// </summary>
        /// <param name="ray">Ray data</param>
        /// <returns>Ray hit data, or null if no intersection</returns>
        RayHit IntersectBackFace(Ray ray);

        /// <summary>
        /// The material assigned to this entity.
        /// </summary>
        Material Material { get; }
    }
}
