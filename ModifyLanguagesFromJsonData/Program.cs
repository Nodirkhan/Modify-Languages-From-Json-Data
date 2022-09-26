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
    }
}
