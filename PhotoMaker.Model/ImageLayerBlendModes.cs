using System.Runtime.CompilerServices;

namespace PhotoMaker.Model
{
    public class ImageLayerBlendModes
    {
        /// <summary>  Режимы наложения  </summary>
        public static IReadOnlyList<IBlendMode> Modes = new List<IBlendMode>
        {
            new NormalMode(),
            new SumMode(),
            new SubtractionMode(),
            new MultiplyMode(),
            new InversionMode(),
            new MaxMode(),
            new MinMode(),
            new MeanMode(),
            new GeomMeanMode()
        };


        #region IBlendMode : abstract class  -  Интерфейс режима наложения

        public abstract class IBlendMode
        {
            public abstract string Name { get; }

            /// <summary>  Выполнить наложение пикселей  </summary>
            /// <param name="p">  Основной пиксель  </param>
            /// <param name="pb">  Накладываемый пиксель  </param>
            /// <param name="pbA">  Прозрачность накладываемого пикселя  </param>
            /// <returns>  Результирующее значение пикселя  </returns>
            public abstract byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255);

            /// <summary>  Выполнить расчет прозрачности при наложении пикселей  </summary>
            /// <param name="pA"></param>
            /// <param name="pbA"></param>
            /// <returns>  Результирующее значение прозрачности  </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte CalcOpacityPbgra32(byte pA = 255, byte pbA = 255)
            {
                if (pA == pbA) return pA;

                int basePart = pA * ((255 - pbA) + 128) >> 8;
                if (basePart > 255 - pbA) { basePart = 255 - pbA; }
                return (byte)(basePart + pbA);
            }
        }

        #endregion // IBlendMode
        #region NormalMode : class  -  "Нормальный"

        public class NormalMode : IBlendMode
        {
            public override string Name => "Normal";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255)
            {
                int basePart = (p * (255 - pbA) + 128) >> 8;
                if (basePart > 255 - pb) { basePart = 255 - pb; }
                return (byte)(basePart + pb);
            }
        }

        #endregion // NormalMode
        #region SumMode : class  -  "Сумма"

        public class SumMode : IBlendMode
        {
            public override string Name => "Sum";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255)
            {
                return (byte)(p + pb > 255 ? 255 : p + pb);
            }
        }

        #endregion // SumMode
        #region SubtractionMode : class  -  "Разность"

        public class SubtractionMode : IBlendMode
        {
            public override string Name => "Subtraction";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255)
            {
                return (byte)(p - pb > 0 ? p - pb : 0);
            }
        }

        #endregion // SubtractionMode
        #region MultiplyMode : class  -  "Произведение"

        public class MultiplyMode : IBlendMode
        {
            public override string Name => "Multiply";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255)
            {
                return (byte)(((p * pb) + 127) >> 8);
            }
        }

        #endregion // MultiplyMode
        #region InversionMode : class  -  "Инверсия"

        public class InversionMode : IBlendMode
        {
            public override string Name => "Inversion";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255)
            {
                int result = 255 - ((p + pb + 1) >> 1);
                return (byte)(result < 0 ? 0 : result);
            }
        }

        #endregion // InversionMode
        #region MaxMode : class  -  "Максимум"

        public class MaxMode : IBlendMode
        {
            public override string Name => "Max";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255)
            {
                return p > pb ? p : pb;
            }
        }

        #endregion // MaxMode
        #region MinMode : class  -  "Минимум"

        public class MinMode : IBlendMode
        {
            public override string Name => "Min";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255)
            {
                return p > pb ? pb : p;
            }
        }

        #endregion // MinMode
        #region MeanMode : class  -  "Среднее"

        public class MeanMode : IBlendMode
        {
            public override string Name => "Mean";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255)
            {
                return (byte)((p + pb + 1) >> 1);
            }
        }

        #endregion // MeanMode
        #region GeomMeanMode : class  -  "Среднее геометрическое"
        public class GeomMeanMode : IBlendMode
        {
            public override string Name => "GeomMean";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override byte BlendPixelPbgra32(byte p, byte pb, byte pbA = 255)
            {
                return (byte)Math.Sqrt(p * pb);
            }
        }
        #endregion // GeomMeanMode
    }
}
