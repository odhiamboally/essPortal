namespace EssPortal.Web.Mvc.Utilities.CustomAttributes;


[AttributeUsage(AttributeTargets.Property)]
public class QueryParameterNameAttribute : Attribute
{
    public string Name { get; }

    public QueryParameterNameAttribute(string name)
    {
        Name = name;
    }
}
