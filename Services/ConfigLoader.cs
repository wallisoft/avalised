using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ConfigUI.Models;

namespace ConfigUI.Services;

public class ConfigLoader
{
    public ConfigDefinition LoadFromFile(string filePath)
    {
        var yaml = File.ReadAllText(filePath);
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        
        var config = deserializer.Deserialize<ConfigDefinition>(yaml);
        return config;
    }
}
