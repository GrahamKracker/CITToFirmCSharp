using System.Diagnostics;
using System.IO.Compression;
using MosaicoSolutions.CSharpProperties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CITToFirmCSharp;

internal class Program
{
    private const string BasePath = @"C:\Users\gjguz\source\repos\Minecraft\CITToFirmCSharp\";
    private static readonly string OldPath = Path.Combine(BasePath, @"Hypixel+ 0.20.7 for 1.21.1\");
    private static readonly string NewPath = Path.Combine(BasePath, @"New Hypixel+\");
    private static readonly string VanillaPath = Path.Combine(BasePath, @"Vanilla Resource Pack\");

    private static string _zipPath = string.Empty;

    private async static Task Main(string[] args)
    {

        if (Directory.Exists(NewPath))
        {
            Directory.Delete(NewPath, true);
        }

        Directory.CreateDirectory(NewPath);

        var fileActionsStopwatch = Stopwatch.StartNew();

        CopyImages();

        CopyModels();

        HandleItems();

        HandleArmor();

        CopyTopLevelFiles();

        int i = 0;
        while (true)
        {
            _zipPath = Path.Combine(
                @"C:\Users\gjguz\AppData\Roaming\PrismLauncher\instances\1.21 Hypixel\.minecraft\resourcepacks\", new DirectoryInfo(NewPath).Name + $"_{i}.zip");
            try
            {
                if (File.Exists(_zipPath))
                {
                    File.Delete(_zipPath);
                }

                break;
            }
            catch (Exception)
            {
                i++;
                if (i == 10) throw;
            }
        }

        fileActionsStopwatch.Stop();
        Console.WriteLine($"handling files took {fileActionsStopwatch.Elapsed}s");

        var zipStopWatch = Stopwatch.StartNew();
        Console.WriteLine("beginning zipping " + Path.GetFileName(_zipPath));

        ZipFile.CreateFromDirectory(NewPath, _zipPath, CompressionLevel.NoCompression, false);
        zipStopWatch.Stop();

        Console.WriteLine($"Done zipping to {Path.GetFileName(_zipPath)}, took {zipStopWatch.Elapsed}s \nCurrent Time: {DateTime.Now:hh:mm:ss tt}");
    }

    private static void HandleItems()
    {
        Parallel.ForEach(Directory.GetFiles(Path.Combine(OldPath, @"assets\minecraft\optifine\cit\skyblock\"), "*.properties", SearchOption.AllDirectories), file =>
        {
            try
            {
                var javaProperties = Properties.Load(file);

                if(javaProperties.ContainsKey("type") && javaProperties["type"] != "item")
                {
                    return;
                }

                Dictionary<string, string> properties = new();
                foreach ((string? key, string? value) in javaProperties)
                {
                    properties.Add(key, value);
                }

                JObject fileObj = new JObject();

                var jsonFile = new FileInfo(Path.ChangeExtension(file, ".json"));
                if (jsonFile.Exists)
                {
                    JObject jsonObj = JObject.Parse(File.ReadAllText(Path.ChangeExtension(file, ".json")));

                    if (jsonObj.TryGetValue("parent", out var value))
                    {
                        fileObj["parent"] = "firmskyblock:" + value;
                    }
                }

                if (!fileObj.ContainsKey("parent"))
                {
                    if (properties.TryGetValue("model", out string? parent))
                    {
                        fileObj["parent"] = "firmskyblock:" + parent;
                    }
                    else
                    {
                        if (properties.TryGetValue("items", out string item))
                        {
                            var vanillaFile = Path.Combine(VanillaPath, @"assets\minecraft\models\item\", item.Split(":")[1] + ".json");
                            if (!File.Exists(vanillaFile))
                            {
                                Console.WriteLine("No vanilla file found at: " + vanillaFile.Split(VanillaPath)[1]);
                                Console.WriteLine("File: " + file.Split(OldPath)[1]);
                                return;
                            }
                            var vanillaObj = JObject.Parse(File.ReadAllText(vanillaFile));
                            fileObj["parent"] = vanillaObj["parent"];
                        }
                        else
                        {
                            Console.WriteLine("No items found for file: " + file);
                        }
                    }
                }


                JObject textures = new JObject();
                if (properties.TryGetValue("texture", out string? texture))
                {
                    textures.Add(new JProperty("layer0", "firmskyblock:item" + Path.ChangeExtension(
                        file.Split(OldPath)[1].Replace(@"assets\minecraft\optifine\cit", @""), "")
                        .Replace(@"\", "/").TrimEnd('.')));
                }
                else
                {
                    if(File.Exists(Path.ChangeExtension(file, ".png")))
                    {
                        textures.Add(new JProperty("layer0",
                            "firmskyblock:item" + Path.ChangeExtension(file.Split(OldPath)[1]
                                    .Replace(@"assets\minecraft\optifine\cit", @""),
                                "").Replace(@"\", "/").TrimEnd('.')));
                    }
                }

                fileObj["textures"] = textures;

                var newFile =
                    new FileInfo(Path.ChangeExtension(Path.Combine(NewPath, @"assets\firmskyblock\models\item\", Path.GetFileName(file)), ".json"));
                newFile.Directory?.Create();
                File.WriteAllText(newFile.FullName, fileObj.ToString(Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error with file: " + file);
                Console.WriteLine(ex);
            }
        });
    }

    private static void HandleArmor()
    {
        Parallel.ForEach(Directory.GetFiles(Path.Combine(OldPath, @"assets\minecraft\optifine\cit\skyblock\"), "*.properties", SearchOption.AllDirectories), file =>
        {
            /*try
            {
                var javaProperties = Properties.Load(file);

                if(!javaProperties.ContainsKey("type") || javaProperties["type"] != "armor")
                {
                    return;
                }

                Dictionary<string, string> properties = new();
                foreach ((string? key, string? value) in javaProperties)
                {
                    properties.Add(key, value);
                }

                JObject fileObj = new JObject();



                    if (properties.TryGetValue("components.custom_data.id", out string? skyblockId))
                    {
                        fileObj["parent"] = "firmskyblock:armor/" + skyblockId;
                    }
                    else
                    {
                        Console.WriteLine("No id found for file: " + file);
                    }
                    {
                        fileObj["parent"] = "firmskyblock:" + parent;
                    }
                    else
                    {
                        if (properties.TryGetValue("items", out string item))
                        {
                            var vanillaFile = Path.Combine(VanillaPath, @"assets\minecraft\models\item\", item.Split(":")[1] + ".json");
                            if (!File.Exists(vanillaFile))
                            {
                                Console.WriteLine("No vanilla file found at: " + vanillaFile.Split(VanillaPath)[1]);
                                Console.WriteLine("File: " + file.Split(OldPath)[1]);
                                return;
                            }
                            var vanillaObj = JObject.Parse(File.ReadAllText(vanillaFile));
                            fileObj["parent"] = vanillaObj["parent"];
                        }
                        else
                        {
                            Console.WriteLine("No items found for file: " + file);
                        }
                    }

                JArray layers = new JArray();


                var newFile =
                    new FileInfo(Path.ChangeExtension(Path.Combine(NewPath, @"assets\firmskyblock\models\item\", Path.GetFileName(file)), ".json"));
                newFile.Directory?.Create();
                File.WriteAllText(newFile.FullName, fileObj.ToString(Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error with file: " + file);
                Console.WriteLine(ex.Message);
            }*/
        });
    }

    private static void CopyTopLevelFiles()
    {
        Parallel.ForEach(Directory.GetFiles(Path.Combine(OldPath), "*", SearchOption.TopDirectoryOnly), file =>
        {
            var newFile =
                new FileInfo(Path.Combine(NewPath, file.Split(OldPath)[1]));
            newFile.Directory?.Create();

            File.Copy(file, newFile.FullName, true);
        });
    }

    private static void CopyModels()
    {
        Parallel.ForEach(
            Directory.GetFiles(Path.Combine(OldPath, @"assets\minecraft\models"), "*.json",
                SearchOption.AllDirectories), file =>
            {
                JObject jsonObj = JObject.Parse(File.ReadAllText(Path.ChangeExtension(file, ".json")));

                if(!jsonObj.TryGetValue("parent", out var value))
                {
                    jsonObj["parent"] = Path.GetFileName(file).Split(".")[0] switch
                    {
                        { } s when s.Contains("handheld_abiphone_") => "firmskyblock:item/hplus_handheld_abiphone",
                        { } s when s.Contains("hplus_block_a") => "firmskyblock:item/hplus_block",
                        { } s when s.Contains("hplus_mirrored_block") => "firmskyblock:item/hplus_block",
                        { } s when s.Contains("handheld") => "firmskyblock:item/hplus_handheld",
                        _ => "firmskyblock:item/hplus_generated"
                    };
                }


                var newFile =
                    new FileInfo(Path.Combine(NewPath, file.Split(OldPath)[1].Replace("minecraft", "firmskyblock")));
                newFile.Directory?.Create();

                File.WriteAllText(newFile.FullName, jsonObj.ToString(Formatting.Indented));
            });
    }

    private static void CopyImages()
    {
        Parallel.ForEach(
            Directory.GetFiles(Path.Combine(OldPath, @"assets\minecraft\optifine\cit\skyblock\"), "*.png",
                SearchOption.AllDirectories), file =>
            {
                var newFile =
                    new FileInfo(Path.Combine(NewPath, file.Split(OldPath)[1])
                        .Replace(@"minecraft\optifine\cit", @"firmskyblock\textures\item"));
                newFile.Directory?.Create();

                File.Copy(file, newFile.FullName, true);
            });

        Parallel.ForEach(
            Directory.GetFiles(Path.Combine(OldPath, @"assets\minecraft\optifine\cit\ui\"), "*.png",
                SearchOption.AllDirectories), file =>
            {
                var newFile =
                    new FileInfo(Path.Combine(NewPath, file.Split(OldPath)[1])
                        .Replace(@"minecraft\optifine\cit", @"firmskyblock\textures\item"));
                newFile.Directory?.Create();

                File.Copy(file, newFile.FullName, true);
            });
    }


    private static string[] GetRelativeFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.GetFiles(path, searchPattern, searchOption)
            .Select(file => file.Split(path)[1])
            .ToArray();
    }
}