using NUnit.Framework;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApiTests
{
    [TestFixture]
    public class PatchObjectTests
    {
        private RestClient client;
        private string baseUrl = "https://api.restful-api.dev";

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

        // Helper method to send PATCH
        private RestResponse SendPatch(string endpoint, object? body = null)
        {
            var request = new RestRequest(endpoint, Method.Patch);

            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body);
                request.AddStringBody(json, DataFormat.Json);
                TestContext.Out.WriteLine("Request Body: " + json);
                TestContext.Out.WriteLine("Request EndPoint: " + endpoint);
            }

            var response = client.Execute(request);

            TestContext.Out.WriteLine("Status Code: " + response.StatusCode);
            TestContext.Out.WriteLine("Response Body: " + response.Content);

            return response;
        }

        // ---------------- Positive Test Cases ----------------

        [Test]
        public void TC01_UpdateSingleField_ShouldReturn200()
        {
            var body = new { name = "Updated Phone" };
            var response = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2", body);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(response.Content != null && response.Content.Contains("Updated Phone"));
        }

        [Test]
        public void TC02_UpdateMultipleFields_ShouldReturn200()
        {
            var body = new { name = "Updated Phone", data = new { color = "black" } };
            var response = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2", body);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(response.Content != null && response.Content.Contains("black"));
        }

        [Test]
        public void TC03_UpdateWithSpecialCharacters_ShouldReturn200()
        {
            var body = new { name = "Phone @123" };
            var response = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2", body);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public void TC04_UpdateNumericField_ShouldReturn200()
        {
            var body = new { data = new { year = 2025 } };
            var response = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2", body);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(response.Content != null && response.Content.Contains("2025"));
        }

        // ---------------- Negative Test Cases ----------------

        [Test]
        public void TC05_UpdateNonExistingId_ShouldReturn404()
        {
            var body = new { name = "Invalid Update" };
            var response = SendPatch("/objects/999999", body);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void TC06_EmptyRequestBody_ShouldReturn400()
        {
            var response = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public void TC0ff8081819782e69e01995abea5e51ca2_InvalidJsonFormat_ShouldReturn400()
        {
            var request = new RestRequest("/objects/ff8081819782e69e01995abea5e51ca2", Method.Patch);
            request.AddStringBody("{ name: MissingQuotes }", DataFormat.Json);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public void TC08_InvalidFieldName_ShouldBeIgnoredOr200()
        {
            var body = new { invalidField = "test" };
            var response = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2", body);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }


        // ---------------- Security / Edge Cases ----------------

        [Test]
        public void TC10_SQLInjectionAttempt_ShouldNotExecuteSQL()
        {
            var body = new { name = "DROP TABLE users;" };
            var response = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2", body);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(response.Content != null && response.Content.Contains("DROP TABLE"));
        }

        [Test]
        public void TC11_XSSAttempt_ShouldStoreSafely()
        {
            var body = new { name = "<script>alert('hack')</script>" };
            var response = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2", body);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(response.Content != null && response.Content.Contains("script"));
        }

        [Test]
        public void TC12_NoAuth_ShouldReturn401_IfRequired()
        {
            // If API requires Auth, this will fail, otherwise it passes
            var request = new RestRequest("/objects/ff8081819782e69e01995abea5e51ca2", Method.Patch);
            request.AddJsonBody(new { name = "NoAuthTest" });

            var response = client.Execute(request);

            Assert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Unauthorized
            );
        }

        // ---------------- Performance / Concurrency ----------------

        [Test]
        public void TC13_ConcurrentUpdates_ShouldNotFail()
        {
            var tasks = new List<System.Threading.Tasks.Task<RestResponse>>();

            for (int i = 0; i < 10; i++)
            {
                var body = new { name = $"Concurrent Update {i}" };
                var request = new RestRequest("/objects/ff8081819782e69e01995abea5e51ca2", Method.Patch);
                request.AddJsonBody(body);

                tasks.Add(client.ExecuteAsync(request));
            }

            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

            foreach (var t in tasks)
            {
                Assert.That(t.Result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
        }

        [Test]
        public void TC14_FrequentUpdates_ShouldPersistLatest()
        {
            var body1 = new { name = "Update1" };
            var body2 = new { name = "Update2" };

            var resp1 = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2", body1);
            var resp2 = SendPatch("/objects/ff8081819782e69e01995abea5e51ca2", body2);

            Assert.That(resp2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsTrue(resp2.Content != null && resp2.Content.Contains("Update2"));
        } 
    }
}
