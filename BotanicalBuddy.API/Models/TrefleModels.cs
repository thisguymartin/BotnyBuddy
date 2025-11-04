namespace BotanicalBuddy.API.Models;

public class TreplePlant
{
    public int Id { get; set; }
    public string? CommonName { get; set; }
    public string? ScientificName { get; set; }
    public string? FamilyCommonName { get; set; }
    public string? ImageUrl { get; set; }
    public string? Slug { get; set; }
}

public class TreplePlantDetail
{
    public int Id { get; set; }
    public string? CommonName { get; set; }
    public string? ScientificName { get; set; }
    public string? Family { get; set; }
    public string? Genus { get; set; }
    public string? Observations { get; set; }
    public bool Vegetable { get; set; }
}

public class TrefleLinks
{
    public string? Self { get; set; }
    public string? First { get; set; }
    public string? Last { get; set; }
    public string? Next { get; set; }
}

public class TrefleMeta
{
    public int Total { get; set; }
    public string? LastModified { get; set; }
}

public class TrefleSearchResponse
{
    public List<TreplePlant> Data { get; set; } = new();
    public TrefleLinks? Links { get; set; }
    public TrefleMeta? Meta { get; set; }
}

public class TrefleDetailResponse
{
    public TreplePlantDetail? Data { get; set; }
    public TrefleMeta? Meta { get; set; }
}
