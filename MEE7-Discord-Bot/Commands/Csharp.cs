using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Newtonsoft.Json;
using System;
using System.Compat.Web;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;

namespace MEE7.Commands
{
    public class Csharp : Command
    {
        public Csharp() : base("c#", "Run csharp code", isExperimental: false, isHidden: true)
        {
            HelpMenu = DiscordNETWrapper.CreateEmbedBuilder("C# Help",
                "You can either post a C# expression, content for a Main() method or a whole program with one class having a Main() Function.\n" +
                "System, System.Linq and System.Collections.Generic are imported by default").
                AddFieldDirectly("Program Template:", @"```
public class Program
{
	public static void Main()
	{
		Console.WriteLine(""Hello World"");
    }
}```");
        }

        public override void Execute(IMessage message)
        {
            var code = message.Content.Split(" ").Skip(1).Combine(" ").Trim('`');
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = ~DecompressionMethods.All
            };
            using var httpClient = new HttpClient(handler);
            using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://dotnetfiddle.net/Home/Run");

            request.Headers.TryAddWithoutValidation("authority", "dotnetfiddle.net");
            request.Headers.TryAddWithoutValidation("accept", "application/json, text/javascript, */*; q=0.01");
            request.Headers.TryAddWithoutValidation("x-requested-with", "XMLHttpRequest");
            request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36");
            request.Headers.TryAddWithoutValidation("origin", "https://dotnetfiddle.net");
            request.Headers.TryAddWithoutValidation("sec-fetch-site", "same-origin");
            request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            request.Headers.TryAddWithoutValidation("referer", "https://dotnetfiddle.net/");
            request.Headers.TryAddWithoutValidation("accept-language", "de-DE,de;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.TryAddWithoutValidation("cookie", "AreCookiesResetForPublicMain=true; uvts=95553ad1-9ab5-48ee-6246-9c3da9c33468; NuGetPackageVersionIds=; Language=CSharp; IsAutoRun=false; UseLocalStorage=true; Console=true; OriginalFiddleId=CsScript; ProjectType=Script");
            request.Headers.TryAddWithoutValidation("content-type", "application/json; charset=UTF-8");

            var codeBlock = HttpUtility.HtmlEncode(code);

            if (!codeBlock.Contains("public static void Main", StringComparison.OrdinalIgnoreCase))
                if (codeBlock.Contains("Console.Write"))
                    codeBlock = "using System;\\nusing System.Linq;\\nusing System.Collections.Generic;\\n\\t\\t\\t\\t\\t\\n" +
                    "public class Program\\n{\\n\\tpublic static void Main()\\n\\t{\\n\\t\\t" + codeBlock + "\\n\\t}\\n}";
                else
                    codeBlock = "using System;\\nusing System.Linq;\\nusing System.Collections.Generic;\\n\\t\\t\\t\\t\\t\\n" +
                    "public class Program\\n{\\n\\tpublic static void Main()\\n\\t{\n\t\tConsole.WriteLine(" + codeBlock + ");\n\t}\\n}";

            request.Content = new StringContent("{\"CodeBlock\":\"" + codeBlock + "\",\"OriginalCodeBlock\":\"using System;\\n\\t\\t\\t\\t\\t\\npublic class Program\\n{\\n\\tpublic static void Main()\\n\\t{\\n\\t\\tConsole.WriteLine(\\\"Hello World\\\");\\n\\t}\\n}\",\"Language\":\"CSharp\",\"Compiler\":\"Net45\",\"ProjectType\":\"Console\",\"OriginalFiddleId\":\"CsCons\",\"NuGetPackageVersionIds\":\"\",\"OriginalNuGetPackageVersionIds\":\"\",\"TimeOffset\":\"2\",\"ConsoleInputLines\":[],\"MvcViewEngine\":\"Razor\",\"MvcCodeBlock\":{\"Model\":\"\",\"View\":\"\",\"Controller\":\"\"},\"OriginalMvcCodeBlock\":{\"Model\":\"\",\"View\":\"\",\"Controller\":\"\"},\"UseResultCache\":false}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=UTF-8");

            var response = httpClient.SendAsync(request).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;

            var json = JsonConvert.DeserializeObject<ResponseJson>(responseJson);

            DiscordNETWrapper.SendEmbed(
                DiscordNETWrapper.CreateEmbedBuilder("C# Runner Output", "", "", message.Author).
                AddFieldDirectly("StdOut", $"```{(json.ConsoleOutput.Length == 0 ? "No output was generated" : new string(json.ConsoleOutput.Take(500).ToArray()))}```").
                AddFieldDirectly("Stats", $"CompileTime: {json.Stats.CompileTime}\nCpuUsage: {json.Stats.CpuUsage}\n" +
                    $"ExecuteTime: {json.Stats.ExecuteTime}\nMemoryUsage: {json.Stats.MemoryUsage}"), message.Channel).Wait();
        }

        // generated by https://app.quicktype.io/#l=cs&r=json2csharp
        public partial class ResponseJson
        {
            [JsonProperty("Language")]
            public long Language { get; set; }

            [JsonProperty("ProjectType")]
            public long ProjectType { get; set; }

            [JsonProperty("Compiler")]
            public long Compiler { get; set; }

            [JsonProperty("IsAutoRun")]
            public bool IsAutoRun { get; set; }

            [JsonProperty("IsReadonly")]
            public bool IsReadonly { get; set; }

            [JsonProperty("CodeBlock")]
            public string CodeBlock { get; set; }

            [JsonProperty("OriginalCodeBlock")]
            public string OriginalCodeBlock { get; set; }

            [JsonProperty("ConsoleOutput")]
            public string ConsoleOutput { get; set; }

            [JsonProperty("OriginalFiddleId")]
            public string OriginalFiddleId { get; set; }

            [JsonProperty("NuGetPackageVersionIds")]
            public object NuGetPackageVersionIds { get; set; }

            [JsonProperty("HeaderDirectivePackageVersionIds")]
            public object HeaderDirectivePackageVersionIds { get; set; }

            [JsonProperty("OriginalNuGetPackageVersionIds")]
            public object OriginalNuGetPackageVersionIds { get; set; }

            [JsonProperty("Stats")]
            public Stats Stats { get; set; }

            [JsonProperty("TimeOffset")]
            public long TimeOffset { get; set; }

            [JsonProperty("NuGetPackageVersions")]
            public object[] NuGetPackageVersions { get; set; }

            [JsonProperty("HeaderDirectivePackageVersions")]
            public object[] HeaderDirectivePackageVersions { get; set; }

            [JsonProperty("IsConsoleInputRequested")]
            public bool IsConsoleInputRequested { get; set; }

            [JsonProperty("ConsoleInputLines")]
            public object ConsoleInputLines { get; set; }

            [JsonProperty("MvcViewEngine")]
            public long MvcViewEngine { get; set; }

            [JsonProperty("MvcCodeBlock")]
            public MvcCodeBlock MvcCodeBlock { get; set; }

            [JsonProperty("OriginalMvcCodeBlock")]
            public MvcCodeBlock OriginalMvcCodeBlock { get; set; }

            [JsonProperty("WebPageHtmlOutput")]
            public object WebPageHtmlOutput { get; set; }

            [JsonProperty("WebPageHtmlOutputId")]
            public object WebPageHtmlOutputId { get; set; }

            [JsonProperty("UserId")]
            public object UserId { get; set; }

            [JsonProperty("UserDisplayName")]
            public object UserDisplayName { get; set; }

            [JsonProperty("Name")]
            public object Name { get; set; }

            [JsonProperty("AccessType")]
            public long AccessType { get; set; }

            [JsonProperty("ForkCount")]
            public long ForkCount { get; set; }

            [JsonProperty("IsInUserFavorites")]
            public bool IsInUserFavorites { get; set; }

            [JsonProperty("IsInUserFiddles")]
            public bool IsInUserFiddles { get; set; }

            [JsonProperty("ViewCount")]
            public long ViewCount { get; set; }

            [JsonProperty("FavoriteCount")]
            public long FavoriteCount { get; set; }

            [JsonProperty("HasErrors")]
            public bool HasErrors { get; set; }

            [JsonProperty("HasCompilationErrors")]
            public bool HasCompilationErrors { get; set; }

            [JsonProperty("IsConvertedFiddle")]
            public bool IsConvertedFiddle { get; set; }

            [JsonProperty("UseResultCache")]
            public bool UseResultCache { get; set; }
        }

        public partial class MvcCodeBlock
        {
            [JsonProperty("Model")]
            public object Model { get; set; }

            [JsonProperty("View")]
            public object View { get; set; }

            [JsonProperty("Controller")]
            public object Controller { get; set; }
        }

        public partial class Stats
        {
            [JsonProperty("RunAt")]
            public string RunAt { get; set; }

            [JsonProperty("CompileTime")]
            public string CompileTime { get; set; }

            [JsonProperty("ExecuteTime")]
            public string ExecuteTime { get; set; }

            [JsonProperty("MemoryUsage")]
            public string MemoryUsage { get; set; }

            [JsonProperty("CpuUsage")]
            public string CpuUsage { get; set; }

            [JsonProperty("IsResultCache")]
            public bool IsResultCache { get; set; }
        }
    }
}
