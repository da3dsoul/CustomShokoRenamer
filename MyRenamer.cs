using System;
using System.IO;
using System.Linq;
using System.Text;
using Shoko.Plugin.Abstractions;
using Shoko.Plugin.Abstractions.DataModels;

namespace Renamer.Cazzar
{
    public class MyRenamer : IRenamer
    {
        public void GetFilename(RenameEventArgs args)
        {
            var video = args.FileInfo;
            var episode = args.EpisodeInfo.First();
            var anime = args.AnimeInfo.First();

            StringBuilder name = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(video.AniDBFileInfo.ReleaseGroup.ShortName))
                name.Append($"[{video.AniDBFileInfo.ReleaseGroup.ShortName}]");

            name.Append($" {anime.PreferredTitle}");
            if (anime.Type != AnimeType.Movie)
            {
                string paddedEpisodeNumber = null;
                switch (episode.Type)
                {
                    case EpisodeType.Episode:
                        paddedEpisodeNumber = episode.Number.PadZeroes(anime.EpisodeCounts.Episodes);
                        break;
                    case EpisodeType.Credits:
                        paddedEpisodeNumber = "C" + episode.Number.PadZeroes(anime.EpisodeCounts.Credits);
                        break;
                    case EpisodeType.Special:
                        paddedEpisodeNumber = "S" + episode.Number.PadZeroes(anime.EpisodeCounts.Specials);
                        break;
                    case EpisodeType.Trailer:
                        paddedEpisodeNumber = "T" + episode.Number.PadZeroes(anime.EpisodeCounts.Trailers);
                        break;
                    case EpisodeType.Parody:
                        paddedEpisodeNumber = "P" + episode.Number.PadZeroes(anime.EpisodeCounts.Parodies);
                        break;
                    case EpisodeType.Other:
                        paddedEpisodeNumber = "O" + episode.Number.PadZeroes(anime.EpisodeCounts.Others);
                        break;
                }

                name.Append($" - {paddedEpisodeNumber}");
            }
            // resolution
            var media = video.MediaInfo?.Video;
            if (media != null)
                name.Append($" ({media.Width}x{media.Height}");
            else
                name.Append(" 0x0");

            if (video.AniDBFileInfo?.Source != null &&
                (video.AniDBFileInfo.Source.Equals("DVD", StringComparison.InvariantCultureIgnoreCase) ||
                 video.AniDBFileInfo.Source.Equals("Blu-ray", StringComparison.InvariantCultureIgnoreCase)))
                name.Append($" {video.AniDBFileInfo.Source}");

            // TODO simplified codecs
            name.Append($" {(string.IsNullOrEmpty(media.SimplifiedCodec) ? media.CodecID : media.SimplifiedCodec)}"
                .TrimEnd());

            if (media?.BitDepth == 10)
                name.Append(" 10bit");
            name.Append(')');

            if (video.AniDBFileInfo != null && video.AniDBFileInfo.Censored) name.Append(" [CEN]");

            name.Append($" [{video.Hashes.CRC.ToUpper()}]");
            name.Append($"{Path.GetExtension(video.Filename)}");

            args.Result = name.ToString().ReplaceInvalidPathCharacters();
        }

        public void GetDestination(MoveEventArgs args)
        {
            var anime = args.AnimeInfo.First();
            bool isPorn = anime.Restricted;
            var location = "/anime/";
            if (anime.Type == AnimeType.Movie) location = "/movies/";
            if (isPorn) location = "/porn/";


            var dest = args.AvailableFolders.FirstOrDefault(a => a.Location == location);

            args.DestinationImportFolder = dest;
            args.DestinationPath = anime.PreferredTitle;
        }

        public void Load()
        {
        }

        public void OnSettingsLoaded(IPluginSettings settings)
        {
        }

        public string Name => "CazzarRenamer";
    }
}
                                                                                                      