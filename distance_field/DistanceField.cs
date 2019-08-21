using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace distance_field
{
    class DistanceField
    {
        /// <summary>
        /// Computes a distance field transform of a high resolution binary source channel
        /// and returns the result as a low resolution channel.
        /// </summary>
        /// <param name="input">The source channel.</param>
        /// <param name="scale_down">The amount the source channel will be scaled down.
        /// A value of 8 means the destination image will be 1/8th the size of the source
        /// image.
        /// </param>
        /// <param name="spread">
        /// The spread in pixels before the distance field clamps to (zero/one). The value
        /// is specified in units of the destination image. The spread in the source image
        /// will be spread*scale_down.
        /// </param>
        /// <returns></returns>
        public static Channel Transform(Channel input, int scale_down, float spread)
        {
            Channel output = new Channel(input.Width / scale_down, input.Height / scale_down);
            int ow = output.Width;
            int oh = output.Height;

            float source_spread = spread * scale_down;

            for (int y=0; y<oh; ++y) {
                for (int x = 0; x < ow; ++x)
                {
                    float sd = SignedDistance(input, x*scale_down + scale_down/2, y*scale_down + scale_down/2, source_spread);
                    output.Data[y*ow + x] = (sd + source_spread) / (source_spread*2);
                }
            }

            return output;
        }

        private static float SignedDistance(Channel c, int cx, int cy, float clamp)
        {
            int w = c.Width;
            int h = c.Height;

            float cd = c.Data[cy * w + cx] - 0.5f;

            int min_x = cx - (int)clamp - 1;
            if (min_x < 0) min_x = 0;
            int max_x = cx + (int)clamp + 1;
            if (max_x >= w) max_x = w-1;

            float distance = clamp;
            for (int dy = 0; dy <= (int)clamp + 1; ++dy) {
                if (dy > distance) continue;
                
                if (cy - dy >=0) {
                    int y1 = cy-dy;
                    for (int x = min_x; x <= max_x; ++x)
                    {
                        if (x - cx > distance) continue;
                        float d = c.Data[y1*w+x] - 0.5f;
                        if (cd*d<0) {
                            float d2 = (y1 - cy)*(y1 - cy) + (x-cx)*(x-cx);
                            if (d2 < distance*distance)
                                distance = (float)Math.Sqrt(d2);
                        }
                    }
                }

                if (dy != 0 && cy+dy < h) {
                    int y2 = cy + dy;
                    for (int x = min_x; x <= max_x; ++x)
                    {
                        if (x - cx > distance) continue;
                        float d = c.Data[y2*w+x] - 0.5f;
                        if (cd*d<0) {
                            float d2 = (y2 - cy)*(y2 - cy) + (x-cx)*(x-cx);
                            if (d2 < distance*distance)
                                distance = (float)Math.Sqrt(d2);
                        }
                    }
                }
            }

            if (cd > 0)
                return distance;
            else
                return -distance;
        }
    }
}
