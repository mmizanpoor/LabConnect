using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.DocumentEditor;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class SyncfusionController : ControllerBase
{
    [HttpPost("Import")]
    public string Import(IFormCollection data)
    {
        if (data.Files.Count == 0)
            return string.Empty;

        using var stream = new MemoryStream();
        var file = data.Files[0];
        var index = file.FileName.LastIndexOf('.');
        var type = index > -1 && index < file.FileName.Length - 1
            ? file.FileName[index..]
            : ".docx";
        file.CopyTo(stream);
        stream.Position = 0;

        var document = WordDocument.Load(stream, GetFormatType(type.ToLowerInvariant()));
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
        document.Dispose();
        return json;
    }

    [HttpPost("SystemClipboard")]
    public string SystemClipboard([FromBody] SyncfusionClipboardParameter param)
    {
        if (string.IsNullOrWhiteSpace(param.Content))
            return string.Empty;

        try
        {
            var document = WordDocument.LoadString(param.Content, GetFormatType(param.Type.ToLowerInvariant()));
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
            document.Dispose();
            return json;
        }
        catch
        {
            return string.Empty;
        }
    }

    [HttpPost("RestrictEditing")]
    public string[]? RestrictEditing([FromBody] SyncfusionRestrictParameter param)
    {
        if (string.IsNullOrWhiteSpace(param.PasswordBase64))
            return null;

        return WordDocument.ComputeHash(param.PasswordBase64, param.SaltBase64, param.SpinCount);
    }

    private static FormatType GetFormatType(string format)
    {
        return format switch
        {
            ".dotx" or ".docx" or ".docm" or ".dotm" => FormatType.Docx,
            ".dot" or ".doc" => FormatType.Doc,
            ".rtf" => FormatType.Rtf,
            ".txt" => FormatType.Txt,
            ".xml" => FormatType.WordML,
            ".html" => FormatType.Html,
            _ => throw new NotSupportedException("EJ2 DocumentEditor does not support this file format."),
        };
    }
}

public class SyncfusionClipboardParameter
{
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class SyncfusionRestrictParameter
{
    public string PasswordBase64 { get; set; } = string.Empty;
    public string SaltBase64 { get; set; } = string.Empty;
    public int SpinCount { get; set; }
}
