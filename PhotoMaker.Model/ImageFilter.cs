using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoMaker.Model
{
    /// <summary>  Фильтр изображения  </summary>
    public class ImageFilter
    {
        /// <summary>  Список имеющихся фильтров  </summary>
        public static IReadOnlyList<IFilter> Modes = new List<IFilter>
        {
            new LinearFilter(),
            new MedianFilter()
        };


        #region IFilter : abstract class  -  Интерфейс фильтра изображения

        /// <summary>  Интерфейс фильтра изображения  </summary>
        public abstract class IFilter
        {
            /// <summary>  Имя фильтра  </summary>
            public abstract string Name { get; }


            /// <summary>  Фильтровать изображение  </summary>
            /// <param name="bitmap">  Битмап изображения  </param>
            /// <param name="kernel">  ядро фильтра  </param>
            /// <returns>  Обработанное фильтром изображение  </returns>
            public abstract unsafe BitmapSource FilterBitmap(BitmapSource bitmap, float[,] kernel);


            /// <summary>  Отразить индекс относительно центра 1D-массива  </summary>
            /// <param name="index">  Индекс пикселя  </param>
            /// <param name="max">  Максимальный индекс 1D-массива  </param>
            /// <returns>  Отраженный индекс  </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected int ReflectIndex1D(int index, int max)
            {
                if (index < 0) return -index - 1;
                if (index >= max) return 2 * max - index - 1;
                return index;
            }
        }

        #endregion // IFilterIFilter
        #region LinearFilter : class  -  Линейный фильтр

        /// <summary>  Линейный фильтр  </summary>
        public class LinearFilter : IFilter
        {
            public override string Name => "Linear";

            public override unsafe BitmapSource FilterBitmap(BitmapSource bitmap, float[,] kernel)
            {
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                int stride = (bitmap.Format.BitsPerPixel * width + 7) / 8;

                var pixels = new byte[height * stride];
                bitmap.CopyPixels(pixels, stride, 0);

                var output = new byte[pixels.Length];

                int kWidth = kernel.GetLength(0);
                int kHeight = kernel.GetLength(1);
                int radiusX = kWidth / 2;
                int radiusY = kHeight / 2;

                // Разворачиваем ядро в 1D массив
                float[] kernel1D = new float[kWidth * kHeight];
                for (int ky = 0; ky < kHeight; ky++)
                {
                    int row = ky * kWidth;
                    for (int kx = 0; kx < kWidth; kx++)
                        kernel1D[row + kx] = kernel[kx, ky];
                }

                int[] offsetX = new int[width * kWidth];
                int[] offsetY = new int[height * kHeight];

                for (int y = 0; y < height; y++)
                    for (int ky = -radiusY; ky <= radiusY; ky++)
                        offsetY[y * kHeight + (ky + radiusY)] = ReflectIndex1D(y + ky, height) * stride;

                for (int x = 0; x < width; x++)
                    for (int kx = -radiusX; kx <= radiusX; kx++)
                        offsetX[x * kWidth + (kx + radiusX)] = ReflectIndex1D(x + kx, width) * 4;

                fixed (byte* p1 = pixels)
                fixed (byte* p2 = output)
                {
                    byte* pSrc = p1;
                    byte* pDst = p2;

                    Action<int> processLine = y =>
                    {
                        byte* dstLine = pDst + y * stride;

                        for (int x = 0; x < width; x++)
                        {
                            float b = 0, g = 0, r = 0;

                            for (int ky = 0; ky < kHeight; ky++)
                            {
                                byte* srcLine = pSrc + offsetY[y * kHeight + ky];
                                int oxBase = x * kWidth;

                                for (int kx = 0; kx < kWidth; kx++)
                                {
                                    float w = kernel1D[ky * kWidth + kx];
                                    byte* srcPixel = srcLine + offsetX[oxBase + kx];

                                    b += srcPixel[0] * w;
                                    g += srcPixel[1] * w;
                                    r += srcPixel[2] * w;
                                }
                            }

                            byte* dstPixel = dstLine + x * 4;
                            dstPixel[0] = (byte)(b < 0 ? 0 : (b > 255 ? 255 : b));
                            dstPixel[1] = (byte)(g < 0 ? 0 : (g > 255 ? 255 : g));
                            dstPixel[2] = (byte)(r < 0 ? 0 : (r > 255 ? 255 : r));
                            dstPixel[3] = pSrc[y * stride + x * 4 + 3];
                        }
                    };

                    if (width * height > 500_000)
                        Parallel.For(0, height, processLine);
                    else
                        for (int y = 0; y < height; y++) processLine(y);
                }

                var result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, output, stride);
                result.Freeze();
                return result;
            }
        }

        #endregion // LinearFilter
        #region MedianFilter : class  -  Медианный фильтр

        /// <summary>  Медианный фильтр  </summary>
        public class MedianFilter : IFilter
        {
            public override string Name => "Median";

            public override unsafe BitmapSource FilterBitmap(BitmapSource bitmap, float[,] kernel)
            {
                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                int stride = (bitmap.Format.BitsPerPixel * width + 7) / 8;

                var pixels = new byte[height * stride];
                bitmap.CopyPixels(pixels, stride, 0);

                var output = new byte[pixels.Length];

                int kWidth = kernel.GetLength(0);
                int kHeight = kernel.GetLength(1);
                int radiusX = kWidth / 2;
                int radiusY = kHeight / 2;

                int[] offsetX = new int[width * kWidth];
                int[] offsetY = new int[height * kHeight];

                for (int y = 0; y < height; y++)
                    for (int ky = -radiusY; ky <= radiusY; ky++)
                        offsetY[y * kHeight + (ky + radiusY)] = ReflectIndex1D(y + ky, height) * stride;

                for (int x = 0; x < width; x++)
                    for (int kx = -radiusX; kx <= radiusX; kx++)
                        offsetX[x * kWidth + (kx + radiusX)] = ReflectIndex1D(x + kx, width) * 4;

                fixed (byte* p1 = pixels)
                fixed (byte* p2 = output)
                {
                    byte* pSrc = p1;
                    byte* pDst = p2;

                    Action<int> processLine = y =>
                    {
                        byte* dstLine = pDst + y * stride;

                        for (int x = 0; x < width; x++)
                        {
                            int total = kWidth * kHeight;
                            byte[] bVals = new byte[total];
                            byte[] gVals = new byte[total];
                            byte[] rVals = new byte[total];

                            int idx = 0;
                            for (int ky = 0; ky < kHeight; ky++)
                            {
                                byte* srcLine = pSrc + offsetY[y * kHeight + ky];
                                int oxBase = x * kWidth;

                                for (int kx = 0; kx < kWidth; kx++)
                                {
                                    byte* srcPixel = srcLine + offsetX[oxBase + kx];
                                    bVals[idx] = srcPixel[0];
                                    gVals[idx] = srcPixel[1];
                                    rVals[idx] = srcPixel[2];
                                    idx++;
                                }
                            }

                            Array.Sort(bVals);
                            Array.Sort(gVals);
                            Array.Sort(rVals);

                            byte* dstPixel = dstLine + x * 4;
                            dstPixel[0] = bVals[total / 2];
                            dstPixel[1] = gVals[total / 2];
                            dstPixel[2] = rVals[total / 2];
                            dstPixel[3] = pSrc[y * stride + x * 4 + 3];
                        }
                    };

                    if (width * height > 500_000)
                        Parallel.For(0, height, processLine);
                    else
                        for (int y = 0; y < height; y++) processLine(y);
                }

                var result = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, output, stride);
                result.Freeze();
                return result;
            }
        }

        #endregion // MedianFilter
    }
}
