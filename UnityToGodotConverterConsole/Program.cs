using ConversionLogic;

namespace UnityToGodotConverterConsole
{
    /// <summary>
    /// This is the main entry point for the Unity to Godot code converter program.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main method that starts the program.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        private static void Main(string[] args)
        {

            string filePath;
            if (args.Length != 1)
            {
                // Prompt the user for input if no argument is provided
                //Console.Write("Enter the path to the C# file: ");
                Console.Write("Enter the path to the Scene file: ");
                filePath = Console.ReadLine().Replace("\"", "");
            }
            else
            {
                // Get the file path from command-line arguments
                filePath = args[0];
            }


            var sceneReader = new UnitySceneReader();

            sceneReader.ReadFile(filePath);

            /*
            // Create a Unity to Godot converter instance
            var converter = new UnityToGodotScriptConverter();

           

            // Check if the correct number of arguments is provided
            

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            // Check if the file has a valid C# extension
            if (!filePath.EndsWith(".cs"))
            {
                Console.WriteLine($"Not a valid C# file: {filePath}");
                return;
            }

            try
            {
                // Read the contents of the input C# file
                string fileContents = File.ReadAllText(filePath);

                // Process the file contents using the converter
                String newCodeFile = converter.ConvertUnityCodeToGodot(fileContents);

                // Generate a new file name for the Godot code
                string newFilePath = filePath.Replace(".cs", "_Godot.cs");

                // Write the converted Godot code to the new file
                File.WriteAllText(newFilePath, newCodeFile);

                Console.WriteLine("Conversion complete. Godot C# code written to " + newFilePath);
                Console.WriteLine("Be sure to double-check your file for errors.");
                Console.WriteLine("Additional adjustments will still be needed.");
            }
            catch (IOException e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
            */
        }
    }
}