
using Azure.Core;
using DemoProject.Domain.DTOs;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DemoProject.Domain.Helpers
{
    public static class CommonHelper
    {
     
        public static async Task<PageListDTO<T>> ToPagingAsync<T>(this IQueryable<T> query, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var totalRecords = await query.CountAsync(ct);
            var totalPages = (totalRecords % pageSize == 0) ? (totalRecords / pageSize) : (totalRecords / pageSize + 1);
            var list = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return new PageListDTO<T>
            {
                Items = list,
                TotalPage = totalPages
            };

        }
        public static string ToSerializeJson<T>(this T toSerialize)
        {
            try
            {
                return JsonConvert.SerializeObject(toSerialize);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task<T> SendMutationWithCorrectResponseAsync<T>(this GraphQLHttpClient client, GraphQL.GraphQLRequest request) { 
           
            var dataJson= (await client.SendMutationAsync<JObject>(request)).Data;

            return dataJson.First.First.ToObject<T>();

        }
        public static async Task<T> SendQueryWithCorrectResponseAsync<T>(this GraphQLHttpClient client, GraphQL.GraphQLRequest request)
        {
            var dataJson = (await client.SendQueryAsync<JObject>(request)).Data;
           
            return dataJson.First.First.ToObject<T>();
        }


    }


}
