using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image.
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        public void Render(Image outputImage)
        {
            for (int x=0; x<outputImage.Width; x++){
                for (int y=0; y<outputImage.Height; y++){
                    Vector3 direction = ConvertToWorldSpaceCoordinates(x, y, outputImage);
                    Vector3 origin = options.CameraPosition;
                    Color color = new Color(0, 0, 0);

                    // Apply anti-aliasing
                    for (double i=0; i<options.AAMultiplier; i++){
                        for (double j=0; j<options.AAMultiplier; j++){
                            direction = ConvertToWorldSpaceCoordinates(x+i/options.AAMultiplier, y+j/options.AAMultiplier, outputImage);
                            
                            // Apply depth of field, implementation inspired by https://medium.com/@elope139/depth-of-field-in-path-tracing-e61180417027
                            if (options.ApertureRadius!=0){
                                Color newColor = new Color(0, 0, 0);
                                int nRays = 5;
                                for (int n=0; n<nRays; n++){
                                    //Vector3 focalPoint = new Vector3(direction.X, direction.Y, direction.Z*options.FocalLength);
                                    Vector3 focalPoint = direction*options.FocalLength - origin;
                                    Vector3 randomPosition = new Vector3(options.ApertureRadius*((new Random()).NextDouble()*2-1)/2, options.ApertureRadius*((new Random()).NextDouble()*2-1)/2, 0);
                                    Vector3 newOrigin = origin + randomPosition;
                                    Vector3 newDirection = focalPoint - newOrigin;
                                    newColor += CastRay(newOrigin, newDirection, 0);
                                }
                                color += (newColor/nRays);
                            } 
                            
                            else {
                                color += CastRay(origin, direction, 0);
                            }
                        }
                    }

                    color = color/(options.AAMultiplier*options.AAMultiplier);
                    outputImage.SetPixel(x, y, color);
                }
            }
        }

        /// <summary>
        /// Convert pixel coordinates to world space coordinates.
        /// </summary>
        /// <param name="x">Pixel x-coordinate</param>
        /// <param name="y">Pixel y-coordinate</param>
        /// <param name="outputImage">Image to store render output</param>
        public Vector3 ConvertToWorldSpaceCoordinates(double x, double y, Image outputImage){
            // Equations taken from COMP30019 lecture slides
            double aspectRatio = (double)outputImage.Height/outputImage.Width;
            double fov = 60*Math.PI/180;

            double normX = (x + 0.5)/outputImage.Width;
            double normY = (y + 0.5)/outputImage.Height;

            double posX = (normX*2 - 1) * Math.Tan(fov/2);
            double posY = (1 - normY*2) * Math.Tan(fov/2)*aspectRatio;

            Vector3 direction = new Vector3(posX, posY, 1);
            
            // Apply camera rotation, using formula taken from https://handwiki.org/wiki/Rodrigues%27_rotation_formula
            double rotationAngle = options.CameraAngle*(Math.PI/180);
            Vector3 rotationAxis = options.CameraAxis.Normalized();
            Vector3 newDirection = direction*Math.Cos(rotationAngle) + rotationAxis.Cross(direction)*Math.Sin(rotationAngle) + rotationAxis*(rotationAxis.Dot(direction))*(1-Math.Cos(rotationAngle));

            return newDirection.Normalized();
        }

        /// <summary>
        /// Cast a ray
        /// </summary>
        /// <param name="origin">Origin of ray</param>
        /// <param name="direction">Direction in which ray is cast</param>
        /// <param name="depth">Recursion depth</param>
        public Color CastRay(Vector3 origin, Vector3 direction, int depth){
            Ray ray = new Ray(origin, direction);
            Vector3 closestHit = new Vector3(direction.X, direction.Y, 1000);
            Color color = new Color(0, 0, 0);

            foreach (SceneEntity entity in this.entities){
                RayHit hit = entity.Intersect(ray);
                RayHit backFaceHit = entity.IntersectBackFace(ray);
                if (hit != null && backFaceHit == null && depth<5) {
                    Vector3 hitToClosestHit = closestHit-hit.Position;
                    double dotProductResult = hit.Position.Normalized().Dot(closestHit.Normalized());
                    
                    if (((hit.Position-origin).LengthSq()<(closestHit-origin).LengthSq())){
                        // Object is closest to the camera and therefore should be rendered
                        closestHit = new Vector3(hit.Position.X, hit.Position.Y, hit.Position.Z);
                        
                        if (entity.Material.Type==Material.MaterialType.Diffuse){
                            color = DiffuseMaterial(entity, hit);
                        } 

                        else if (entity.Material.Type==Material.MaterialType.Reflective){
                            Vector3 reflection = Reflect(hit.Incident, hit.Normal);
                            Vector3 offset = 0.1*(reflection);
                            color = CastRay(hit.Position+offset, reflection, depth+1);
                        }
                        
                        else if (entity.Material.Type==Material.MaterialType.Refractive){
                            Color refractionColor = new Color(0, 0, 0);
                            Color reflectionColor = new Color(0, 0, 0);
                            double kr = Fresnel(entity, hit.Incident, hit.Normal);

                            if (kr < 1){
                                // Not the case of total internal reflection, so calculate refraction
                                Vector3 refraction = Refract(entity, hit.Incident, hit.Normal);
                                Vector3 refractionOffset = 0.1*(refraction);
                                refractionColor = CastRay(hit.Position+refractionOffset, refraction, depth+1);
                            }

                            // Calculate reflection
                            Vector3 reflection = Reflect(hit.Incident, hit.Normal);
                            Vector3 reflectionOffset = 0.1*(reflection);
                            reflectionColor = CastRay(hit.Position+reflectionOffset, reflection, depth+1);
                            
                            // Combine reflection and refraction as final output
                            color = reflectionColor*kr + refractionColor*(1-kr);
                        }

                        else if (entity.Material.Type==Material.MaterialType.Glossy){
                            color = GlossyMaterial(entity, hit);
                        }  
                    }
                }
            }
            
            return color;
        }

        /// <summary>
        /// Handles diffuse materials
        /// </summary>
        /// <param name="entity">Entity hit by ray</param>
        /// <param name="hit">Ray hit</param>
        public Color DiffuseMaterial(SceneEntity entity, RayHit hit){
            Color lightColor = new Color(0, 0, 0);

            foreach (PointLight light in this.lights){
                // Check if point is not in shadow
                if (!(IsInShadow(hit.Position, light.Position))){
                    double dotProductResult = hit.Normal.Dot((light.Position-hit.Position).Normalized());
                    if ((dotProductResult)<0.00001){
                        dotProductResult = 0;
                    }
                    lightColor += light.Color*dotProductResult;
                } 
                    
            }
            
            // Make sure colour values are not negative
            if (lightColor.R < 0 || lightColor.G < 0 || lightColor.B < 0){
                lightColor = new Color(0, 0, 0);
            }
            
            return entity.Material.Color*lightColor;
        }
        
        /// <summary>
        /// Handles glossy materials
        /// </summary>
        /// <param name="entity">Entity hit by ray</param>
        /// <param name="hit">Ray hit</param>
        public Color GlossyMaterial(SceneEntity entity, RayHit hit){
            // Equations taken from https://www.scratchapixel.com/lessons/3d-basic-rendering/phong-shader-BRDF/phong-illumination-models-brdf
            Color diffuse = DiffuseMaterial(entity, hit);
            double ka = 0.15;   // ambient reflection constant
            double ks = 1;      // specular reflection constant
            double kd = 1.1;    // diffuse reflection constant
            double n = 200;     // shininess constant
            Color color = new Color(0, 0, 0);

            // Add ambient reflection component
            color += ka*entity.Material.Color;

            foreach (PointLight light in this.lights){
                Vector3 hitToLight = light.Position-hit.Position;
                Vector3 lightReflection = Reflect(light.Position-hit.Position, hit.Normal);
                Color specular = light.Color;

                // Check if point is not in shadow
                if (!(IsInShadow(hit.Position, light.Position))){
                    double dotProductResult = lightReflection.Normalized().Dot(hit.Incident.Normalized());
                    if (dotProductResult<0){
                        dotProductResult = 0;
                    }

                    // Add diffuse and specular reflection components
                    color += kd*(hitToLight.Normalized().Dot(hit.Normal))*diffuse+ks*Math.Pow(dotProductResult,n)*specular;
                }
            }

            return color;
        }
        
        /// <summary>
        /// Determine whether a given point is covered by a shadow
        /// </summary>
        /// <param name="hitPosition">Position of given point</param>
        /// <param name="lightPosition">Position of the light source</param>
        public bool IsInShadow(Vector3 hitPosition, Vector3 lightPosition){
            Vector3 hitToLight = lightPosition-hitPosition;
            Vector3 offset = 0.001*hitToLight;
            Ray shadowRay = new Ray(hitPosition + offset, hitToLight);

            foreach (SceneEntity entity in this.entities){
                RayHit shadowHit = entity.Intersect(shadowRay);
                
                if (shadowHit != null){
                    Vector3 hitToShadowHit = shadowHit.Position-hitPosition;
                    double dotProductResult = hitToShadowHit.Normalized().Dot(hitToLight.Normalized());
                    double hitToShadowHitDistance = Math.Sqrt(Math.Pow(shadowHit.Position.X-hitPosition.X, 2) + Math.Pow(shadowHit.Position.Y-hitPosition.Y, 2) + Math.Pow(shadowHit.Position.Z-hitPosition.Z, 2));
                    double hitToLightDistance = Math.Sqrt(Math.Pow(lightPosition.X-hitPosition.X, 2) + Math.Pow(lightPosition.Y-hitPosition.Y, 2) + Math.Pow(lightPosition.Z-hitPosition.Z, 2));

                    // Check if object is between point and light source, and hence will cast a shadow
                    if ((0.9999<=dotProductResult) && (dotProductResult<=1.0001) && (hitToShadowHitDistance<hitToLightDistance)){
                        return true;
                    }
                        
                 }
            }
            
            return false;          
        }

        /// <summary>
        /// Return reflection vector
        /// </summary>
        /// <param name="incident">Position of incident ray</param>
        /// <param name="normal">Position of normal to incident ray</param>
        public Vector3 Reflect(Vector3 incident, Vector3 normal){
            // Equation taken from https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel
            return incident - 2*incident.Dot(normal)*normal;
        }

        /// <summary>
        /// Return refraction vector
        /// </summary>
        /// <param name="incident">Position of incident ray</param>
        /// <param name="normal">Position of normal to incident ray</param>
        public Vector3 Refract(SceneEntity entity, Vector3 incident, Vector3 normal){
            // Implementation inspired by https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel
            double n1 = 1;
            double n2 = entity.Material.RefractiveIndex;
            double cosi = normal.Dot(incident.Normalized());

            if (cosi<0){
                // Outside the surface
                cosi = -cosi;
            } 

            else {
                // Inside the surface
                normal = -normal;

                double temp = n1;
                n1 = n2;
                n2 = temp;

            }

            double n = n1/n2;
            double k = 1 - n*n*(1-cosi*cosi);
            
            return n*incident.Normalized() + (n*cosi-Math.Sqrt(k))*normal;
        }

        public double Fresnel(SceneEntity entity, Vector3 incident, Vector3 normal){
            // Implementation inspired by https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel
            double kr = 0;
            double n1 = 1;
            double n2 = entity.Material.RefractiveIndex;
            double cosi = normal.Dot(incident.Normalized());

            if (cosi<0){
                // Outside the surface
                cosi = -cosi;
            } 

            double sint = n1/n2*Math.Sqrt(1-cosi*cosi);

            if (sint>=1){
                // Total internal reflection
                kr = 1;
            }

            else {
                double cost = Math.Sqrt(Math.Max(0, 1-sint*sint));
                double rs = ((n2*cosi)-(n1*cost))/((n2*cosi)+(n1*cost));
                double rp = ((n1*cosi)-(n2*cost))/((n1*cosi)+(n2*cost));
                kr = (rs*rs+rp*rp)/2;
            }

            return kr;
        }

    }
}
