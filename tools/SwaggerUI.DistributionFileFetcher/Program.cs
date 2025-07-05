var version = args[0];
var outputDir = args.Length > 1 ? args[1] : null;
var cacheDir = args.Length > 2 ? args[2] : null;

if (!Version.TryParse(version, out _))
{
    Console.WriteLine($"Error version: {version}");
    return 1;
}

var packageFileName = $"swagger-ui-dist-{version}.tgz";

if (string.IsNullOrWhiteSpace(cacheDir))
{
    cacheDir = Path.Combine(Environment.CurrentDirectory, ".dist_cache");
}
EnsureDirectory(cacheDir);

var packageCachePath = Path.Combine(cacheDir, packageFileName);

if (!File.Exists(packageCachePath))
{
    var fetchUrl = $"https://registry.npmjs.org/swagger-ui-dist/-/{packageFileName}";
    Console.WriteLine($"Start fetch {fetchUrl}");

    using var client = new HttpClient();
    var packageData = await client.GetByteArrayAsync(fetchUrl);
    File.WriteAllBytes(packageCachePath, packageData);
}

if (string.IsNullOrWhiteSpace(outputDir))
{
    outputDir = Path.Combine(Environment.CurrentDirectory, ".dist_unpack", version);
}
EnsureDirectory(outputDir);

Console.WriteLine($"Start unpack {packageCachePath} into {outputDir}");

try
{
    //System.Formats.Tar.TarReader unsupported read now

    using var reader = SharpCompress.Readers.Tar.TarReader.Open(File.OpenRead(packageCachePath));
    while (reader.MoveToNextEntry()
           && reader.Entry is { } entry
           && !string.IsNullOrWhiteSpace(entry.Key)
           && !entry.IsDirectory)
    {
        var relativeDir = Path.GetDirectoryName(entry.Key) ?? string.Empty;
        var fileName = Path.GetFileName(entry.Key)!;
        var entryOutputDir = Path.Combine(outputDir, relativeDir);
        var entryOutputPath = Path.Combine(entryOutputDir, fileName);

        if (File.Exists(entryOutputPath))
        {
            Console.WriteLine($"Skip existed file -> \"{entryOutputPath}\"");
            continue;
        }
        EnsureDirectory(entryOutputDir);

        using var entryStream = reader.OpenEntryStream();
        using var fileStream = File.OpenWrite(entryOutputPath);
        await entryStream.CopyToAsync(fileStream);
    }
}
catch
{
    // remove package if unpack failed
    File.Delete(packageCachePath);
    throw;
}
Console.WriteLine("Distribution file fetch finished");

return 0;

static void EnsureDirectory(string cacheDir)
{
    if (!Directory.Exists(cacheDir))
    {
        Directory.CreateDirectory(cacheDir);
    }
}
