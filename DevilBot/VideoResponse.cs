using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public class VideoResponse
{
    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("start")]
    public int Start { get; set; }

    [JsonProperty("per_page")]
    public int PerPage { get; set; }

    [JsonProperty("page")]
    public int Page { get; set; }

    [JsonProperty("time_ms")]
    public int TimeMs { get; set; }

    [JsonProperty("total_count")]
    public int TotalCount { get; set; }

    [JsonProperty("total_pages")]
    public int TotalPages { get; set; }

    [JsonProperty("videos")]
    public List<Video> Videos { get; set; } = new();
}

public class Video
{
    [JsonProperty("id")]
    public string Id { get; set; } = "";

    [JsonProperty("title")]
    public string Title { get; set; } = "";

    [JsonProperty("keywords")]
    public string Keywords { get; set; } = "";

    [JsonProperty("views")]
    public int Views { get; set; }

    [JsonProperty("rate")]
    public string Rate { get; set; } = "";

    [JsonProperty("url")]
    public string Url { get; set; } = "";

    [JsonProperty("added")]
    public string Added { get; set; } = "";

    [JsonProperty("length_sec")]
    public int LengthSec { get; set; }

    [JsonProperty("length_min")]
    public string LengthMin { get; set; } = "";

    [JsonProperty("embed")]
    public string Embed { get; set; } = "";

    [JsonProperty("default_thumb")]
    public Thumbnail DefaultThumb { get; set; } = new();

    [JsonProperty("thumbs")]
    public List<Thumbnail> Thumbs { get; set; } = new();
}

public class Thumbnail
{
    [JsonProperty("size")]
    public string Size { get; set; } = "";

    [JsonProperty("width")]
    public int Width { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

    [JsonProperty("src")]
    public string Src { get; set; } = "";
}
