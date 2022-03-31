using System;
using System.IO;
using System.Net;
using System.Text;

namespace RaidSystem
{
    class DiscordApi
    {

        public class DiscordMessage
        {
            public string Content { get; set; }
        }

        public static void SendMessage(string message)
        {
            var load = new DiscordMessage();
            load.Content = message;

            string json = SimpleJson.SimpleJson.SerializeObject(load);

            SendSerializedJson(json);
        }           

        internal static void SendSerializedJson(string json)
        {
            if (String.IsNullOrWhiteSpace(RaidSystem.WebhookUrl.Value)) return;

            byte[] bytes = Encoding.UTF8.GetBytes(json);

            WebRequest request = WebRequest.Create(RaidSystem.WebhookUrl.Value);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = bytes.Length;

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();

            using (dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

            }

            response.Close();
        }
    }
}
