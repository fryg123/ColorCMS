using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Colorful.Models
{
    public class Country : Base
    {
        public string Id { get; set; }
        public string Zone { get; set; }
        public string Name { get; set; }
        public string NameEN { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
        public string AreaCode { get; set; }
        public string Area { get; set; }
        public string Domain { get; set; }
    }

    public class AreaBase
    {
        public string id { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
    }

    public class CountryJson : AreaBase
    {
        public List<Province> province { get; set; }
    }

    public class Province : AreaBase
    {
        public List<City> city { get; set; }
    }

    public class City : AreaBase
    {
    }
}