using DlibDotNet;
using DlibDotNet.Dnn;
using DlibInstanceSegmentaion.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DlibInstanceSegmentaion.General
{
    public static class InstanceSegmentaion
    {
        private const string SemanticSegmentationNetFilename = "Extra/semantic_segmentation_voc2012net_v2.dnn";
        public static SegmentInfo DoSegmentaion(string picturePath,string imageId)
        {
            try
            {
                string outputImageFolder = "segmented";
                SegmentInfo segmentInfo = null;
                if (!Directory.Exists(outputImageFolder))
                    Directory.CreateDirectory(outputImageFolder);
                using (var net = LossMulticlassLogPerPixel.Deserialize(SemanticSegmentationNetFilename, 3))
                {
                    // Load the input image.
                    using (var inputImage = Dlib.LoadImageAsMatrix<RgbPixel>(picturePath))
                    {
                        // Create predictions for each pixel. At this point, the type of each prediction
                        // is an index (a value between 0 and 20). Note that the net may return an image
                        // that is not exactly the same size as the input.
                        using (var output = net.Operator(inputImage))
                        using (var temp = output.First())
                        {
                            // Crop the returned image to be exactly the same size as the input.
                            var rect = Rectangle.CenteredRect((int)(temp.Columns / 2d), (int)(temp.Rows / 2d), (uint)inputImage.Columns, (uint)inputImage.Rows);
                            using (var dims = new ChipDims((uint)inputImage.Rows, (uint)inputImage.Columns))
                            using (var chipDetails = new ChipDetails(rect, dims))
                            using (var indexLabelImage = Dlib.ExtractImageChip<ushort>(temp, chipDetails, InterpolationTypes.NearestNeighbor))
                            {
                                // Convert the indexes to RGB values.
                                using (var rgbLabelImage = IndexLabelImageToRgbLabelImage(indexLabelImage))
                                {
                                    // Show the input image on the left, and the predicted RGB labels on the right.
                                    //using (var joinedRow = Dlib.JoinRows(inputImage, rgbLabelImage))
                                    string outputImagePath = outputImageFolder + "/" + imageId + ".png";
                                    Dlib.SavePng(rgbLabelImage, outputImagePath);
                                    
                                    // Find the most prominent class label from amongst the per-pixel predictions.
                                    var classLabel = GetMostProminentNonBackgroundClassLabel(indexLabelImage);

                                    segmentInfo = new SegmentInfo();
                                    segmentInfo.SegmentedImagePath = outputImagePath;
                                    segmentInfo.SegmentedObjectTitle = classLabel;
                                }
                            }
                        }
                    }
                }
                return segmentInfo;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static Matrix<RgbPixel> IndexLabelImageToRgbLabelImage(Matrix<ushort> indexLabelImage)
        {
            var nr = indexLabelImage.Rows;
            var nc = indexLabelImage.Columns;

            var rgbLabelImage = new Matrix<RgbPixel>(nr, nc);
            for (var r = 0; r < nr; ++r)
                for (var c = 0; c < nc; ++c)
                    rgbLabelImage[r, c] = PascalVOC2012.IndexLabelToRgbLabel(indexLabelImage[r, c]);

            return rgbLabelImage;
        }

        // Find the most prominent class label from amongst the per-pixel predictions.
        private static string GetMostProminentNonBackgroundClassLabel(Matrix<ushort> indexLabelImage)
        {
            var nr = indexLabelImage.Rows;
            var nc = indexLabelImage.Columns;

            var counters = new uint[PascalVOC2012.ClassCount];
            for (var r = 0; r < nr; ++r)
                for (var c = 0; c < nc; ++c)
                {
                    var label = indexLabelImage[r, c];
                    ++counters[label];
                }

            var maxValue = counters.ToList().GetRange(1, counters.Length - 1).Max();
            var mostProminentIndexLabel = counters.ToList().FindIndex(1, counters.Length - 1, u => u == maxValue);

            return PascalVOC2012.FindVoc2012Class((ushort)mostProminentIndexLabel).ClassLabel;
        }
    }
}
