using System.Text.Json;
using System.Xml.Linq;

namespace ConfigurationMigration
{
    public class WindowConfig
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public WindowConfig()
        {
            Top = 0;
            Left = 0;
            Width = 400;
            Height = 150;
        }
    }

    public class LoginConfig
    {
        public string Name { get; set; }
        public Dictionary<string, WindowConfig> Windows { get; set; }

        public LoginConfig()
        {
            Windows = new Dictionary<string, WindowConfig>();
        }
    }

    public class ConfigFile
    {
        public Dictionary<string, LoginConfig> Logins { get; set; }

        public ConfigFile()
        {
            Logins = new Dictionary<string, LoginConfig>();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            string xmlFilePath = @"C:\Users\User\source\repos\NET02.2\config.xml";
            string jsonDirectory = @"C:\Users\User\source\repos\NET02.2\Config";

            ConfigFile configFile = ReadConfigFile(xmlFilePath);

            foreach (var loginConfig in configFile.Logins.Values)
            {
                Console.WriteLine($"Login: {loginConfig.Name}");
                foreach (var windowConfigPair in loginConfig.Windows)
                {
                    string windowTitle = windowConfigPair.Key;
                    WindowConfig windowConfig = windowConfigPair.Value;
                    Console.WriteLine($"{windowTitle}({windowConfig.Top}, {windowConfig.Left}, {windowConfig.Width}, {windowConfig.Height})");
                }
            }

            MigrateToJSON(configFile, jsonDirectory);
        }

        public static ConfigFile ReadConfigFile(string filePath)
        {
            ConfigFile configFile = new ConfigFile();

            XDocument xmlConfig = XDocument.Load(filePath);
            var loginElements = xmlConfig.Descendants("login");

            foreach (var loginElement in loginElements)
            {
                LoginConfig loginConfig = new LoginConfig();
                loginConfig.Name = loginElement.Attribute("name")?.Value;

                var windowElements = loginElement.Descendants("window");
                foreach (var windowElement in windowElements)
                {
                    string windowTitle = windowElement.Attribute("title")?.Value;
                    WindowConfig windowConfig = new WindowConfig();

                    var topElement = windowElement.Element("top");
                    if (topElement != null)
                        windowConfig.Top = int.Parse(topElement.Value);

                    var leftElement = windowElement.Element("left");
                    if (leftElement != null)
                        windowConfig.Left = int.Parse(leftElement.Value);

                    var widthElement = windowElement.Element("width");
                    if (widthElement != null)
                        windowConfig.Width = int.Parse(widthElement.Value);

                    var heightElement = windowElement.Element("height");
                    if (heightElement != null)
                        windowConfig.Height = int.Parse(heightElement.Value);

                    loginConfig.Windows.Add(windowTitle, windowConfig);
                }

                configFile.Logins.Add(loginConfig.Name, loginConfig);
            }

            return configFile;
        }

        public static void MigrateToJSON(ConfigFile configFile, string jsonDirectory)
        {
            Directory.CreateDirectory(jsonDirectory);

            foreach (var loginConfigPair in configFile.Logins)
            {
                string loginName = loginConfigPair.Key;
                LoginConfig loginConfig = loginConfigPair.Value;

                string jsonFilePath = Path.Combine(jsonDirectory, loginName, "config.json");
                Directory.CreateDirectory(Path.GetDirectoryName(jsonFilePath));

                string json = JsonSerializer.Serialize(loginConfig);
                File.WriteAllText(jsonFilePath, json);
            }
        }
    }
}