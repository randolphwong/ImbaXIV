using System;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;

namespace ImbaXIV
{
    public class ConfigManager
    {
        static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static readonly string ProjectName = Assembly.GetExecutingAssembly().GetName().Name;
        static readonly string ConfigFolderPath = Path.Combine(AppDataPath, ProjectName);
        static readonly string ConfigFilePath = Path.Combine(ConfigFolderPath, "config.xml");
        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static Config Load()
        {
            if (!Directory.Exists(ConfigFolderPath))
                Directory.CreateDirectory(ConfigFolderPath);

            XmlSerializer reader = new XmlSerializer(typeof(Config));
            StreamReader file;
            try
            {
                file = new StreamReader(ConfigFilePath);
            }
            catch (FileNotFoundException)
            {
                return new Config();
            }
            Config config = (Config)reader.Deserialize(file);
            file.Close();
            return config;
        }

        public static void Save(Config config)
        {
            if (!Directory.Exists(ConfigFolderPath))
                Directory.CreateDirectory(ConfigFolderPath);

            config.Version = Version;
            XmlSerializer writer = new XmlSerializer(typeof(Config));
            FileStream file = File.Create(ConfigFilePath);
            writer.Serialize(file, config);
            file.Close();
        }
    }
}
