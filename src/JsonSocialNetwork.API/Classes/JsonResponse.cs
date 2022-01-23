using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace JsonSocialNetwork.API.Classes
{
    public class JsonResponse
    {
        public JsonResponse(int code)
        {
            Code = code.ToString();
            Message = GetMessage(code);
        }

        public static string GetCode(int code)
            => _responses.ContainsKey(code) ? code.ToString() : "1005";
        public static string GetMessage(int code)
            => _responses.TryGetValue(code, out string? message) ? message : "Unknown error.";

        [JsonPropertyName("code")]
        public string Code { get; init; }

        [JsonPropertyName("message")]
        public string Message { get; init; }


        private static readonly Dictionary<int, string> _responses = new()
        {
            [1005] = "Unknown error.",
            [1000] = "OK",
            [9992] = "Post is not existed",
            [9993] = "Code verify is incorrect",
            [9994] = "No Data or end of list data",
            [9995] = "User is not validated",
            [9996] = "User existed.",
            [9997] = "Method is invalid",
            [9998] = "Token is invalid.",
            [9999] = "Exception error.",
            [1001] = "Can not connect to DB.",
            [1002] = "Parameter is not enough.",
            [1003] = "Parameter type is invalid.",
            [1004] = "Parameter value is invalid.",
            [1006] = "File size is too big.",
            [1007] = "Upload File Failed!.",
            [1008] = "Maximum number of images.",
            [1009] = "Not access.",
            [1010] = "Action has been done previously by this user."
        };
    }
}
