using System.Reflection;
using System.Resources;
using CommonLibrary;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Settings settings = new();
        /// <summary>
        /// A Dictionary, with the a string[] of the possible arguments as the key, and the property name of the Settings class to edit
        /// </summary>
        Dictionary<string[], string> SettingValues = new()
        {
            [["--extra"]] = "AddExtraFields",
            [["--prefer-tags"]] = "UseTagsInsteadOfCategory",
            [["--thumbnail-convert", "--albumart-convert", "-c"]] = "ReEncodeAlbumArt",
            [["--description-as-comment"]] = "AddDescriptionInCommentTag",
            [["--description-in-description"]] = "AddDescriptionInDescriptionTag",
            [["--albumart-quality", "--thumbnail-quality", "-q"]] = "AlbumArtQuality",
            [["--fulldate", "--full-date", "-d"]] = "AddFullDate",
            [["--purl"]] = "AddYtDlpPURL",
            [["--subfolders", "-s"]] = "LookInSubfolders",
            [["--files-extension", "--file-extension", "-f"]] = "FileExtension",
            [["--download-album-art", "--download-thumbnail", "-t"]] = "DownloadAlbumArt"
        };
        string[] availableArgs = SettingValues.SelectMany(file => file.Key).ToArray();
        for (int i = 1; i < args.Length; i++)
        {
            if (availableArgs.Contains(args[i]))
            {
                string? propertyToEdit = SettingValues.First(val => val.Key.Contains(args[i])).Value; // Get the property name that needs to be edited
                if (propertyToEdit != null)
                {
                    PropertyInfo? propertyInfo = settings.GetType().GetProperty(propertyToEdit);
                    object? getValue = propertyInfo?.GetValue(settings, null);
                    if (propertyInfo == null) continue;
                    switch (getValue?.GetType()?.ToString())
                    {
                        case "System.Int32":
                            if (int.TryParse(args[i + 1], out int result)) propertyInfo.SetValue(settings, result);
                            break;
                        case "System.String":
                            propertyInfo.SetValue(settings, args[i + 1]);
                            Console.WriteLine(args[i + 1]);
                            break;
                        case null:
                            break;
                        default:
                            propertyInfo.SetValue(settings, args[i + 1] != "false" && args[i + 1] != "n");
                            break;
                    }
                }
            }
        }
        if (args[0].Equals("--folder", StringComparison.CurrentCultureIgnoreCase) || args[0].Equals("-f", StringComparison.CurrentCultureIgnoreCase)) // Get all the items in the folder
        {
            /// <summary>
            /// A Dictionary that contains the file name as a key, and the list of the available file extensions as a value
            /// </summary>
            Dictionary<string, List<string>> fileExtensions = [];
            string[] AvailableFiles = Directory.GetFiles(args[1], "*.*", settings.LookInSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (string suggestedFile in AvailableFiles)
            {
                string fileName = GetRealFileName(suggestedFile);
                string extensionName = GetRealExtension(suggestedFile);
                if (fileExtensions.TryGetValue(fileName, out List<string> extensions)) extensions.Add(extensionName); else fileExtensions[fileName] = [extensionName];
            }
            foreach (var file in fileExtensions) // Add value to each element
            {
                if (file.Value.Contains("." + settings.FileExtension) && file.Value.Contains(".info.json"))
                {
                    string? imageExtension = file.Value.Contains(".webp") ? ".webp" : file.Value.Contains(".jpg") ? ".jpg" : file.Value.Contains(".jpeg") ? ".jpeg" : file.Value.Contains(".png") ? ".png" : null;
                    byte[] ImageBytes = imageExtension != null ? File.ReadAllBytes(file.Key + imageExtension) : [];
                    try
                    {
                        await UpdateMetadata.WriteTags(File.ReadAllText(file.Key + ".info.json"), TagLib.File.Create(file.Key + "." + settings.FileExtension), ImageBytes, "image/" + imageExtension == ".webp" ? "webp" : imageExtension == "png" ? "png" : "jpeg", settings, Callback, file.Key + "." + settings.FileExtension);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Callback(new InformationCallback(GravityType.ERROR, "Failed startup for: " + file + "." + settings.FileExtension));
                    }
                }
            }
        }
        else if (args[0].Equals("--thumbnail", StringComparison.CurrentCultureIgnoreCase)) // Provide info.json path, video path, thumbnail path and mimetype
        {
            await UpdateMetadata.WriteTags(File.ReadAllText(args[1]), TagLib.File.Create(args[2]), File.ReadAllBytes(args[3]), args[4], settings, Callback, args[2]);
        }
        else // Standard method: provide info.json path and video path
        {
            await UpdateMetadata.WriteTags(File.ReadAllText(args[1]), TagLib.File.Create(args[2]), settings, Callback, args[2]);
        }
    }
    /// <summary>
    /// Get the extension of a file
    /// </summary>
    /// <param name="str">The file name</param>
    /// <returns>The file extension</returns>
    private static string GetRealExtension(string str)
    {
        if (str.EndsWith(".info.json")) return ".info.json";
        return str[str.LastIndexOf('.')..];
    }
    /// <summary>
    /// Get the file name of a file, without the extension
    /// </summary>
    /// <param name="str">The file name, with the extension</param>
    /// <returns>The file name, without the extension</returns>
    private static string GetRealFileName(string str)
    {
        if (str.EndsWith(".info.json")) return str[..str.LastIndexOf(".info.json")];
        return str[..str.LastIndexOf('.')];
    }
    private static void Callback(InformationCallback callback)
    {
        Console.WriteLine(callback.Gravity == GravityType.ERROR ? "🛑" : callback.Gravity == GravityType.WARNING ? "⚠️" : "ℹ️" + "  " + callback.Message);
    }
}