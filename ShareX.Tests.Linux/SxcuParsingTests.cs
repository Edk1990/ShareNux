using Newtonsoft.Json;
using ShareX.UploadersLib;
using Xunit;

namespace ShareX.Tests.Linux;

public class SxcuParsingTests
{
    [Fact]
    public void CanDeserializeStandardSxcuConfiguration()
    {
        string sxcuJson = """
        {
          "Version": "15.0.0",
          "Name": "Custom Host",
          "DestinationType": "ImageUploader",
          "RequestMethod": "POST",
          "RequestURL": "https://example.com/api/upload",
          "Headers": {
            "Authorization": "Bearer sample-token"
          },
          "Body": "MultipartFormData",
          "FileFormName": "file",
          "URL": "{json:data.url}",
          "ThumbnailURL": "{json:data.thumb}",
          "DeletionURL": "{json:data.delete_url}"
        }
        """;

        CustomUploaderItem? item = JsonConvert.DeserializeObject<CustomUploaderItem>(sxcuJson);

        Assert.NotNull(item);
        Assert.Equal("Custom Host", item.Name);
        Assert.Equal(CustomUploaderDestinationType.ImageUploader, item.DestinationType);
        Assert.Equal(ShareX.UploadersLib.HttpMethod.POST, item.RequestMethod);
        Assert.Equal("https://example.com/api/upload", item.RequestURL);
        Assert.Equal("file", item.FileFormName);
        Assert.Equal("{json:data.url}", item.URL);
        Assert.Equal("Bearer sample-token", item.Headers?["Authorization"]);
    }

    [Fact]
    public void CanDeserializeAndMigrateLegacySyntaxSxcu()
    {
        // Legacy ShareX syntax using $json:path$ with older version
        string legacyJson = """
        {
          "Version": "13.0.0",
          "Name": "Legacy Host",
          "RequestMethod": "POST",
          "RequestURL": "https://example.com/api/upload",
          "URL": "$json:data.link$",
          "DeletionURL": "$json:data.deletehash$"
        }
        """;

        CustomUploaderItem? item = JsonConvert.DeserializeObject<CustomUploaderItem>(legacyJson);
        Assert.NotNull(item);
        
        item.CheckBackwardCompatibility(); // Triggers migration from $json:path$ to {json:path}

        Assert.Equal("Legacy Host", item.Name);
        Assert.Equal("{json:data.link}", item.URL);
        Assert.Equal("{json:data.deletehash}", item.DeletionURL);
    }
}
