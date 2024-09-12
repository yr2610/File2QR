using YamlDotNet.Serialization;

public class QRCodeInfo
{
    public int TotalQRCodes { get; set; }
    public int ChunkSize { get; set; }

    public string ToYaml()
    {
        var serializer = new Serializer();
        return serializer.Serialize(this);
    }
}
