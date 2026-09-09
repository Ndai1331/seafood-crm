using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Data.Wiki
{
    public class WikiNote
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("scope")] public string Scope { get; set; } = "team";
        [JsonProperty("ownerUserId")] public int? OwnerUserId { get; set; }
        [JsonProperty("folderPath")] public string FolderPath { get; set; } = "/";
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("slug")] public string Slug { get; set; } = string.Empty;
        [JsonProperty("bodyMd")] public string? BodyMd { get; set; }
        [JsonProperty("createdBy")] public int? CreatedBy { get; set; }
        [JsonProperty("createdDate")] public DateTime CreatedDate { get; set; }
        [JsonProperty("modifiedBy")] public int? ModifiedBy { get; set; }
        [JsonProperty("modifiedDate")] public DateTime ModifiedDate { get; set; }
        [JsonProperty("canEdit")] public bool CanEdit { get; set; }
    }

    public class WikiNoteListItem
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("scope")] public string Scope { get; set; } = "team";
        [JsonProperty("folderPath")] public string FolderPath { get; set; } = "/";
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("slug")] public string Slug { get; set; } = string.Empty;
        [JsonProperty("modifiedDate")] public DateTime ModifiedDate { get; set; }
        [JsonProperty("snippet")] public string? Snippet { get; set; }
    }

    public class WikiGraph
    {
        [JsonProperty("nodes")] public List<WikiGraphNode> Nodes { get; set; } = new();
        [JsonProperty("edges")] public List<WikiGraphEdge> Edges { get; set; } = new();
    }

    public class WikiGraphNode
    {
        [JsonProperty("id")] public long Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("slug")] public string Slug { get; set; } = string.Empty;
    }

    public class WikiGraphEdge
    {
        [JsonProperty("source")] public long Source { get; set; }
        [JsonProperty("target")] public long Target { get; set; }
    }
}
