using System;

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using Serilog;
using Serilog.Formatting.Json;

namespace Abot2.Demo
{
    public class Program
    {
        public static List<string> Urls = new List<string>();
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: Constants.LogFormatTemplate)
                .CreateLogger();

            Log.Information("Demo starting up!");

            //await DemoPageRequester();
            await DemoSimpleCrawler();

            Log.Information("Demo done!");
            Console.Clear();

            XmlTextWriter writer = new XmlTextWriter("Urls.xml", System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("Urls");
            foreach (var i in Urls)
            {
                if (!i.Contains("/-/media"))
                {
                    createNode(i, writer);
                }

            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            Console.ReadKey();
        }

        private static async Task DemoSimpleCrawler()
        {
            CrawlConfiguration crawlConfig = new CrawlConfiguration();
            crawlConfig.CrawlTimeoutSeconds = 0;
            crawlConfig.MaxConcurrentThreads = 100;
            crawlConfig.MaxPagesToCrawl = 100000;
            crawlConfig.MaxPagesToCrawlPerDomain = 10000;
            crawlConfig.MaxCrawlDepth = 1000;
            //crawlConfig.UserAgentString = agent;
            crawlConfig.MaxPageSizeInBytes = 0;
            crawlConfig.DownloadableContentTypes = "text/html, text/plain";
            crawlConfig.IsUriRecrawlingEnabled = false;
            crawlConfig.IsExternalPageCrawlingEnabled = false;
            crawlConfig.IsExternalPageLinksCrawlingEnabled = false;
            crawlConfig.HttpServicePointConnectionLimit = 50;
            crawlConfig.HttpRequestTimeoutInSeconds = 15;
            crawlConfig.HttpRequestMaxAutoRedirects = 7;
            crawlConfig.IsHttpRequestAutoRedirectsEnabled = true;
            crawlConfig.IsHttpRequestAutomaticDecompressionEnabled = false;
            crawlConfig.MinAvailableMemoryRequiredInMb = 0;
            crawlConfig.MaxMemoryUsageInMb = 256;
            crawlConfig.MaxMemoryUsageCacheTimeInSeconds = 0;
            crawlConfig.MaxRetryCount = 2;
            crawlConfig.IsForcedLinkParsingEnabled = true;
            crawlConfig.IsAlwaysLogin = true;
            crawlConfig.LoginUser = "mylanextpreview";
            crawlConfig.LoginPassword = "7isRfgb3s23ttW!";
            crawlConfig.MinCrawlDelayPerDomainMilliSeconds = 3000;

            var crawler = new PoliteWebCrawler(crawlConfig);


            crawler.PageCrawlCompleted += Crawler_PageCrawlCompleted;

            var crawlResult = await crawler.CrawlAsync(new Uri("https://www.viatris.com/en"));
        }

        private static void Crawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            foreach (HyperLink a in new AngleSharpHyperlinkParser().GetLinks(e.CrawledPage))
            {
                if (e.CrawlContext.RootUri.Authority == a.HrefValue.Authority)
                {
                    if (!Urls.Exists(x => x == a.HrefValue.AbsoluteUri))
                    {
                        Urls.Add(a.HrefValue.AbsoluteUri);
                    }
                }


            }



            // IEnumerable internalLinks = allLinksOnPage.Where(l => l.Authority == e.CrawlContext.RootUri.Authority);
            // IEnumerable externalLinks = allLinksOnPage.Except(internalLinks);

        }

        private static async Task DemoPageRequester()
        {
            var pageRequester =
                new PageRequester(new CrawlConfiguration(), new WebContentExtractor());

            //var result = await pageRequester.MakeRequestAsync(new Uri("http://google.com"));
            var result = await pageRequester.MakeRequestAsync(new Uri("https://viatriscom.93preview.mylansitecore.com/en"));
            Log.Information("{result}", new { url = result.Uri, status = Convert.ToInt32(result.HttpResponseMessage.StatusCode) });

        }

        public static void createNode(string url, XmlTextWriter writer)
        {
            writer.WriteStartElement("Url");
            writer.WriteString(url);
            writer.WriteEndElement();
        }
    }
}
