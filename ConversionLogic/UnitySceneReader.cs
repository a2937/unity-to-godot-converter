using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace ConversionLogic
{
    public class UnitySceneReader
    {
        public Dictionary<string, object> ReadFile(string path)
        {

            using (var reader = new StreamReader(path))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                // Load YAML data into a Dictionary
                var yamlData = new Dictionary<string, object>();
                foreach (var document in yamlStream.Documents)
                {
                    if (document.RootNode is YamlMappingNode rootNode)
                    {
                        foreach (var entry in rootNode.Children)
                        {
                            if (entry.Key is YamlScalarNode keyNode)
                            {
                                // Convert YAML scalar node to string
                                string key = keyNode.Value;

                                // Convert YAML node to object
                                object value = GetValue(entry.Value);

                                // Add key-value pair to dictionary
                                yamlData[key] = value;
                            }
                        }
                    }
                }

                foreach (var kvp in yamlData)
                {
                    Console.WriteLine($"Section: {kvp.Key}");

                    if (kvp.Value is Dictionary<string, object> section)
                    {
                        foreach (var entry in section)
                        {
                            Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");

                            // If the value is another nested dictionary, you can access its values as well
                            if (entry.Value is Dictionary<string, object> nestedSection)
                            {
                                foreach (var nestedEntry in nestedSection)
                                {
                                    Console.WriteLine($"  Nested Key: {nestedEntry.Key}, Nested Value: {nestedEntry.Value}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                    }
                }

                return yamlData;
            }
        }

        // Recursively convert YAML node to object
        private static object GetValue(YamlNode node)
        {
            switch (node.NodeType)
            {
                case YamlNodeType.Scalar:
                    return ((YamlScalarNode)node).Value;
                case YamlNodeType.Sequence:
                    var sequence = new List<object>();
                    foreach (var item in ((YamlSequenceNode)node).Children)
                    {
                        sequence.Add(GetValue(item));
                    }
                    return sequence;
                case YamlNodeType.Mapping:
                    var mapping = new Dictionary<string, object>();
                    foreach (var entry in ((YamlMappingNode)node).Children)
                    {
                        if (entry.Key is YamlScalarNode keyNode)
                        {
                            string key = keyNode.Value;
                            object value = GetValue(entry.Value);
                            mapping[key] = value;
                        }
                    }
                    return mapping;
                default:
                    throw new NotSupportedException($"Unsupported YAML node type: {node.NodeType}");
            }
        }
    }
}
