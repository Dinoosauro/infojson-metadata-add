namespace CommonLibrary;
public class Settings()
{
    /// <summary>
    /// Add fields in custom metadata
    /// </summary>
    public bool AddExtraFields { get; set; } = true;
    /// <summary>
    /// For the genre tag, use the video's tags (added by the user) instead of the video category (added by the content provider I think)
    /// </summary>
    public bool UseTagsInsteadOfCategory { get; set; } = false;
    /// <summary>
    /// Re-encode the album art
    /// </summary>
    public bool ReEncodeAlbumArt { get; set; } = true;
    /// <summary>
    /// Add the video description in the "Comment" tag
    /// </summary>
    public bool AddDescriptionInCommentTag { get; set; } = false;
    /// <summary>
    /// Add the video description in the "Description" tag
    /// </summary>
    public bool AddDescriptionInDescriptionTag { get; set; } = true;
    /// <summary>
    /// The number, from 1 to 100, of the JPEG quality for album art re-encoding
    /// </summary>
    public int AlbumArtQuality { get; set; } = 75;
    /// <summary>
    /// Add the full date (YYYYMMDD) to the file, instead of YYYY
    /// </summary>
    public bool AddFullDate { get; set; } = false;
    /// <summary>
    /// Add the URL in the "PURL" tag, that is usually added by yt-dlp when embedding metadata
    /// </summary>
    public bool AddYtDlpPURL { get; set; } = true;
    /// <summary>
    /// Get files also from the subfolders
    /// </summary>
    public bool LookInSubfolders { get; set; } = true;
    /// <summary>
    /// Download the Album Art if nothing is provided
    /// </summary>
    public bool DownloadAlbumArt { get; set; } = false;
    /// <summary>
    /// The file extension of the video/audio file to look in a directory
    /// </summary>
    public string FileExtension { get; set; } = "webm";
}