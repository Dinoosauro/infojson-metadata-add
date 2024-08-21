namespace CommonLibrary
{
    public class CustomMetadataFormat
    {
        /// <summary>
        /// Gets the container TagLibSharp has identified for the current file
        /// </summary>
        /// <param name="type">The Type of the TagLib.Tag</param>
        /// <returns>A CustomMetadataType of the container, if a supported container for custom metadata is available</returns>
        public static CustomMetadataTypes? GetSuggestedContainer(string type)
        {
            string[] permittedTypes = System.Enum.GetNames(typeof(CustomMetadataTypes));
            foreach (string availableType in permittedTypes)
            {
                if (type.Contains(availableType.ToLower())) return (CustomMetadataTypes)Enum.Parse(typeof(CustomMetadataTypes),
                availableType);
            }
            return null;
        }
        /// <summary>
        /// The different types of container that support custom metadata
        /// </summary>
        public enum CustomMetadataTypes
        {
            APE,
            APPLE,
            ASF,
            ID3,
            MATROSKA,
            XIPH,
            PNG,
            RIFF,
            XMP
        }
        /// <summary>
        /// The void that adds the custom metadata to the supported containers
        /// <param name="type">The CustomMetadataTypes enum of the container</param>
        /// <param name="file">The TagLib.File where the new metadata will be applied</param>
        /// </summary>
        public static void UpdateCustomValueHandle(TagLib.File file, string key, string value)
        {
            switch (GetSuggestedContainer(file.TagTypes.ToString().ToLower()))
            {
                case CustomMetadataTypes.APE:
                    {
                        TagLib.Ape.Tag tag = (TagLib.Ape.Tag)file.GetTag(TagLib.TagTypes.Ape, true);
                        tag.SetValue(key, value);
                        break;
                    }
                case CustomMetadataTypes.APPLE:
                    {
                        TagLib.Mpeg4.AppleTag tag = (TagLib.Mpeg4.AppleTag)file.GetTag(TagLib.TagTypes.Apple, true);
                        tag.SetDashBox("com.apple.iTunes", key, value);
                        break;
                    }
                case CustomMetadataTypes.ASF:
                    {
                        TagLib.Asf.Tag tag = (TagLib.Asf.Tag)file.GetTag(TagLib.TagTypes.Asf, true);
                        tag.SetDescriptorString(value, [key]);
                        break;
                    }
                case CustomMetadataTypes.ID3:
                    {
                        TagLib.Id3v2.Tag metadata = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2, true);
                        TagLib.Id3v2.PrivateFrame privateFrame = TagLib.Id3v2.PrivateFrame.Get(metadata, key, true);
                        privateFrame.PrivateData = TagLib.ByteVector.FromString(value);
                        break;
                    }
                case CustomMetadataTypes.MATROSKA:
                    {

                        TagLib.Matroska.Tag tag = (TagLib.Matroska.Tag)file.GetTag(TagLib.TagTypes.Matroska, true);
                        tag.Set(key, null, value);
                        break;
                    }
                case CustomMetadataTypes.XIPH:
                    {
                        TagLib.Ogg.XiphComment tag = (TagLib.Ogg.XiphComment)file.GetTag(TagLib.TagTypes.Xiph, true);
                        tag.SetField(key, [value]);
                        break;
                    }
                case CustomMetadataTypes.PNG:
                    {
                        TagLib.Png.PngTag tag = (TagLib.Png.PngTag)file.GetTag(TagLib.TagTypes.Png, true);
                        tag.SetKeyword(key, value);
                        break;
                    }
                case CustomMetadataTypes.RIFF:
                    {
                        TagLib.Riff.MovieIdTag tag = (TagLib.Riff.MovieIdTag)file.GetTag(TagLib.TagTypes.MovieId, true);
                        tag.SetValue(TagLib.ByteVector.FromString(key), new TagLib.ByteVectorCollection(){
TagLib.ByteVector.FromString(value)
});
                        break;
                    }
                case CustomMetadataTypes.XMP:
                    {
                        TagLib.Xmp.XmpTag tag = (TagLib.Xmp.XmpTag)file.GetTag(TagLib.TagTypes.XMP, true);
                        tag.SetTextNode("", key, value);
                        break;
                    }
            }
        }



    }
}