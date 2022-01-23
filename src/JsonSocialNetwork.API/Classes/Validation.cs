using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace JsonSocialNetwork.API.Classes
{
    public class Validation
    {
        public static bool IsEmpty(params string?[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (string.IsNullOrEmpty(parameter))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsBool(params string?[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (!(parameter == "0" || parameter == "1"))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsID(params string[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (string.IsNullOrEmpty(parameter)) continue;
                if (!(int.TryParse(parameter, out int parsed) && parsed > 0))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsPhone(params string[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (!Regex.IsMatch(parameter, @"^0\d{9}$"))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsPassword(params string[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (!Regex.IsMatch(parameter, @"^[a-zA-Z0-9]{6,10}$"))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsNewPasswordValid(string currentPass, string newPass)
        {
            if (currentPass == newPass) return false;
            int currPassSize = currentPass.Length;
            int x = (int)(0.8 * newPass.Length);
            if (currPassSize >= x)
            {
                for (int i = 0; i < (currPassSize - x + 1); i++)
                {
                    string test = currentPass.Substring(i, x);
                    if (newPass.Contains(test))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        public static bool IsImage(params IFormFile[] files)
        {
            foreach (var file in files)
            {
                if (file.ContentType.Split('/')[0] != "image")
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsImageOversize(params IFormFile[] files)
        {
            foreach (var file in files)
            {
                if (file.Length > 5242880)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsVideo(params IFormFile[] files)
        {
            foreach (var file in files)
            {
                if (file.ContentType.Split('/')[0] != "video")
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsVideoOversize(params IFormFile[] files)
        {
            foreach (var file in files)
            {
                if (file.Length > 52428800)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
