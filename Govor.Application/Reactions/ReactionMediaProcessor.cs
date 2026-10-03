using Govor.Domain.Common;
using Govor.Domain.Models.Reactions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SmartRes;

namespace Govor.Application.Reactions;

public static class ReactionMediaLimits
{
    public const int MaxDimension = 128;
    public const int MaxUploadBytes = 2 * 1024 * 1024;
    public const int MaxImageBytes = 256 * 1024;
    public const int MaxGifBytes = 512 * 1024;
    public const int MaxFrames = 60;
    public const int MaxDurationMilliseconds = 5000;
}

public record ProcessedReactionMedia(byte[] Data, ReactionKind Kind, string MimeType,
    string Extension, int Width, int Height, int DurationMilliseconds);

public interface IReactionMediaProcessor
{
    Task<Result<ProcessedReactionMedia, Error>> ProcessAsync(byte[] data, CancellationToken cancellationToken = default);
}

public class ReactionMediaProcessor : IReactionMediaProcessor
{
    public async Task<Result<ProcessedReactionMedia, Error>> ProcessAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (data.Length == 0 || data.Length > ReactionMediaLimits.MaxUploadBytes)
            return Invalid("Upload must contain at most 2 MiB.");

        try
        {
            var configuration = Configuration.Default.Clone();
            configuration.MaxDegreeOfParallelism = 2;
            configuration.MemoryAllocator = MemoryAllocator.Create(new MemoryAllocatorOptions
            { MaximumPoolSizeMegabytes = 16, AllocationLimitMegabytes = 64 });
            var options = new DecoderOptions
            {
                Configuration = configuration, SkipMetadata = true,
                MaxFrames = ReactionMediaLimits.MaxFrames + 1,
                TargetSize = new Size(ReactionMediaLimits.MaxDimension, ReactionMediaLimits.MaxDimension)
            };
            using var input = new MemoryStream(data, writable: false);
            var format = await Image.DetectFormatAsync(input, cancellationToken);
            var isGif = format.Name == "GIF";
            if (!isGif && format.Name is not ("PNG" or "JPEG" or "WEBP"))
                return Invalid("Only PNG, JPEG, WebP and GIF are supported.");
            input.Position = 0;
            var info = await Image.IdentifyAsync(options, input, cancellationToken);
            if (info.Width > 2048 || info.Height > 2048 || info.FrameMetadataCollection.Count > ReactionMediaLimits.MaxFrames)
                return Invalid("Source dimensions must not exceed 2048 px and GIF must not exceed 60 frames.");
            input.Position = 0;
            using var image = await Image.LoadAsync<Rgba32>(options, input, cancellationToken);
            if (image.Frames.Count > ReactionMediaLimits.MaxFrames || (!isGif && image.Frames.Count > 1))
                return Invalid("Only GIF animations with at most 60 frames are supported.");

            var duration = 0;
            if (isGif)
            {
                foreach (var frame in image.Frames)
                {
                    var metadata = frame.Metadata.GetGifMetadata();
                    metadata.FrameDelay = Math.Max(2, metadata.FrameDelay);
                    duration += metadata.FrameDelay * 10;
                }
                if (duration > ReactionMediaLimits.MaxDurationMilliseconds)
                    return Invalid("GIF duration must not exceed 5 seconds per loop.");
            }

            // Re-encode pixels, never store the original upload or trust its MIME/extension.
            foreach (var dimension in new[] { 128, 96, 64, 48 })
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                { Size = new Size(dimension, dimension), Mode = ResizeMode.Max }));
                using var output = new MemoryStream();
                if (isGif)
                    await image.SaveAsGifAsync(output, new GifEncoder { SkipMetadata = true }, cancellationToken);
                else
                    await image.SaveAsPngAsync(output, cancellationToken);
                if (output.Length <= (isGif ? ReactionMediaLimits.MaxGifBytes : ReactionMediaLimits.MaxImageBytes))
                    return new ProcessedReactionMedia(output.ToArray(), isGif ? ReactionKind.Gif : ReactionKind.Image,
                        isGif ? "image/gif" : "image/png", isGif ? ".gif" : ".png", image.Width, image.Height, duration);
            }
            return Invalid("The processed file exceeds the reaction size limit.");
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or InvalidMemoryOperationException or ArgumentException)
        {
            return Invalid("Invalid or excessively complex image.");
        }
    }

    private static Result<ProcessedReactionMedia, Error> Invalid(string message) =>
        Result<ProcessedReactionMedia, Error>.Failure(Error.Validation("Reaction.Media.Invalid", message));
}
