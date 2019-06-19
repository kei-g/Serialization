using System;

namespace SnowStep.IO
{
    [Flags]
    public enum FileStreamAttributes
    {
        None = 0,
        ModifiedWhenRead = 1,
        ContainsSecurity = 2,
        ContainsProperties = 4,
        Sparse = 8,
    }
}
