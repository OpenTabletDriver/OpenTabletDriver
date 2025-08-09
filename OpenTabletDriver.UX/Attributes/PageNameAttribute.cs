using System;

namespace OpenTabletDriver.UX.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PageNameAttribute : Attribute
    {
        public PageNameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
