using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Core.Extension
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(this T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static List<T> CloneList<T>(this  IEnumerable<T> sources)
        {
            var cloneList =
            new List<T>();
            foreach (var item in sources)
            {
                cloneList.Add(item.Clone());
            }

            return cloneList;
        }

        public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(this HttpRequestMessage req)
        {
            HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);

            clone.Content = req.Content;
            // // Copy the request's content (via a MemoryStream) into the cloned object
            // var ms = new MemoryStream();
            // if (req.Content != null)
            // {
            //     await req.Content.CopyToAsync(ms).ConfigureAwait(false);
            //     ms.Position = 0;
            //     clone.Content = new StreamContent(ms);
            //
            //     // Copy the content headers
            //     foreach (var h in req.Content.Headers)
            //         clone.Content.Headers.Add(h.Key, h.Value);
            // }


            clone.Version = req.Version;

            foreach (KeyValuePair<string, object?> option in req.Options)
                clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

            // foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
            //     clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clone;
        }
        
    }

    public static class ClaimPrincipalExtensions
    {
        public static bool HasClaim(this ClaimsPrincipal source,string type,string value,bool demo)
        {
            var claim = source.Claims.FirstOrDefault(x =>x.Type ==type);
            if (claim == null) return false;
         
            var values = claim.Value.Split(new []{','}).ToList();
            if (values.FirstOrDefault(x=>x.Contains(value)) !=null) return true;
            return false;
        }
    }
}