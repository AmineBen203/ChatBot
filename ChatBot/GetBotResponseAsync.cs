using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace ChatBot
{
    public class BotService
    {
        int bati;
        public async Task<string> GetBotResponseAsync(string prompt)
        {
            var client = new HttpClient();
            var apiKey = "YOUR_API_TOKEN";
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "gpt-4", // Or "gpt-4"
                prompt = prompt,
                max_tokens = 150
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            Console.WriteLine(response);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                Console.Write(responseContent);
                return jsonResponse.choices[0].text.ToString().Trim();
                

            }

            return bati.ToString();
        }
    }
}
