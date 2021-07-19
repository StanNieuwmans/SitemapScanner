using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
            Dictionary<string, string> statusErrors = new Dictionary<string, string>();
            Dictionary<string, string> errors = new Dictionary<string, string>();
            foreach (var url in listUrls)
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
                        errors.Add(url, e.ToString());
                    }
                }
            }
            if (statusErrors.Count == 0)
            {
                //Not realy important errors.
                return errors;
            }
            return statusErrors;
        }
    }

}
