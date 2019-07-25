using GitApp1.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GitApp1.Models
{
    public class Repository
    {
        public string name { get; set; }
        public string full_name { get; set; }
        public string license { get; set; }
        public string branch { get; set; }
        public bool valid { get; set; }
    }

    public class PullRequest
    {
        public string title { get; set; }
        public string body { get; set; }
    }

    public class GitHub
    {
        public Dictionary<string, string> pages = null;
        public List<Repository> Repositories = null;
        private string token = null; // Example "token 1234567890c17479f5416d487bd011be2b557daf";

        public GitHub()
        {
            Repositories = new List<Repository>();
            Global.timeout = DateTime.UtcNow.AddHours(1);
            Global.remaining = 60;
            pages = new Dictionary<string, string>();
        }

        private DateTime UnixTime2DateTime(double unixTime)
        {
            if (unixTime < 0)
                return DateTime.UtcNow;

            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
        }

        private dynamic Http (string url, string payload=null, string token = null)
        {
            if (Global.remaining == 0)
            {
                return (DateTime.Now - Global.timeout).TotalMinutes;
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "BYU_Test";

                request.Method = "GET";

                if (payload != null)
                {
                    request.ContentType = "application/json";
                    byte[] bytes = Encoding.ASCII.GetBytes(payload);
                    request.Method = "POST";

                    if (token != null)
                        request.Headers.Add("Authorization:" + token);

                    request.ContentLength = bytes.Length;
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(bytes, 0, bytes.Length);
                    dataStream.Close();
                }

                HttpWebResponse response = null;
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    try
                    {
                        Global.timeout = UnixTime2DateTime(Int32.Parse(response.Headers["x-ratelimit-reset"]));
                        Global.remaining = int.Parse(response.Headers["x-ratelimit-remaining"]);
                        pages = response.Headers["Link"].Split(',')
                            .Select(value => value.Split(';'))
                            .ToDictionary(
                            pair => pair[1].Replace("rel=", "").Replace ("\"", "").Replace(" ",""), 
                            pair => pair[0].Replace("<","").Replace(">",""));

                    }
                    catch
                    {
                        //timeout = 60 seconds
                        pages.Clear();
                    }

                    var encoding = ASCIIEncoding.ASCII;
                    using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                    {
                        string json = reader.ReadToEnd();
                        dynamic github = JsonConvert.DeserializeObject(json);
                        return github;
                    }
                }
            }
            catch (WebException x)
            {
                using (var stream = x.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string error = reader.ReadToEnd();
                    if (error.Contains("API rate limit exceeded"))
                    {
                        string timeout = x.Response.Headers["x-ratelimit-reset"];
                        return UnixTime2DateTime(double.Parse(timeout));
                    }
                }
            }
            return null;
        }

        public List<Repository> GetRepositories(string organization)
        {
            Repositories.Clear();
            if (pages.Count == 0)
                pages.Add("next", "https://api.github.com/orgs/" + organization + "/repos");
            do
            {
                dynamic repositories = Http(pages["next"]);
                if (Errors(repositories))
                    return Repositories;

                foreach (var repository in repositories)
                {
                    string name = repository["name"].ToString();
                    string full_name = repository["full_name"].ToString();
                    
                    try
                    {
                        string license = repository["license"]["name"].ToString();
                        Repositories.Add(new Repository() { name = name, license = license, valid = true });
                    }
                    catch
                    {
                        Repositories.Add(new Repository() { name = name, full_name = token==null?null:full_name,  valid = false });
                    }
                }
            } while (pages.ContainsKey("next"));

            return Repositories;
        }

        public bool Open_A_Pull_Request(string repo)
        {
            PullRequest pr = new PullRequest() { title = "License Issue", body = "Can you please install a GitHub license for this repository" };
            var status = Http("https://api.github.com/repos/" + repo + "/issues", JsonConvert.SerializeObject(pr));
            if (Errors(status))
                return false;
            return true;
        }
        
        private bool Errors(dynamic obj)
        {
            if (obj == null)
                return true;

            string obj_type = string.Format("{0}", obj.GetType());
            if (obj_type.Equals("System.DateTime"))
            {
                Global.timeout = obj;
                Global.remaining = 0;
                return true;
            }

            return false;
        }
    }
}
