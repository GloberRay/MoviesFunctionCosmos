using Newtonsoft.Json;
using System;

namespace Entities
{
    public class Movie
    {
        [JsonProperty(PropertyName ="id")]
        public string Id { get; set; }

        public string Title { get; set; }
        public string Category { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRegistered { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
