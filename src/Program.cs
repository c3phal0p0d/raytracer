using System;
using System.IO;
using CommandLine;

namespace RayTracer
{
    public class Program
    {
        /// <summary>
        /// Command line arguments configuration
        /// </summary>
        public class OptionsConf
        {
            [Option('f', "file", Required = true, HelpText = "Input file path (txt).")]
            public string InputFilePath { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output file path (PNG).")]
            public string OutputFilePath { get; set; }

            [Option('w', "width", Default = (int)400, HelpText = "Output image width in pixels.")]
            public int OutputImageWidth { get; set; }

            [Option('h', "height", Default = (int)400, HelpText = "Output image height in pixels.")]
            public int OutputImageHeight { get; set; }

            [Option('x', "aa-mult", Default = (int)1, HelpText = "Anti-aliasing sampling multiplier.")]
            public int AAMultiplier { get; set; }

            [Option('l', "ambient", Default = (bool)false, HelpText = "Enable ambient lighting.")]
            public bool AmbientLightingEnabled { get; set; }

            [Option('p', "cam-pos", Default = (string)"0,0,0", HelpText = "Camera position in world coordinates in form: x,y,z")]
            public string CameraPosition { get; set; }

            [Option('a', "cam-axis", Default = (string)"0,0,1", HelpText = "Camera axis in world coordinates in form: x,y,z")]
            public string CameraAxis { get; set; }

            [Option('n', "cam-angle", Default = (double)0, HelpText = "Camera angle in degrees.")]
            public double CameraAngle { get; set; }

            [Option('r', "aperture-radius", Default = (double)0, HelpText = "Aperture radius of the camera.")]
            public double ApertureRadius { get; set; }

            [Option('t', "focal-length", Default = (double)1, HelpText = "Focal length of the camera.")]
            public double FocalLength { get; set; }
            
            [Option('q', "quality", Default = 0,
                HelpText = "Quality of the render.")]
            public int Quality { get; set; }
        }

        /// <summary>
        /// Helper to parse command line argument as Vector3
        /// </summary>
        /// <param name="cmdStr">String as vector in form #,#,#</param>
        /// <returns></returns>
        static Vector3 CmdStrToVector3(string cmdStr)
        {
            try
            {
                string[] s = cmdStr.Split(',');
                return new Vector3(double.Parse(s[0]), double.Parse(s[1]), double.Parse(s[2]));
            }
            catch (Exception)
            {
                throw new ArgumentException(@"Command line error: Expecting vector in form #,#,# but got " + cmdStr, "cmdLineParam");
            }
        }

        /// <summary>
        /// Main program entry point for the ray tracer.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private static void Main(string[] args)
        {
            // Assume failure as a default (override later if successful).
            Environment.ExitCode = 1;

            Parser.Default
                .ParseArguments<OptionsConf>(args)
                .WithParsed(options =>
                {
                    try
                    {
                        // Construct a new output image, with size according to command line args
                        var outputImage = new Image(options.OutputImageWidth,
                            options.OutputImageHeight);

                        // Parse the scene specification file
                        var sceneReader =
                            new SceneReader(options.InputFilePath);

                        // Construct the scene
                        var scene = new Scene(new SceneOptions(
                            options.AAMultiplier,
                            options.AmbientLightingEnabled,
                            CmdStrToVector3(options.CameraPosition),
                            CmdStrToVector3(options.CameraAxis),
                            options.CameraAngle,
                            options.ApertureRadius,
                            options.FocalLength,
                            options.Quality
                        ));
                        sceneReader.PopulateScene(scene);

                        // Render the scene by executing the core ray tracing logic
                        scene.Render(outputImage);

                        // Write output to file as PNG
                        outputImage.WritePNG(options.OutputFilePath);

                        Environment.ExitCode = 0;
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("Input file not found.");
                    }
                    catch (SceneReader.ParseException e)
                    {
                        Console.WriteLine(
                            $"Input file invalid on line {e.Line}: {e.Message}");
                    }
                    catch (ArgumentException e)
                    {
                        if (e.ParamName == "cmdLineParam")
                            Console.WriteLine(e.Message);
                        else
                            // Still allow other argument exceptions through
                            throw e;
                    }
                });
        }
    }
}
