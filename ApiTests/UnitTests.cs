using System.Net;
using RestSharp;
using NUnit.Compatibility;
using Newtonsoft.Json;
#nullable enable
namespace ApiTests;

[TestFixture]
public class UnitTests
{
    private RestClient client;
    private string baseUrl = "https://restful-api.dev";

    [SetUp]
    public void Setup()
    {
        client = new RestClient(baseUrl);
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        client = null;
    }

    [Test]
    public void Patch_UpdateName_ShouldReturn200()
    {
        Console.WriteLine("Hello Debugging");
        var payload = new { name = "Updated Name" };
        var jsonBody = JsonConvert.SerializeObject(payload);
        //var request = new RestRequest("")
        var request = new RestRequest("/objects/7", Method.Post);
        //request.AddJsonBody(new { name = "Updated Name 1" });
        request.AddStringBody(jsonBody, DataFormat.Json);

        //Console.WriteLine("Request Body" + request.);
        // Log request details
        Console.WriteLine("----- HTTP REQUEST -----");
        Console.WriteLine($"{request.Method} {client.BuildUri(request)}");
        foreach (var header in request.Parameters.Where(p => p.Type == ParameterType.HttpHeader))
        {
            Console.WriteLine($"{header.Name}: {header.Value}");
        }
        Console.WriteLine();
        Console.WriteLine("Request Body: " + jsonBody);
        Console.WriteLine("------------------------");

        // Send request
        RestResponse response = client.Execute(request);
        Console.WriteLine("Response Status Code: " + response.StatusCode);
        Console.WriteLine("Response Content: " + response.Content);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.IsTrue(response.Content != null && response.Content.Contains("Updated Name 1"));
    }

    [Test]
    public void Patch_InvalidId_ShouldReturn404()
    {
        var request = new RestRequest("/objects/999999", Method.Patch);
        request.AddJsonBody(new { name = "Invalid Update" });

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public void Patch_EmptyBody_ShouldReturn400()
    {
        var request = new RestRequest("/objects/7", Method.Patch);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
