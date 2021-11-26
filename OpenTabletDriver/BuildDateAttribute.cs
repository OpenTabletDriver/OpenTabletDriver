using System;

public class BuildDateAttribute : Attribute
{
    public static string BuildDate { private set; get; }
    public BuildDateAttribute(string buildDate)
    {
        BuildDate = buildDate;
    }
}