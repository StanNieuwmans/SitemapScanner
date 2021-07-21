using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace SitemapScanner
{
    public class Program
    {
        static void Main(string[] args)
        {
            var url = GetUrl();
            List<string> listUrl = GetSiteMapUrls(url);
            Console.WriteLine("------------------Start Processing Urls---------------------");
            var errorDictionary = ProcessSiteMapUrls(listUrl);
            if (errorDictionary.Count != 0)
            {
                Console.WriteLine("------------------------------ERRORS--------------------------");
                foreach (var error in errorDictionary)
                {
                    Console.WriteLine(string.Format(" Status: {0} URL: {1}", error.Value, error.Key));
                }
            }
            else
            {
                Console.WriteLine("No errors found!");
            }
            Console.ReadLine();
        }

        public static string GetUrl()
        {
            Console.WriteLine("Enter sitemap url:");
            string url = Console.ReadLine();
            Console.WriteLine("The selected url is: " + url);
            return url;
        }
        public static List<string> GetSiteMapUrls(string url)
        {
            Console.WriteLine("Getting all the urls from : " + url);
            List<string> urls = new List<string>();
            string baseurl = url;

            using (WebClient wc = new WebClient())
            {

                wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.164 Safari/537.36");
                wc.Encoding = System.Text.Encoding.UTF8;

                string reply = wc.DownloadString(baseurl);

                XmlDocument urldoc = new XmlDocument();

                urldoc.LoadXml(reply);

                XmlNodeList xnList = urldoc.GetElementsByTagName("url");
                XmlNodeList sitemapList = urldoc.GetElementsByTagName("sitemap");

                foreach (XmlNode node in xnList)
                {
                    urls.Add(node["loc"].InnerText);
                }
                foreach (XmlNode sitemap in sitemapList)
                {
                    var sitemapUrlList = GetSiteMapUrls(sitemap["loc"].InnerText);
                    foreach (var siteurl in sitemapUrlList)
                    {
                        urls.Add(siteurl);
                    }
                }
            }
            return urls;
        }
        public static Dictionary<string, string> ProcessSiteMapUrls(List<string> listUrls)
        {
            Dictionary<string, string> statusErrors = new Dictionary<string, string>();
            Parallel.ForEach(listUrls, url =>
            {
                Console.WriteLine(string.Format("Date/Time: {0}; Crawling: {1}", DateTime.Now.ToString(), url));
                try
                {
                    HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                }
                catch (Exception e)
                {
                    if (e is WebException ex)
                    {
                        HttpWebResponse response = (HttpWebResponse)ex.Response;

                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                                statusErrors.Add(url, response.StatusDescription);
                                break;
                            case HttpStatusCode.BadRequest:
                                statusErrors.Add(url, response.StatusDescription);
                                break;
                            case HttpStatusCode.InternalServerError:
                                statusErrors.Add(url, response.StatusDescription);
                                break;
                            case HttpStatusCode.ServiceUnavailable:
                                statusErrors.Add(url, response.StatusDescription);
                                break;
                        }
                    }
                    else if (e is System.UriFormatException)
                    {
                        statusErrors.Add(url, "The URI specified is not a valid URI.");
                    }
                    else
                    {
                        statusErrors.Add(url, e.ToString());
                    }
                }
            });
            return statusErrors;
        }
    }

}
