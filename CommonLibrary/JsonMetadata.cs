using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using TagLib;

namespace CommonLibrary;

public class JsonFile
{
    public string? id { get; set; }
    public string? title { get; set; }
    public string? thumbnail { get; set; }
    public string? description { get; set; }
    public string? channel_id { get; set; }
    public string? channel_url { get; set; }
    public object? view_count { get; set; }
    public string[]? categories { get; set; }
    public string[]? tags { get; set; }
    public object? comment_count { get; set; }
    public object? like_count { get; set; }
    public string? channel { get; set; }
    public object? channel_follower_count { get; set; }
    public string? uploader_id { get; set; }
    public string? upload_date { get; set; }
    public string? fulltitle { get; set; }
    public string? webpage_url { get; set; }
}
public class UpdateMetadata
{
    public static async Task WriteTags(string JsonFile, TagLib.File TagFile, Settings settings, Action<InformationCallback> callback, string contentName)
    {
        await WriteTags(JsonFile, TagFile, [], "", settings, callback, contentName);
    }
    /// <summary>
    /// Write the metadata in the provided TagLib.File, and save it.
    /// </summary>
    /// <param name="JsonFile">The string that contains the JSON file</param>
    /// <param name="TagFile">The TagLib.File of the video/audio file</param>
    /// <param name="Picture">A byte array of the image content</param>
    /// <param name="PictureMimeType">The mimetype of the provided image</param>
    /// <param name="settings">Custom options</param>
    /// <param name="callback">The Action to call to provide information</param>
    /// <param name="contentName">The path of the video/audio file</param>
    public static async Task WriteTags(string JsonFile, TagLib.File TagFile, byte[] Picture, string PictureMimeType, Settings settings, Action<InformationCallback> callback, string contentName)
    {
        JsonFile? JsonParsed = null;
        try
        {
            JsonParsed = System.Text.Json.JsonSerializer.Deserialize<JsonFile>(JsonFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            callback(new InformationCallback(GravityType.ERROR, "Failed decoding the JSON file of: " + contentName));
        }
        if (JsonParsed == null)
        {
            callback(new InformationCallback(GravityType.ERROR, "Failed decoding the JSON file of: " + contentName));
            return;
        }
        TagFile.Tag.Title = JsonParsed.fulltitle ?? JsonParsed.title;
        if (JsonParsed.channel != null)
        {
            TagFile.Tag.AlbumArtists = [JsonParsed.channel];
            TagFile.Tag.Performers = [JsonParsed.channel];
        }
        TagFile.Tag.Comment = settings.AddDescriptionInCommentTag ? JsonParsed.description : JsonParsed.webpage_url;
        if (settings.AddDescriptionInDescriptionTag) TagFile.Tag.Description = JsonParsed.description;
        TagFile.Tag.Genres = settings.UseTagsInsteadOfCategory && JsonParsed.tags != null ? JsonParsed.tags : JsonParsed.categories ?? ([]);
        if (Picture.Length == 0 && settings.DownloadAlbumArt && JsonParsed.thumbnail != null)
        {
            try
            {
                Picture = await new HttpClient().GetByteArrayAsync(JsonParsed.thumbnail);
                callback(new InformationCallback(GravityType.INFORMATION, "Downloaded: " + JsonParsed.thumbnail));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                callback(new InformationCallback(GravityType.WARNING, "-- FAILED THUMBNAIL DOWNLOAD (" + JsonParsed.thumbnail + ") --"));
            }
        }
        if (Picture.Length > 0)
        {
            if (settings.ReEncodeAlbumArt)
            {
                using Image image = Image.Load(Picture);
                using MemoryStream stream = new();
                JpegEncoder encoder = new()
                {
                    Quality = settings.AlbumArtQuality
                };
                image.SaveAsJpeg(stream, encoder);
                Picture = stream.ToArray();
            }
            TagLib.Picture picture = new()
            {
                Type = TagLib.PictureType.Other,
                MimeType = settings.ReEncodeAlbumArt ? "image/jpeg" : PictureMimeType ?? "image/webp",
                Data = Picture
            };
            TagFile.Tag.Pictures = [picture];
        }
        if (uint.TryParse(settings.AddFullDate ? JsonParsed.upload_date?.ToString() : JsonParsed.upload_date?[..4], out uint result)) TagFile.Tag.Year = result;
        if (settings.AddExtraFields)
        {
            Dictionary<string, string?> updateFields = new() // The name of the custom metadata as a key, the value of the metadata as a value
            {
                ["video_id"] = JsonParsed.id,
                ["thumbnail_url"] = JsonParsed.thumbnail,
                ["channel_id"] = JsonParsed.channel_id,
                ["channel_url"] = JsonParsed.channel_url,
                ["followers"] = JsonParsed.channel_follower_count?.ToString(),
                ["views"] = JsonParsed.view_count?.ToString(),
                ["categories"] = JsonParsed.categories != null ? string.Join(", ", JsonParsed.categories) : null,
                ["tags"] = JsonParsed.tags != null ? string.Join(", ", JsonParsed.tags) : null,
                ["comment_count"] = JsonParsed.comment_count?.ToString(),
                ["likes"] = JsonParsed.like_count?.ToString(),
                ["fulldate"] = JsonParsed.upload_date,
                ["uploader_id"] = JsonParsed.uploader_id,
                ["webpage_url"] = JsonParsed.webpage_url
            };
            foreach (var entry in updateFields) if (entry.Value != null) CustomMetadataFormat.UpdateCustomValueHandle(TagFile, entry.Key, entry.Value);
        }
        else if (settings.AddYtDlpPURL && JsonParsed.webpage_url != null)
        {
            CustomMetadataFormat.UpdateCustomValueHandle(TagFile, "PURL", JsonParsed.webpage_url);
        }
        TagFile.Save();
        callback(new InformationCallback(GravityType.INFORMATION, "Updated file: " + contentName));
        TagFile.Dispose();
    }
}