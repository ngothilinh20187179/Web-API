using System;

namespace JsonSocialNetwork.API.Classes
{
    public class Formatter
    {
        public enum Result { OK, WRONG_FORMAT, EMPTY }

        public static string PostedTime(DateTime dateTime)
            => dateTime.ToString("HH:mm dd/MM/yy");

        public static Result TryParseId(string? id, out int result, bool includeZero = false)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(id)) return Result.EMPTY;
            return (int.TryParse(id, out result) && (result > (includeZero ? -1 : 0))) ? Result.OK : Result.WRONG_FORMAT;
        }

        public static Result TryParseIds(string? ids, out int[] results, bool includeZero = false)
        {
            results = Array.Empty<int>();
            if (string.IsNullOrWhiteSpace(ids)) return Result.EMPTY;
            results = new int[ids.Length];
            string[] stringIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            foreach (string id in stringIds)
            {
                if (!int.TryParse(id, out int parsedId) && parsedId > (includeZero ? -1 : 0))
                {
                    return Result.WRONG_FORMAT;
                }
                results[count++] = parsedId;
            }
            return Result.OK;
        }
    }
}
