using System.Text.RegularExpressions;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

var azureStorageConnectionString = app.Configuration.GetValue<string>("AzureStorageConnectionString");

var secrets = new Secrets();
// vai pegar informações do appsettings e jogar dentro da classe Secrets
app.Configuration.GetSection("Secrets").Bind(secrets);

app.MapGet("/", () => new {
    ConnectionString = connectionString,
    Secrets = secrets,
    ApiUrl = app.Configuration.GetValue<string>("ApiUrl")
});

app.MapPost("/", (Upload model) => {
    var url = UploadBase64Image(model.Image, "user-images");

    return Results.Ok(url);
});

app.Run();

string UploadBase64Image(string base64Image, string container)
{
    var fileName = Guid.NewGuid().ToString() + ".jpg";
    var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(base64Image, "");

    byte[] imageBytes = Convert.FromBase64String(data);
    var blobClient = new BlobClient(azureStorageConnectionString, container, fileName);

    using var stream = new MemoryStream(imageBytes);

    blobClient.Upload(stream);

    return blobClient.Uri.AbsoluteUri; // url final da imagem
}

public record Upload(string Image);

public class Secrets
{
    public string JwtTokenSecret { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}