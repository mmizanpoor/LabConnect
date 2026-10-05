using System.Net;
using Microsoft.VisualBasic;

namespace LabConnectPortal.Api.Infrastructure.Auth.Sms;

public class SmsSender(ILogger<SmsSender> logger) : ISmsSender
{
    public Task SendAsync(string mobileNumber, string message)
    {
        logger.LogInformation("ارسال پیامک به {Mobile}: {Message}", mobileNumber, message);
        return Task.CompletedTask;
    }


    public bool SendSMS(string message, string mobile, int? labCode)
    {
        string tag = "LabConnect";
        string url = "http://www.ptnmed.com/main/webservice.aspx?ID=send&tag=" + tag + "&labcode=" + (labCode.HasValue ? labCode : 1111) + "&msg=" + ConvertToAsc(message) + "&to=" + mobile;


        if (Strings.InStr(Strings.LCase(SendPostUrl(url)), "send ok") > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    private string SendPostUrl(string url)
    {
        try
        {
            var req = (HttpWebRequest)System.Net.WebRequest.Create(url);
            req.Method = "GET";
            var res = (HttpWebResponse)req.GetResponse();
            req.Timeout = 5000;

            Stream stream = res.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.ASCII);
            string info = streamReader.ReadToEnd();

            return info;
        }
        catch (Exception)
        {

            return "Error Of Server";
        }
    }

    private string ConvertToAsc(string text)
    {
        string lastStr = "";

        for (int i = 1; i < text.Length + 1; i++)
        {
            lastStr = lastStr + Strings.AscW(Strings.Mid(text, i, 1)).ToString() + "|";
        }

        return lastStr;
    }
}
