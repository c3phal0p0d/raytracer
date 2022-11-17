# Raytracer

<p align=“center">
    <img src="images/final_scene.png">
</p>

## Features
### Currently implemented
* Support for triangles, spheres & planes
* Support for different types of materials: diffuse, reflective, refractive & glossy
* Shadow casting
* Custom camera orientation
* Depth of field
* Anti-aliasing

### To be implemented
* Support for emissive materials
*  Ambient occlusion
* OBJ model support

## Using the program
### Representing the scene in the input text file
* Material: ```Material "MaterialName" <MaterialType> (Color) <RefractiveIndex>```
	* ```<MaterialType>``` represents the material type, and can be one of ```Diffuse```, ```Reflective```, ```Refractive``` or ```Glossy```. 
	* ```(Color)``` represents the color of the material as ```(r, g, b)``` where each value is a double between 0 and 1 corresponding to red, green and blue respectively. 
	* ```<RefractiveIndex>``` is a double value between 0 and 1.
* Triangle: ```Triangle "TriangleName" (v0) (v1) (v2) "MaterialName"```
	* Vertices are represented by ``` (v0)```, ```(v1)``` and ```(v2)``` each as coordinates ```(x, y, z)```. The front face is defined by clockwise winding order.
	* ```"MaterialName"``` indicates the material that should be used.
* Sphere: ```Sphere "SphereName" (center) <radius> "MaterialName"```
	* ```(center)``` represents the center of the sphere as coordinates ```(x, y, z)```
	* ```<radius>``` is a double representing the radius of the sphere
	* ```"MaterialName"``` indicates the material that should be used.
* Plane: ```Plane "PlaneName" (center) (normal) "MaterialName"```
	* ```(center)``` represents the center of the sphere as coordinates ```(x, y, z)```
	* ```(normal)``` represents the direction of the normal vector of the plane as coordinates ```(x, y, z)```
	* ```"MaterialName"``` indicates the material that should be used.
* Point light: ```PointLight "LightName" (position) (Color)```
	* ```(position)``` represents the position of the point light as coordinates ```(x, y, z)```
	* ```(Color)``` represents the color of the material as ```(r, g, b)``` where each value is a double between 0 and 1 corresponding to red, green and blue respectively. 


### Running the program from the command line
Ensure that .NET SDK version 6.0 or higher is installed, and then run the following command:
```
dotnet run -- -f tests/sample_scene_1.txt -o images/ouput.png
```

#### Command arguments
* ``` -f ```: Input text file path
* ``` -o ```: Output image file path
* ``` -w ```: Image width (default 400px)                                    
* ``` -h ```: Input height (default 400px)
* ``` -x ```: Anti-aliasing multiplier (default 1)                                
* ``` -p ```: Camera position coordinates (default (0.0, 0.0, 0.0))
* ``` -a ```: Camera rotation axis vector (default (0.0, 0.0, 1.0))
* ``` -n ```: Camera rotation angle (default 0 degrees) 
* ``` -r ```: Aperture radius of camera (default 0.0) 
* ``` -t ```: Focal length of camera (default 1.0) 

``` -f ``` and ```-o``` are mandatory, the rest are optional.


                                     
## References

* https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle  
* https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection  
* https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection  
* https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel  
* https://handwiki.org/wiki/Rodrigues%27_rotation_formula  
* https://medium.com/@elope139/depth-of-field-in-path-tracing-e61180417027  
* https://www.scratchapixel.com/lessons/3d-basic-rendering/phong-shader-BRDF/phong-illumination-models-brdf  
