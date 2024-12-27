using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Collections.Generic;


public class RedisController : Controller
{
    private static ConnectionMultiplexer muxer;
    private static ISubscriber subscriber;
    private static List<string> channels = new List<string>();
    private static Dictionary<string, List<string>> messages = new Dictionary<string, List<string>>();



    //Sehife yuklenmesi
    public ActionResult Index(string selectedChannel = null)
    {
        muxer = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { { "redis-18714.c16.us-east-1-3.ec2.redns.redis-cloud.com", 18714 } },
            User = "default",
            Password = "IgMjUjXVjS3u7yGevmQ0P3ju6rtW3ryk",
        });
        ViewBag.Channels = channels;
        ViewBag.SelectedChannel = selectedChannel;
        ViewBag.Messages = selectedChannel != null && messages.ContainsKey(selectedChannel)
            ? messages[selectedChannel]
            : new List<string>();
        return View();
    }

    //Channel Elave edilmesi
    [HttpPost]
    public ActionResult AddChannel(string channelName)
    {
        if (!string.IsNullOrWhiteSpace(channelName) && !channels.Contains(channelName))
        {
            channels.Add(channelName);
            messages[channelName] = new List<string>();

            subscriber = muxer.GetSubscriber();

            subscriber.Subscribe(channelName, (channel, message) =>
            {
                messages[channel].Add(message.ToString());
            });
        }
        return RedirectToAction("Index");
    }



    //Secilmis channele message yazmaq
    [HttpPost]
    public ActionResult SendMessage(string selectedChannel, string message)
    {
        if (selectedChannel!=null)
        {
            subscriber.Publish(selectedChannel, message);
        }
        return RedirectToAction("Index", new { selectedChannel });
    }
}
