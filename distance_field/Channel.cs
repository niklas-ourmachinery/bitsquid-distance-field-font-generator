using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace distance_field
{
    /// <summary>
    /// Represents a channel in an image (such as: R, G, B, A).
    /// </summary>
    class Channel
    {
        int _width;
        int _height;

        /// <summary>
        /// Width of the channel.
        /// </summary>
        public int Width { get { return _width; } }

        /// <summary>
        /// Height of the channel.
        /// </summary>
        public int Height { get { return _height; } }
      
        /// <summary>
        /// Width*Height float pixels of data.
        /// </summary>
        public float[] Data;

        /// <summary>
        ///  Creates a new channel with specified with and height.
        /// </summary>
        public Channel(int width, int height)
        {
            _width = width;
            _height = height;
            Data = new float[width *height];
        }

        /// <summary>
        /// Creates a new channel with specified width and height and fills it with the specified value.
        /// </summary>
        public Channel(int width, int height, float fill)
        {
            _width = width;
            _height = height;
            Data = new float[width * height];
            for (int i = 0; i < width * height; ++i)
                Data[i] = fill;
        }
    }

    /// <summary>
    /// An image is a collection of named channels (such as R, G, B, A).
    /// </summary>
    class Image
    {
        public Dictionary<String, Channel> Channels = new Dictionary<String, Channel>();

        /// <summary>
        /// Width of image.
        /// </summary>
        public int Width { get { return Channels.Values.First().Width; } }

        /// <summary>
        /// Height of image.
        /// </summary>
        public int Height { get { return Channels.Values.First().Height; } }
    }
}
