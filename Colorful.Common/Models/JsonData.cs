using System;
using System.Collections.Generic;
using System.Linq;

namespace Colorful.Models
{
    /// <summary>
    /// 通用树状类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonData<T>
    {
        public T id { get; set; }
        public string text { get; set; }
        public List<JsonData<T>> children { get; set; }
    }

    public interface IDataSource
    {
        List<JsonData<string>> Datas { get; }
    }
}