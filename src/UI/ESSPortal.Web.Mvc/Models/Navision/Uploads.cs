using System.ComponentModel.DataAnnotations;

namespace EssPortal.Web.Mvc.Models.Navision;

public class Uploads
{
    [Key]
    public int Id { get; set; }
    public string? Path { get; set; }
    public string? File_Type { get; set; }
    public string? Application_No { get; set; }
    public string? File_Name { get; set; }
    public string? Uploadname { get; set; }
    public bool Is_Submitted { get; set; }
}
