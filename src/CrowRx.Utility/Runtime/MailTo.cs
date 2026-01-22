using UnityEngine;
using UnityEngine.Networking;

//reference : http://minhyeokism.tistory.com/46 [programmer-dominic.kim]

namespace CrowRx.Utility
{
    public static class MailTo
    {
        public static void Send(string mailto, string subject, string body) => Application.OpenURL($"mailto:{mailto}?subject={EscapeURL(subject)}&body={EscapeURL(body)}");
        private static string EscapeURL(string url) => UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }
}