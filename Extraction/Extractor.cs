using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SourceAFIS.General;
using SourceAFIS.Meta;
using SourceAFIS.Extraction.Filters;
using SourceAFIS.Extraction.Model;
using SourceAFIS.Extraction.Minutiae;
using SourceAFIS.Templates;
using Size = SourceAFIS.General.Size;

namespace SourceAFIS.Extraction
{
    public sealed class Extractor
    {
        [DpiAdjusted]
        [Parameter(Lower = 8, Upper = 32)]
        public int BlockSize = 15;

        public DpiAdjuster DpiAdjuster = new DpiAdjuster();
        [Nested]
        public LocalHistogram Histogram = new LocalHistogram();
        [Nested]
        public SegmentationMask Mask = new SegmentationMask();
        [Nested]
        public Equalizer Equalizer = new Equalizer();
        [Nested]
        public HillOrientation Orientation = new HillOrientation();
        [Nested]
        public OrientedSmoother RidgeSmoother = new OrientedSmoother();
        [Nested]
        public OrientedSmoother OrthogonalSmoother = new OrientedSmoother();
        [Nested]
        public ThresholdBinarizer Binarizer = new ThresholdBinarizer();
        [Nested]
        public VotingFilter BinarySmoother = new VotingFilter();
        [Nested]
        public Thinner Thinner = new Thinner();
        [Nested]
        public CrossRemover CrossRemover = new CrossRemover();
        [Nested]
        public RidgeTracer RidgeTracer = new RidgeTracer();
        [Nested]
        public InnerMask InnerMask = new InnerMask();
        [Nested]
        public MinutiaMask MinutiaMask = new MinutiaMask();
        [Nested]
        public DotRemover DotRemover = new DotRemover();
        [Nested]
        public PoreRemover PoreRemover = new PoreRemover();
        [Nested]
        public GapRemover GapRemover = new GapRemover();
        [Nested]
        public TailRemover TailRemover = new TailRemover();
        [Nested]
        public FragmentRemover FragmentRemover = new FragmentRemover();
        [Nested]
        public BranchMinutiaRemover BranchMinutiaRemover = new BranchMinutiaRemover();
        [Nested]
        public MinutiaCollector MinutiaCollector = new MinutiaCollector();
        [Nested]
        public MinutiaShuffler MinutiaSorter = new MinutiaShuffler();
        [Nested]
        public StandardDpiScaling StandardDpiScaling = new StandardDpiScaling();
        [Nested]
        public MinutiaCloudRemover MinutiaCloudRemover = new MinutiaCloudRemover();
        [Nested]
        public UniqueMinutiaSorter UniqueMinutiaSorter = new UniqueMinutiaSorter();

        public DetailLogger.Hook Logger = DetailLogger.Null;

        public int Count;

        public Extractor()
        {
            RidgeSmoother.Lines.StepFactor = 1.59f;
            OrthogonalSmoother.AngleOffset = Angle.PIB;
            OrthogonalSmoother.Lines.Radius = 4;
            OrthogonalSmoother.Lines.AngularResolution = 11;
            OrthogonalSmoother.Lines.StepFactor = 1.11f;
            BinarySmoother.Radius = 2;
            BinarySmoother.Majority = 0.61f;
            BinarySmoother.BorderDistance = 17;
            Count = 0;
        }

