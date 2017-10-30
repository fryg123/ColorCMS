using System;
using System.Collections.Generic;
using System.Text;

namespace Colorful.Json
{
    #region IJson
    public interface IJson
    {
        string ToJson<T>(T target);
        T Parse<T>(string json);
    }
    #endregion
}
