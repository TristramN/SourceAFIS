using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SourceAFIS.General;

namespace SourceAFIS.Extraction.Model
{
    public sealed class DotRemover : ISkeletonFilter
    {
        public DetailLogger.Hook Logger = DetailLogger.Null;
        public int Count = 0;
        public void Filter(SkeletonBuilder skeleton)
        {
            ////Testing Start
            //Count++;
            //var inFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "skeletonInput" + Count + ".bin");
            //var outFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "skeletonInput" + Count + ".bin");
            //var file = new FileStream(inFileDir, FileMode.OpenOrCreate);
            //var binWrite = new BinaryWriter(file);

            //foreach (var i in skeleton.Minutiae)
            //{
            //    binWrite.Write(i.Valid);
            //    Console.WriteLine("Input Valid: " + i.Valid);
            //    binWrite.Write(i.Position.X);
            //    Console.WriteLine("Postion X: " + i.Position.X);
            //    binWrite.Write(i.Position.Y);
            //    Console.WriteLine("Position Y: " + i.Position.Y);

            //    foreach (var ridge in i.Ridges)
            //    {
            //        binWrite.Write(ridge.Start.Position.X);
            //        Console.WriteLine("Postion X: " + ridge.Start.Position.X);
            //        binWrite.Write(ridge.Start.Position.Y);
            //        Console.WriteLine("Position Y: " + ridge.Start.Position.Y);
            //        binWrite.Write(ridge.End.Position.X);
            //        Console.WriteLine("Postion X: " + ridge.End.Position.X);
            //        binWrite.Write(ridge.End.Position.Y);
            //        Console.WriteLine("Position Y: " + ridge.End.Position.Y);
            //        foreach (var point in ridge.Points)
            //        {
            //            binWrite.Write(point.X);
            //            binWrite.Write(point.Y);
            //        }
            //    }
            //    Console.WriteLine(i);
            //}
            //binWrite.Close();
            //file.Close();
            ////Testing Finish

            List<SkeletonBuilder.Minutia> removed = new List<SkeletonBuilder.Minutia>();
            foreach (SkeletonBuilder.Minutia minutia in skeleton.Minutiae)
                if (minutia.Ridges.Count == 0)
                    removed.Add(minutia);
            foreach (SkeletonBuilder.Minutia minutia in removed)
                skeleton.RemoveMinutia(minutia);
            Logger.Log(skeleton);
        }
    }
}