        public TemplateBuilder Extract(byte[,] invertedImage, int dpi)
        {
            TemplateBuilder template = null;
            DpiAdjuster.Adjust(this, dpi, delegate ()
            {
                byte[,] image = ImageInverter.GetInverted(invertedImage);

                BlockMap blocks = new BlockMap(new Size(image.GetLength(1), image.GetLength(0)), BlockSize);
                Logger.Log("BlockMap", blocks);
                //Testing Start
                var outFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "bloackMapOut" + DateTime.Now.Second + ".bin");
                var outFileText = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "blockMapTextOut" + DateTime.Now.Second + ".txt");
                var file = new FileStream(outFileDir, FileMode.CreateNew);
                var binWrite = new BinaryWriter(file);
                TextWriter tw = new StreamWriter(outFileText);
                LogSize(blocks.PixelCount, binWrite, tw, "PixelCount");
                LogSize(blocks.BlockCount, binWrite, tw, "BlockCount");
                LogSize(blocks.CornerCount, binWrite, tw, "CornerCount");
                LogRectangleC(blocks.AllBlocks, binWrite, tw, "AllBlocks");
                LogRectangleC(blocks.AllCorners, binWrite, tw, "AllCorners");
                LogPointGrid(blocks.Corners, binWrite, tw, "Corners");
                LogRectangleGrid(blocks.BlockAreas, binWrite, tw, "BlockAreas");
                LogPointGrid(blocks.BlockCenters, binWrite, tw, "BlockCenters");
                LogRectangleGrid(blocks.CornerAreas, binWrite, tw, "CornerAreas");
                binWrite.Close();
                tw.Close();
                //Testing End

                short[,,] histogram = Histogram.Analyze(blocks, image);
                ////testing
                //var outFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "histogramImageInput" + ".bin");
                //var file = new FileStream(outFileDir, FileMode.CreateNew);
                //var binWrite = new BinaryWriter(file);
                //binWrite.Write(image.GetLength(0));
                //binWrite.Write(image.GetLength(1));
                //for (var i = 0; i < image.GetLength(0); i++)
                //{
                //    for (var j = 0; j < image.GetLength(1); j++)
                //    {
                //        binWrite.Write(image[i, j]);
                //    }
                //}
                //binWrite.Close();
                ////End testing

                ////Testing Start     
                //Count++;
                //var outFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "histogramOut" + Count + ".bin");
                //var outFileText = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                //    "histogramTextOut" + Count + ".txt");
                //var file = new FileStream(outFileDir, FileMode.CreateNew);
                //var binWrite = new BinaryWriter(file);
                //TextWriter tw = new StreamWriter(outFileText);
                //binWrite.Write(histogram.GetLength(0));
                //tw.WriteLine(histogram.GetLength(0));
                //binWrite.Write(histogram.GetLength(1));
                //tw.WriteLine(histogram.GetLength(1));
                //binWrite.Write(histogram.GetLength(2));
                //tw.WriteLine(histogram.GetLength(2));
                //for (var i = 0; i < histogram.GetLength(0); i++)
                //{
                //    for (var j = 0; j < histogram.GetLength(1); j++)
                //    {
                //        for (var k = 0; k < histogram.GetLength(2); k++)
                //        {
                //            binWrite.Write(histogram[i, j, k]);
                //            tw.WriteLine(histogram[i, j, k]);
                //        }
                //    }
                //}
                //binWrite.Close();
                //file.Close();
                //tw.Close();
                //Testing Finish

                //Testing Start
                //outFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "bloackMapOutPostHis" + DateTime.Now.Second + ".bin");
                //outFileText = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                //   "blockMapTextOutPostHis" + DateTime.Now.Second + ".txt");
                //file = new FileStream(outFileDir, FileMode.CreateNew);
                //binWrite = new BinaryWriter(file);
                //tw = new StreamWriter(outFileText);
                //LogSize(blocks.PixelCount, binWrite, tw);
                //LogSize(blocks.BlockCount, binWrite, tw);
                //LogSize(blocks.CornerCount, binWrite, tw);
                //LogRectangleC(blocks.AllBlocks, binWrite, tw);
                //LogRectangleC(blocks.AllCorners, binWrite, tw);
                //LogPointGrid(blocks.Corners, binWrite, tw);
                //LogRectangleGrid(blocks.BlockAreas, binWrite, tw);
                //LogPointGrid(blocks.BlockCenters, binWrite, tw);
                //LogRectangleGrid(blocks.CornerAreas, binWrite, tw);
                //binWrite.Close();
                //tw.Close();
                //Testing End

                short[,,] smoothHistogram = Histogram.SmoothAroundCorners(blocks, histogram);
                BinaryMap mask = Mask.ComputeMask(blocks, histogram);
                float[,] equalized = Equalizer.Equalize(blocks, image, smoothHistogram, mask);

                byte[,] orientation = Orientation.Detect(equalized, mask, blocks);
                float[,] smoothed = RidgeSmoother.Smooth(equalized, orientation, mask, blocks);
                float[,] orthogonal = OrthogonalSmoother.Smooth(smoothed, orientation, mask, blocks);

                BinaryMap binary = Binarizer.Binarize(smoothed, orthogonal, mask, blocks);
                binary.AndNot(BinarySmoother.Filter(binary.GetInverted()));
                binary.Or(BinarySmoother.Filter(binary));
                Logger.Log("BinarySmoothingResult", binary);
                CrossRemover.Remove(binary);

                BinaryMap pixelMask = mask.FillBlocks(blocks);
                BinaryMap innerMask = InnerMask.Compute(pixelMask);

                BinaryMap inverted = binary.GetInverted();
                inverted.And(pixelMask);

                SkeletonBuilder ridges = null;
                SkeletonBuilder valleys = null;

                Parallel.Invoke(
                    () => { ridges = ProcessSkeleton("Ridges", binary); },
                    () => { valleys = ProcessSkeleton("Valleys", inverted); });

                template = new TemplateBuilder();
                template.OriginalDpi = dpi;
                template.OriginalWidth = invertedImage.GetLength(1);
                template.OriginalHeight = invertedImage.GetLength(0);

                MinutiaCollector.Collect(ridges, TemplateBuilder.MinutiaType.Ending, template);
                MinutiaCollector.Collect(valleys, TemplateBuilder.MinutiaType.Bifurcation, template);
                MinutiaMask.Filter(template, innerMask);
                StandardDpiScaling.Scale(template);
                MinutiaCloudRemover.Filter(template);
                UniqueMinutiaSorter.Filter(template);
                MinutiaSorter.Shuffle(template);
                Logger.Log("FinalTemplate", template);
            });
            return template;
        }

        SkeletonBuilder ProcessSkeleton(string name, BinaryMap binary)
        {
            SkeletonBuilder skeleton = null;

            DetailLogger.RunInContext(name, delegate ()
            {
                Logger.Log("Binarized", binary);
                BinaryMap thinned = Thinner.Thin(binary);
                skeleton = new SkeletonBuilder();
                RidgeTracer.Trace(thinned, skeleton);

                ////Testing Start     
                //var outFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "outputSkeletonRidgeTracer-" + name + DateTime.UtcNow.Millisecond + ".bin");
                //var file = new FileStream(outFileDir, FileMode.CreateNew);
                //var binWrite = new BinaryWriter(file);
                //binWrite.Write(skeleton.Minutiae.Count());
                //Console.WriteLine(skeleton.Minutiae.Count());
                //foreach (var minutia in skeleton.Minutiae)
                //{
                //    binWrite.Write(minutia.Valid);
                //    Console.WriteLine(minutia.Valid);
                //    binWrite.Write(minutia.Position.X);
                //    Console.WriteLine(minutia.Position.X);
                //    binWrite.Write(minutia.Position.Y);
                //    Console.WriteLine(minutia.Position.Y);
                //    binWrite.Write(minutia.Ridges.Count);
                //    Console.WriteLine(minutia.Ridges.Count);
                //}
                //binWrite.Close();
                //file.Close();
                ////Testing Finish

                DotRemover.Filter(skeleton);
                PoreRemover.Filter(skeleton);
                GapRemover.Filter(skeleton);
                TailRemover.Filter(skeleton);
                FragmentRemover.Filter(skeleton);
                BranchMinutiaRemover.Filter(skeleton);
            });
            Count++;
            return skeleton;
        }

        public void LogSize(Size pixelCount, BinaryWriter binWrite, TextWriter tw, string name)
        {
            binWrite.Write(pixelCount.Width);
            tw.WriteLine("{0} Width: {1}", name, pixelCount.Width);
            binWrite.Write(pixelCount.Height);
            tw.WriteLine("{0} Height: {1}", name, pixelCount.Height);
        }

        public void LogRectangleC(RectangleC rec, BinaryWriter binWrite, TextWriter tw, string name)
        {
            binWrite.Write(rec.X);
            tw.WriteLine("{1} X: {0} ", rec.X, name);
            binWrite.Write(rec.Y);
            tw.WriteLine("{1} Y: {0}", rec.Y, name);
            binWrite.Write(rec.Width);
            tw.WriteLine("{1} Width: {0}", rec.Width, name);
            binWrite.Write(rec.Height);
            tw.WriteLine("{1} Height: {0}", rec.Height, name);
            //binWrite.Write(rec.Bottom);
            //tw.WriteLine("Bottom: " + rec.Bottom);
            //binWrite.Write(rec.Left);
            //tw.WriteLine("Left: " + rec.Left);
            //binWrite.Write(rec.Top);
            //tw.WriteLine("Top: " + rec.Top);
            //binWrite.Write(rec.Right);
            //tw.WriteLine("Right: " + rec.Right);
            //binWrite.Write(rec.TotalArea);
            //tw.WriteLine("TotalArea: " + rec.TotalArea);
            //binWrite.Write(rec.Center.X);
            //tw.WriteLine("Center x: " + rec.Center.X);
            //binWrite.Write(rec.Center.Y);
            //tw.WriteLine("center y: " + rec.Center.Y);
            //binWrite.Write(rec.Point.X);
            //tw.WriteLine("point x: " + rec.Point.X);
            //binWrite.Write(rec.Point.Y);
            //tw.WriteLine("point y: " + rec.Point.Y);
            //binWrite.Write(rec.Size.Height);
            //tw.WriteLine("size height: " + rec.Size.Height);
            //binWrite.Write(rec.Size.Width);
            //tw.WriteLine("size width: " + rec.Size.Width);
            //binWrite.Write(rec.RangeX.End);
            //tw.WriteLine("rangex end: " + rec.RangeX.End);
            //binWrite.Write(rec.RangeX.Begin);
            //tw.WriteLine("rangex begin: " + rec.RangeX.Begin);
            //binWrite.Write(rec.RangeX.Length);
            //tw.WriteLine("rangex length: " + rec.RangeX.Length);
            //binWrite.Write(rec.RangeY.End);
            //tw.WriteLine("rangey end: " + rec.RangeY.End);
            //binWrite.Write(rec.RangeY.Begin);
            //tw.WriteLine("rangey begin: " + rec.RangeY.Begin);
            //binWrite.Write(rec.RangeY.Length);
            //tw.WriteLine("rangey length: " + rec.RangeY.Length);
        }

        public void LogPointGrid(BlockMap.PointGrid grid, BinaryWriter binWrite, TextWriter tw, string name)
        {
            binWrite.Write(grid.AllX.Count());
            tw.WriteLine("{1} AllX Length: {0}", grid.AllX.Count(), name);
            foreach (var i in grid.AllX)
            {
                binWrite.Write(i);
                tw.WriteLine(i);
            }
            binWrite.Write(grid.AllY.Count());
            tw.WriteLine("{1} AllY Length: {0}", grid.AllY.Count(), name);
            foreach (var i in grid.AllY)
            {
                binWrite.Write(i);
                tw.WriteLine(i);
            }
        }

        public void LogRectangleGrid(BlockMap.RectangleGrid grid, BinaryWriter binWrite, TextWriter tw, string name)
        {
            LogPointGrid(grid.Corners, binWrite, tw, name);
        }
    }
}
