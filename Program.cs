using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CITToFirmCSharp.Migrators;

namespace CITToFirmCSharp;

public class Program
{

    public static  string NewPath => Path.Combine(ZipDirectory, @"Hypixel+ Firmament\");
    public static string TempPath => Path.Combine(ZipDirectory, @"Hypixel+ Firmament Temp Dir\");
    public static string ItemIdsPath => Path.Combine(AppContext.BaseDirectory, "itemIds.json");

    public static  string ModelOutputPath => Path.Combine(NewPath, "assets", "firmskyblock", "models", "item");
    public static string TextureOutputPath => Path.Combine(NewPath, "assets", "hypixelplus", "textures", "item");
    public static string ArmorTextureOutputPath => Path.Combine(NewPath, "assets", "hypixelplus", "textures", "entity", "equipment");
    public static string ArmorOutputPath => Path.Combine(NewPath, "assets", "firmskyblock", "overrides", "armor_models");

    private static string _zipPath = string.Empty;
    public static bool CleanUp;
    private static string ZipDirectory => Path.GetDirectoryName(_zipPath)!;

    private static async Task Main(string[] args)
    {
        Console.WriteLine("Starting CITToFirmCSharp... (Created by GrahamKracker, based on ThatGravyBoat's kotlin script)");
        Console.WriteLine("This program is provided as is, with no warranty or guarantee of functionality. Use at your own risk.");
        Console.WriteLine("This program is not affiliated with Hypixel or Mojang in any way.");

        if(args.Length > 0)
            _zipPath = args[0];
        else
        {
            Console.WriteLine("Enter the path of the Hypixel+ zip file: ");
            _zipPath = Console.ReadLine() ?? throw new ArgumentException("No zip file path provided");
            _zipPath = _zipPath.Trim('"');
        }

        string outputFolder = string.Empty;

        if(args.Length > 1)
            outputFolder = args[1];
        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            Console.WriteLine(
                "Enter the output folder, usually the resourcepacks folder in your .minecraft, or hit enter to use the directory of the zip file: ");
            outputFolder = Console.ReadLine() ?? ZipDirectory;
            if (outputFolder == string.Empty)
                outputFolder = ZipDirectory;
            outputFolder = outputFolder.Trim('"');
        }

        CleanUp = true;
        if (args.Length > 2)
            CleanUp = bool.Parse(args[2]);

        var totalStopwatch = Stopwatch.StartNew();
        Console.WriteLine("Removing old directories...");
        var oldDirectoryStopwatch = Stopwatch.StartNew();
        if (Directory.Exists(NewPath))
        {
            Directory.Delete(NewPath, true);
        }
        Directory.CreateDirectory(NewPath);

        if (Directory.Exists(TempPath))
        {
            Directory.Delete(TempPath, true);
        }
        Console.WriteLine("Done removing old directories, took " + oldDirectoryStopwatch.ElapsedMilliseconds + "ms");


        Console.WriteLine("Extracting original zip file...");
        var extractStopwatch = Stopwatch.StartNew();
        ZipFile.ExtractToDirectory(_zipPath, TempPath);
        extractStopwatch.Stop();
        Console.WriteLine("Done extracting, took " + extractStopwatch.ElapsedMilliseconds + "ms");

        Directory.CreateDirectory(ModelOutputPath);
        Directory.CreateDirectory(TextureOutputPath);
        Directory.CreateDirectory(ArmorTextureOutputPath);
        Directory.CreateDirectory(ArmorOutputPath);

        await new ItemMigrator().Run();

        foreach (var file in Directory.GetFiles(Path.Combine(TempPath), "*", SearchOption.TopDirectoryOnly))
        {
            File.Move(file, Path.Combine(NewPath, Path.GetFileName(file)));
        }

        ItemMigrator.UpdateMaxSupportedFormat();

        string zipPath;
        int i = 0;
        while (true)
        {
            zipPath = Path.Combine(
                outputFolder,
                new DirectoryInfo(NewPath).Name + $"_{i}.zip");
            try
            {
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                break;
            }
            catch (Exception)
            {
                i++;
                if (i == 10) throw;
            }
        }

        var zipStopWatch = Stopwatch.StartNew();
        Console.WriteLine("Beginning zipping " + Path.GetFullPath(zipPath));
        ZipFile.CreateFromDirectory(NewPath, zipPath, CompressionLevel.NoCompression, false);
        zipStopWatch.Stop();
        Console.WriteLine(
            $"Done zipping to {zipPath}, took {zipStopWatch.ElapsedMilliseconds}ms");

        if (CleanUp)
        {
            Console.WriteLine("Cleaning up...");
            var cleanupStopwatch = Stopwatch.StartNew();
            Directory.Delete(TempPath, true);
            Directory.Delete(NewPath, true);
            cleanupStopwatch.Stop();
            Console.WriteLine("Cleaned up in " + cleanupStopwatch.ElapsedMilliseconds + "ms");
        }
        else
        {
            foreach (var directory in Directory.GetDirectories(TempPath, "*", SearchOption.AllDirectories).Reverse())
            {
                if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                    Directory.Delete(directory);
            }
        }

        totalStopwatch.Stop();
        Console.WriteLine("Total time elapsed: " + totalStopwatch.ElapsedMilliseconds + "ms");

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}