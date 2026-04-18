using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using MovieCatalogExam.Models;   

namespace Movie 

{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string createdMovieId; 

        private const string BaseUrl = "http://144.91.123.158:5000";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI2YzhkYjAzMC1iYmJlLTRiNGYtYmEwNy1jMjUyZDcyMjgxOTIiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjQ5OjU1IiwiVXNlcklkIjoiNWYwYmU2OTAtYjE3Yi00Y2E3LTYyODQtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJsdWNoaWFiQGV4YW1wbGUuY29tIiwiVXNlck5hbWUiOiJsdWNoaWFiIiwiZXhwIjoxNzc2NTE2NTk1LCJpc3MiOiJNb3ZpZUNhdGFsb2dfQXBwX1NvZnRVbmkiLCJhdWQiOiJNb3ZpZUNhdGFsb2dfV2ViQVBJX1NvZnRVbmkifQ.DOQz7P46taib63WVBc0Re1i8gnWuUdo8ionv-YLT_sQ"; 

        private const string LoginEmail = "luchiab@example.com";
        private const string LoginPassword = "123456";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Test, Order(1)]
        public void CreateMovie_ShouldWork()
        {
            var request = new RestRequest("/api/Movie/Create", Method.Post);

            var body = new
            {
                title = "Test Movie",
                description = "Test Description"
            };

            request.AddJsonBody(body);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(result.Movie, Is.Not.Null);
            Assert.That(result.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Msg, Is.EqualTo("Movie created successfully!"));

            createdMovieId = result.Movie.Id;
        }

        [Test, Order(2)]
        public void EditMovie_ShouldWork()
        {
            var request = new RestRequest($"/api/Movie/Edit?movieId={createdMovieId}", Method.Put);

            var body = new
            {
                title = "Edited Movie",
                description = "Edited Description"
            };

            request.AddJsonBody(body);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(result.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Test, Order(3)]
        public void GetAllMovies_ShouldReturnList()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var movies = JsonSerializer.Deserialize<List<MovieDto>>(response.Content);

            Assert.That(movies, Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteMovie_ShouldWork()
        {
            var request = new RestRequest($"/api/Movie/Delete?movieId={createdMovieId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(result.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateMovie_WithoutRequiredFields_ShouldFail()
        {
            var request = new RestRequest("/api/Movie/Create", Method.Post);

            request.AddJsonBody(new { });

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void Edit_NonExistingMovie_ShouldFail()
        {
            var request = new RestRequest("/api/Movie/Edit?movieId=invalid-id", Method.Put);

            request.AddJsonBody(new
            {
                title = "Test",
                description = "Test"
            });

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(result.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Test, Order(7)]
        public void Delete_NonExistingMovie_ShouldFail()
        {
            var request = new RestRequest("/api/Movie/Delete?movieId=invalid-id", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(result.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}