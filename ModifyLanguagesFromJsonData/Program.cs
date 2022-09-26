using System.Net.Http;
using System.Collections;
using System;
using System.Collections.Generic;
using Nancy.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace ModifyLanguagesFromJsonData
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Please write path of file");
            var path = Console.ReadLine();
            if (!File.Exists(path))
                Console.WriteLine("Path is not valid");
            else
            {
              string JsonText = await File.ReadAllTextAsync(path);

                try
                {
                    var jToken = JToken.Parse(JsonText);
                    Console.WriteLine("Select language you want to translate\nUzbek = 1\nEnglish = 2");
                    var TypeLanguage = Console.ReadLine();
                    switch (TypeLanguage)
                    {
                        case "1": await ReadAllValues(jToken, "uz");break;
                        case "2": await ReadAllValues(jToken, "en");break;
                        default: Console.WriteLine("Type of language is not correct");break;
                    }
                    Console.WriteLine(jToken);
                    string TranslatedJtoken = jToken.ToString();
                    Console.WriteLine("Ma'lumotlar filega yozilsinmi\nHa = 1\nYo\'q = 2");

                    var choose = Console.ReadLine();
                    if(choose == "1")
                    {
                        switch (TypeLanguage)
                        {
                            case "1":
                                using (StreamWriter writer = new StreamWriter("Uz.json"))
                                {
                                    await writer.WriteLineAsync(TranslatedJtoken);
                                }

                                break;
                            case "2":
                                using (StreamWriter writer = new StreamWriter("Eng.json"))
                                {
                                    await writer.WriteLineAsync(TranslatedJtoken);
                                }

                                break;
                            default: Console.WriteLine("Type of language is not correct"); break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ma\'lumotlar filega yozilmadi");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        public static class Translate
        {
            public static  async Task<string> Translator(string text, string toLg)
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://translo.p.rapidapi.com/api/v3/translate"),
                    Headers =
                {
                   { "X-RapidAPI-Key", "0fff634702mshe9b242b86f3ddc2p13b5c4jsne0c488cdfb9a" },
                   { "X-RapidAPI-Host", "translo.p.rapidapi.com" },
                },
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                  { "to", $"{toLg}" },
                  { $"text", $"{text}" },
                }),
                };
                string translated = string.Empty;
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var ObjectResponse = JsonConvert.DeserializeObject<Response>(body);
                    translated = ObjectResponse.translated_text; 
                }
                return translated;
            }
        }
        public static async Task ReadAllValues(JToken jtoken,string ToLanguage)
        {

            if (jtoken != null)
            {
                if (jtoken.Children().Any())
                {
                    foreach (var child in jtoken.Children())
                    {
                        await ReadAllValues(child, ToLanguage);
                        if (child.Type == JTokenType.Property)
                        {
                            var property = (JProperty)child;
                            if (!property.Value.HasValues)
                            {
                                property.Value = await Translate.Translator(property.Value.ToString(), ToLanguage);
                            }
                        }
                    }
                }
            }
        }
        public static string TranslateText(string input)
        {
            // Set the language from/to in the url (or pass it into this function)
            string url = String.Format
            ("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
             "en", "uz", Uri.EscapeUriString(input));
            HttpClient httpClient = new HttpClient();
            string result = httpClient.GetStringAsync(url).Result;

            // Get all json data
            var jsonData = new JavaScriptSerializer().Deserialize<List<dynamic>>(result);

            // Extract just the first array element (This is the only data we are interested in)
            var translationItems = jsonData as IEnumerable;

            // Translation Data
            string translation = "";

            // Loop through the collection extracting the translated objects
            foreach (object item in translationItems)
            {
                // Convert the item array to IEnumerable
                IEnumerable translationLineObject = item as IEnumerable;

                // Convert the IEnumerable translationLineObject to a IEnumerator
                if(item != null)
                {
                IEnumerator translationLineString = translationLineObject.GetEnumerator();

                    
                // Get first object in IEnumerator
                translationLineString.MoveNext();

                // Save its value (translated text)
                translation += string.Format(" {0}", Convert.ToString(translationLineString.Current));
                }
            }

            // Remove first blank character
            if (translation.Length > 1) { translation = translation.Substring(1); };

            // Return translation
            return translation;
        }

    }
}
