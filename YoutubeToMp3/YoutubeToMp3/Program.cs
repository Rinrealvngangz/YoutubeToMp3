using System.Reflection;
using VideoLibrary;
using YoutubeExplode;
using YoutubeExplode.Converter;

var linksYoutube = new List<string>()
{
    "https://www.youtube.com/watch?v=Ohq6cTsok08"
};

Console.WriteLine("Start convert...");
YoutubeToAudio videoToAudio = new YoutubeToAudio();
await videoToAudio.ConvertVideoYoutubeToAudioByLink(linksYoutube);
class YoutubeToAudio
{
    private string Destination = @"your destination";
    static string? directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
    readonly string Pathffmpeg  = $@"{directoryName}/ffmpeg";
    private void AddExtensionFile(string extension, ref string name)
    {
        name += "." + extension;
    }
    private async Task ConvertVideoYoutubeToAudioByLink(string link,CancellationTokenSource cts)
    {
        try
        {
            var youtubeClient = new YoutubeClient();
            var videoInfo = await GetVideoInfo(link);
            string pathDestinationVideo = GetDestinationPathName(videoInfo,"mp3"); 
            await youtubeClient.Videos
                .DownloadAsync(link, pathDestinationVideo, c => c
                    .SetPreset(ConversionPreset.UltraFast)
                    .SetFFmpegPath(Pathffmpeg)
                );
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            cts.Cancel();
            throw;
        }
    }
    public async Task ConvertVideoYoutubeToAudioByLink(List<string> links)
    {
        var cts = new CancellationTokenSource();
        var options = new ParallelOptions() { CancellationToken = cts.Token };
        if (!ExistDestination())
        {
            Console.WriteLine("Not found destination");
            return;
        }
        try
        {
            await Parallel.ForEachAsync(links, options, async (url, c) =>
            {
                await ConvertVideoYoutubeToAudioByLink(url,cts);
            });
            Console.WriteLine("Successfully");
            Console.WriteLine(Destination);
        }
        catch
        {
            cts.Cancel();
        }
        finally
        {
            cts.Dispose();
        }
    }
    private async Task<YouTubeVideo> GetVideoInfo(string link)
    {
        try
        {
            var youtube = new YouTube();
            var videoInfo = await youtube.GetVideoAsync(link);
            return videoInfo;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
   
    private bool ExistDestination()
    {
        if (Directory.Exists(Destination))
        {
            return true;
        }
        return false;
    }
    private string GetDestinationPathName(YouTubeVideo videoInfo,string extension)
    {
        var nameVideo = videoInfo.FullName;
        var posDot = nameVideo.IndexOf('.');
        var formatName = nameVideo.Substring(0, posDot);
        var destinationPath = Path.Combine(Destination, formatName);
        AddExtensionFile("mp3",ref destinationPath);
        RenameVideoIfExist(ref destinationPath,formatName);
        return destinationPath;
    }

    private void RenameVideoIfExist(ref string path , string nameVideo)
    {
        int i = 0;
        while (true)
        {
            if (!File.Exists(path))
            {
                return;
            }
            i++;
            nameVideo += nameVideo + i;
            var destinationPath = Path.Combine(Destination, nameVideo);
            AddExtensionFile("mp3",ref destinationPath);
            path = destinationPath;
        }
    }
}