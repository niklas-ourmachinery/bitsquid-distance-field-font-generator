using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace distance_field
{
    class Tga
    {
        const int NO_COLOR_MAP = 0;
        
        /// <summary>
        /// Uncompressed color tga format.
        /// </summary>
        public const int UNCOMPRESSED_TRUECOLOR_IMAGE = 2;

        /// <summary>
        /// Uncompressed monochrome tga format.
        /// </summary>
        public const int UNCOMPRESSED_BW_IMAGE = 3;

        static byte ReadByte(byte[] bytes, ref int p)
        {
            p++;
            return bytes[p - 1];
        }

        static void WriteByte(byte[] bytes, ref int p, byte value)
        {
            bytes[p] = value;
            p++;
        }

        static void SkipBytes(byte[] bytes, ref int p, int count)
        {
            p += count;
        }

        static void WriteZero(byte[] bytes, ref int p, int count)
        {
            for (int i = 0; i < count; ++i)
                bytes[p + i] = 0;
            p += count;
        }

        static int ReadUInt16(byte[] bytes, ref int p)
        {
            p += 2;
            return bytes[p-2] + (bytes[p-1] << 8);
        }

        static void WriteUInt16(byte[] bytes, ref int p, UInt16 value)
        {
            WriteByte(bytes, ref p, (byte)(value & 0xff));
            WriteByte(bytes, ref p, (byte)(value >> 8));
        }

        /// <summary>
        /// Reads a .tga image. Currently very limited. Only reads the UNCOMPRESSED_BW_IMAGE format.
        /// </summary>
        public static Image Load(byte[] bytes)
        {
            // Read header
            int p = 0;
            byte id_length = ReadByte(bytes, ref p);
            byte color_map = ReadByte(bytes, ref p);
            Debug.Assert(color_map == NO_COLOR_MAP);
            byte image_type = ReadByte(bytes, ref p);
            Debug.Assert(image_type == UNCOMPRESSED_BW_IMAGE);
            SkipBytes(bytes, ref p, 5); // Color map
            SkipBytes(bytes, ref p, 4); // X/Y origin
            int width = ReadUInt16(bytes, ref p);
            int height = ReadUInt16(bytes, ref p);
            byte pixel_depth = ReadByte(bytes, ref p);
            Debug.Assert(pixel_depth == 8);
            byte descriptor = ReadByte(bytes, ref p);
            SkipBytes(bytes, ref p, id_length);

            // Read image data
            Image image = new Image();
            if (image_type == UNCOMPRESSED_BW_IMAGE)
            {
                Channel c = new Channel(width, height);
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        c.Data[y * width + x] = (float)bytes[p] / 255.0f;
                        p++;
                    }
                }
                image.Channels["A"] = c;
            }
            return image;
        }

        /// <summary>
        /// Saves a tga image. Currently very limited. Only saves the UNCOMPRESSED_TRUECOLOR_IMAGE
        /// format.
        /// </summary>
        public static byte[] Save(Image image, int image_type)
        {
            Debug.Assert(image_type == UNCOMPRESSED_TRUECOLOR_IMAGE);

            int size = 18 + image.Height * image.Width * 4;
            byte[] bytes = new byte[size];

            Channel a = image.Channels["A"];
            Channel r = image.Channels["R"];
            Channel g = image.Channels["G"];
            Channel b = image.Channels["B"];

            // Write header
            int p = 0;
            WriteByte(bytes, ref p, 0); // id length
            WriteByte(bytes, ref p, 0); // color map
            WriteByte(bytes, ref p, (byte)image_type);
            WriteZero(bytes, ref p, 5); // color map data
            WriteZero(bytes, ref p, 4); // x/y origin
            WriteUInt16(bytes, ref p, (UInt16)a.Width);
            WriteUInt16(bytes, ref p, (UInt16)a.Height);
            WriteByte(bytes, ref p, 32); // Pixel depth
            WriteByte(bytes, ref p, 8 + 32); // Descriptor

            int height = image.Height;
            int width = image.Width;
            if (image_type == UNCOMPRESSED_TRUECOLOR_IMAGE)
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        bytes[p] = (byte)(b.Data[y * width + x] * 255.0f); p++;
                        bytes[p] = (byte)(g.Data[y * width + x] * 255.0f); p++;
                        bytes[p] = (byte)(r.Data[y * width + x] * 255.0f); p++;
                        bytes[p] = (byte)(a.Data[y * width + x] * 255.0f); p++;
                    }
                }
            }
            return bytes;
        }
    }
}
