using System.Text.Json.Serialization;

namespace MovieCatalogExam.Models;

public class ApiResponseDto
{
    [JsonPropertyName("msg")]

    public string Msg { get; set; }

    [JsonPropertyName("movie")]

    public MovieDto Movie { get; set; } = new MovieDto(); 
}