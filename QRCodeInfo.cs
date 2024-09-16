using YamlDotNet.Serialization;

public class QRCodeInfo
{
    public int TotalQRCodes { get; set; }
    public bool IsCompressed { get; set; }
    public string? FileSHA256 { get; set; }

    public string ToYaml()
    {
        var serializer = new Serializer();
        return serializer.Serialize(this);
    }
}
