using ConversionLogic;
namespace ConversionTests
{
    /// <summary>
    /// This class contains unit tests for the UnityToGodotConverter class.
    /// </summary>
    public class UnityToGodotConverterTests
    {
        /// <summary>
        /// A simple test to ensure that the testing system works.
        /// </summary>
        [Fact]
        public void TestSystemWorks()
        {
            Assert.True(1 == 1, "Test system failed to initialize");
        }

        /// <summary>
        /// Test to ensure that the conversion process works properly.
        /// </summary>
        /// <param name="inputFilePath">The file path to the input Unity code.</param>
        /// <param name="expectedFilePath">The file path to the expected Godot code.</param>
        [Theory]
        [InlineData("TestData/01-Hello World/HelloWorldUnity.cs", "TestData/01-Hello World/HelloWorldGodot.cs")]
        [InlineData("TestData/02-Life Cycle/LifeCycleUnity.cs", "TestData/02-Life Cycle/LifeCycleGodot.cs")]
        [InlineData("TestData/03-Transform/BasicSyntaxUnity.cs", "TestData/03-Transform/BasicSyntaxGodot.cs")]
        [InlineData("TestData/04-Sprites/SpriteUseUnity.cs", "TestData/04-Sprites/SpriteUseGodot.cs")]
        [InlineData("TestData/05-Components/UseComponentUnity.cs", "TestData/05-Components/UseComponentGodot.cs")]
        public void ConversionProperlyWorks(string inputFilePath, string expectedFilePath)
        {
            // Arrange: Read input and expected output from files.
            string unityCode = File.ReadAllText(inputFilePath);
            string expectedGodotCode = File.ReadAllText(expectedFilePath);

            var converter = new UnityToGodotConverter();

            // Act: Perform the conversion.
            string godotCode = converter.ConvertUnityCodeToGodot(unityCode);

            // Assert: Check if the conversion result matches the expected Godot code.
            Assert.Equal(expectedGodotCode, godotCode);
        }
    }
}