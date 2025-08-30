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
    using var responseMessage = await client.GetAsync(fetchUrl, HttpCompletionOption.ResponseContentRead);
    using var packageStream = await responseMessage.Content.ReadAsStreamAsync();
    await WriteFileAsync(packageStream, packageCachePath);
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
        await WriteFileAsync(entryStream, entryOutputPath);
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
        try
        {
            Directory.CreateDirectory(cacheDir);
        }
        catch
        {
            if (Directory.Exists(cacheDir))
            {
                return;
            }
            throw;
        }
    }
}

static async Task WriteFileAsync(Stream stream, string filePath)
{
    const int RetryCount = 4;

    //尝试在并行的情况下正常执行
    for (var i = 1; i <= RetryCount; i++)
    {
        try
        {
            using var fileStream = File.OpenWrite(filePath);
            await stream.CopyToAsync(fileStream);
        }
        catch
        {
            if (i == RetryCount)
            {
                throw;
            }
            else
            {
                Thread.Sleep(RetryCount * 100);
            }
        }
    }
}
