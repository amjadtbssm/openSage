﻿using System;
using System.Collections.Generic;
using System.IO;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Dds;
using OpenSage.Data.Tga;

namespace OpenSage.Content
{
    internal sealed class TextureLoader : ContentLoader<Texture>
    {
        public override object PlaceholderValue { get; }

        public TextureLoader(GraphicsDevice graphicsDevice)
        {
            PlaceholderValue = AddDisposable(Texture.CreatePlaceholderTexture2D(graphicsDevice));
        }

        public override IEnumerable<string> GetPossibleFilePaths(string filePath)
        {
            yield return Path.ChangeExtension(filePath, ".dds");
            yield return Path.ChangeExtension(filePath, ".tga");
        }

        protected override Texture LoadEntry(FileSystemEntry entry, ContentManager contentManager)
        {
            switch (Path.GetExtension(entry.FilePath).ToLower())
            {
                case ".dds":
                    if (entry.FilePath == @"art\textures\palantira.dds")
                    {
                        goto case ".tga";
                    }
                    var ddsFile = DdsFile.FromFileSystemEntry(entry);
                    return CreateTextureFromDds(
                        contentManager.GraphicsDevice,
                        ddsFile);

                case ".tga":
                    var tgaFile = TgaFile.FromFileSystemEntry(entry);
                    return CreateTextureFromTga(
                        contentManager.GraphicsDevice,
                        tgaFile,
                        true); // TODO: Don't need to generate mipmaps for GUI textures.

                default:
                    throw new InvalidOperationException();
            }
        }

        private static Texture CreateTextureFromDds(
            GraphicsDevice graphicsDevice,
            DdsFile ddsFile)
        {
            var mipMapData = new TextureMipMapData[ddsFile.Header.MipMapCount];

            for (var i = 0; i < ddsFile.Header.MipMapCount; i++)
            {
                mipMapData[i] = new TextureMipMapData
                {
                    Data = ddsFile.MipMaps[i].Data,
                    BytesPerRow = (int) ddsFile.MipMaps[i].RowPitch
                };
            }

            return Texture.CreateTexture2D(
                graphicsDevice,
                ToPixelFormat(ddsFile.ImageFormat),
                (int) ddsFile.Header.Width,
                (int) ddsFile.Header.Height,
                mipMapData);
        }

        private static PixelFormat ToPixelFormat(DdsImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case DdsImageFormat.Bc1:
                    return PixelFormat.Bc1;

                case DdsImageFormat.Bc2:
                    return PixelFormat.Bc2;

                case DdsImageFormat.Bc3:
                    return PixelFormat.Bc3;

                default:
                    throw new ArgumentOutOfRangeException(nameof(imageFormat));
            }
        }

        public static TextureMipMapData[] GetData(TgaFile tgaFile, bool generateMipMaps)
        {
            if (tgaFile.Header.ImageType != TgaImageType.UncompressedRgb)
            {
                throw new InvalidOperationException();
            }

            var data = ConvertTgaPixels(
                tgaFile.Header.ImagePixelSize,
                tgaFile.Data);

            if (generateMipMaps)
            {
                return MipMapUtility.GenerateMipMaps(
                    tgaFile.Header.Width,
                    tgaFile.Header.Height,
                    data);
            }
            else
            {
                return new[]
                {
                    new TextureMipMapData
                    {
                        Data = data,
                        BytesPerRow = tgaFile.Header.Width * 4
                    }
                };
            }
        }

        private static Texture CreateTextureFromTga(
            GraphicsDevice graphicsDevice,
            TgaFile tgaFile,
            bool generateMipMaps)
        {
            var mipMapData = GetData(tgaFile, generateMipMaps);

            return Texture.CreateTexture2D(
                graphicsDevice,
                PixelFormat.Rgba8UNorm,
                tgaFile.Header.Width,
                tgaFile.Header.Height,
                mipMapData);
        }

        private static byte[] ConvertTgaPixels(byte pixelSize, byte[] data)
        {
            switch (pixelSize)
            {
                case 24: // BGR
                    {
                        var result = new byte[data.Length / 3 * 4];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i += 3)
                        {
                            result[resultIndex++] = data[i + 2]; // R
                            result[resultIndex++] = data[i + 1]; // G
                            result[resultIndex++] = data[i + 0]; // B
                            result[resultIndex++] = 255;         // A
                        }
                        return result;
                    }

                case 32: // BGRA
                    {
                        var result = new byte[data.Length];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i += 4)
                        {
                            result[resultIndex++] = data[i + 2]; // R
                            result[resultIndex++] = data[i + 1]; // G
                            result[resultIndex++] = data[i + 0]; // B
                            result[resultIndex++] = data[i + 3]; // A
                        }
                        return result;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelSize));
            }
        }
    }

    public sealed class TextureLoadOptions : LoadOptions
    {
        public bool GenerateMipMaps { get; set; }
    }
}
