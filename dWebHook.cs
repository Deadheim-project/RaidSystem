// Decompiled with JetBrains decompiler
// Type: dWebHook
// Assembly: RaidSystem, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DB90A6E5-D9AE-469D-B808-D30088C3F40A
// Assembly location: C:\Users\Werner\Downloads\wernermayer-Deadheim-6.0.11\RaidSystem.dll

using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;

public class dWebHook : IDisposable
{
    private readonly WebClient dWebClient;
    private static NameValueCollection discordValues = new NameValueCollection();

    public string WebHook { get; set; }

    public string UserName { get; set; }

    public string ProfilePicture { get; set; }

    public dWebHook() => this.dWebClient = new WebClient();

    public void SendMessage(string msgSend) => Task.Run((Func<Task>)(async () =>
    {
        dWebHook.discordValues = new NameValueCollection();
        this.ProfilePicture = "https://www.diariodepernambuco.com.br/static/app/noticia_127983242361/2016/09/06/663700/20160906094608464704o.jpg";
        this.UserName = "Cachorro";
        this.WebHook = "https://discord.com/api/webhooks/973618519159762974/O_jAehuGpNSdUlKiiG-CSxpFiuIYK_kCtdZVrW5dgIFOqB51A2GXI4tJawjGofur3DMa";
        dWebHook.discordValues.Add("username", this.UserName);
        dWebHook.discordValues.Add("avatar_url", this.ProfilePicture);
        dWebHook.discordValues.Add("content", msgSend);
        this.dWebClient.UploadValues(this.WebHook, dWebHook.discordValues);
    }));

    public void Dispose() => this.dWebClient.Dispose();
}
