using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

namespace Sitemaperrorfinder
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
                foreach (var tikk in errorDictionary)
                {
                    Console.WriteLine("------------------------------ERRORS--------------------------");
                    Console.WriteLine(string.Format(" Status: {1}; URL: {0};", tikk.Key, tikk.Value));
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
                foreach (XmlNode woh in sitemapList)
                {
                    var fire = GetSiteMapUrls(woh["loc"].InnerText);
                    foreach (var tata in fire)
                    {
                        urls.Add(tata);
                    }
                }
            }
            return urls;
        }
        public static Dictionary<string, string> ProcessSiteMapUrls(List<string> listUrls)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            foreach (var url in listUrls)
            {
                Console.WriteLine(string.Format("Date/Time: {0}; Crawling: {1};", DateTime.Now.ToString(), url));
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                try
                {
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                }
                catch (Exception e)
                {
                    if (e is WebException ex)
                    {
                        HttpWebResponse rataat = (HttpWebResponse)ex.Response;

                        switch (rataat.StatusCode)
                        {
                            case HttpStatusCode.NotFound:
                                errors.Add(url, rataat.StatusDescription);
                                break;
                            case HttpStatusCode.BadRequest:
                                errors.Add(url, rataat.StatusDescription);
                                break;
                            case HttpStatusCode.InternalServerError:
                                errors.Add(url, rataat.StatusDescription);
                                break;
                            case HttpStatusCode.ServiceUnavailable:
                                errors.Add(url, rataat.StatusDescription);
                                break;
                        }
                    }
                }
            }
            return errors;
        }
    }

}
