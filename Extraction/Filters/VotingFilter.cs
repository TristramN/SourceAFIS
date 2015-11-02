using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SourceAFIS.General;
using SourceAFIS.Meta;

namespace SourceAFIS.Extraction.Filters
{
    public sealed class VotingFilter
    {
        [Parameter(Upper = 10)]
        public int Radius = 1;
        [Parameter(Lower = 0.51)]
        public float Majority = 0.51f;
        [Parameter(Lower = 0, Upper = 20)]
        public int BorderDistance = 0;

        public static int Count = 0;

        public DetailLogger.Hook Logger = DetailLogger.Null;

        public BinaryMap Filter(BinaryMap input)
        {
            ////Testing Start
            //Count++;
            //var inFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "inputBinary" + Count + ".bin");
            //var outFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "outputBinary" + Count + ".bin");
            //var file = new FileStream(inFileDir, FileMode.CreateNew);
            //var binWrite = new BinaryWriter(file);
            //binWrite.Write(input.WordWidth);
            //Console.WriteLine(input.WordWidth);
            //binWrite.Write(input.Width);
            //Console.WriteLine(input.Width);
            //binWrite.Write(input.Height);
            //Console.WriteLine(input.Height);
            //foreach (var i in input.Map)
            //{
            //    binWrite.Write(i);
            //    Console.WriteLine(i);
            //}
            //binWrite.Close();
            //file.Close();
            ////Testing Finish

            RectangleC rect = new RectangleC(new Point(BorderDistance, BorderDistance),
                new Size(input.Width - 2 * BorderDistance, input.Height - 2 * BorderDistance));
            BinaryMap output = new BinaryMap(input.Size);
            Parallel.For(rect.RangeY.Begin, rect.RangeY.End, delegate (int y)
            {
                for (int x = rect.Left; x < rect.Right; ++x)
                {
                    RectangleC neighborhood = new RectangleC(
                        new Point(Math.Max(x - Radius, 0), Math.Max(y - Radius, 0)),
                        new Point(Math.Min(x + Radius + 1, output.Width), Math.Min(y + Radius + 1, output.Height)));

                    int ones = 0;
                    for (int ny = neighborhood.Bottom; ny < neighborhood.Top; ++ny)
                        for (int nx = neighborhood.Left; nx < neighborhood.Right; ++nx)
                            if (input.GetBit(nx, ny))
                                ++ones;

                    double voteWeight = 1.0 / neighborhood.TotalArea;
                    if (ones * voteWeight >= Majority)
                        output.SetBitOne(x, y);
                }
            });
            Logger.Log(output);

            ////Testing          
            //file = new FileStream(outFileDir, FileMode.CreateNew);
            //binWrite = new BinaryWriter(file);
            //binWrite.Write(output.WordWidth);
            //binWrite.Write(output.Width);
            //binWrite.Write(output.Height);
            //foreach (var i in output.Map)
            //{
            //    binWrite.Write(i);
            //}
            //binWrite.Close();
            //file.Close();
            ////Testing End

            return output;
        }
    }
}
