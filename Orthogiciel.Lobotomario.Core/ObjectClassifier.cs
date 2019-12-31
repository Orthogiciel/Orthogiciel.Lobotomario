using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;

namespace Orthogiciel.Lobotomario.Core
{
    public class ObjectClassifier
    {
        private readonly GameObjectRepository gameObjectRepository;
        private Image<Bgr, Byte> tileset;
        private HOGDescriptor hogDescriptor;
        private SVM svm;

        public ObjectClassifier(GameObjectRepository gameObjectRepository)
        {
            this.gameObjectRepository = gameObjectRepository;
            this.tileset = new Image<Bgr, Byte>(Properties.Resources.Tileset);
            this.hogDescriptor = new HOGDescriptor(new Size(16, 16), new Size(8, 8), new Size(4, 4), new Size(8, 8));
            this.svm = new SVM();
            svm.SetKernel(SVM.SvmKernelType.Rbf);
            svm.Type = SVM.SvmType.CSvc;
            svm.C = 12.5;
            svm.Gamma = 0.50625;

            ComputeImagesHOGDescriptors();
            TrainObjectClassifier();
        }

        public float ClassifyImage(Bitmap snapshot)
        {
            var hogMatrix = new Matrix<float>(1, (int)this.hogDescriptor.DescriptorSize);
            var img = new Image<Bgr, Byte>(snapshot).GetSubRect(new Rectangle(new Point(0,0), new Size(16, 16)));
            var hog = hogDescriptor.Compute(img);

            for (var i = 0; i < hogMatrix.Cols; i++)
            {
                hogMatrix[0,i] = hog[i];
            }

            return svm.Predict(hogMatrix);
        }

        private void ComputeImagesHOGDescriptors()
        {
            gameObjectRepository.Tiles.ForEach(t =>
            {
                t.HogDescriptors = new Matrix<float>(t.SpritesheetPositions.Count, (int)this.hogDescriptor.DescriptorSize);

                for (var i = 0; i < t.SpritesheetPositions.Count; i++)
                {
                    Console.WriteLine($"Computing Image HOG Descriptors - Tile {t.TileType} - Position ({t.SpritesheetPositions[i].X},{t.SpritesheetPositions[i].Y})");

                    var img = this.tileset.GetSubRect(new Rectangle(t.SpritesheetPositions[i], new Size(t.Bounds.Height, t.Bounds.Width)));
                    var hog = hogDescriptor.Compute(img);

                    for (var j = 0; j < hog.Length; j++)
                    {
                        t.HogDescriptors[i, j] = hog[j];
                    }
                }
            });
        }

        private void TrainObjectClassifier()
        {
            var trainData = new Matrix<float>(0, 0);
            var trainClasses = new Matrix<int>(0, 0);

            gameObjectRepository.Tiles.ForEach(t =>
            {
                if (trainData.Rows == 0)
                {
                    trainData = t.HogDescriptors;
                    trainClasses = new Matrix<int>(t.HogDescriptors.Rows, 1);
                    trainClasses.SetValue((int)t.TileType);
                }
                else
                {
                    var newTrainClass = new Matrix<int>(t.HogDescriptors.Rows, 1);
                    newTrainClass.SetValue((int)t.TileType);

                    trainData = trainData.ConcateVertical(t.HogDescriptors);
                    trainClasses = trainClasses.ConcateVertical(newTrainClass);
                }
            });

            svm.Train(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses);
        }

        private void TestTileRecognition()
        {
            gameObjectRepository.Tiles.ForEach(t =>
            {
                Console.WriteLine($"Testing Tile {t.TileType}...");

                for (var i = 0; i < t.HogDescriptors.Rows; i++)
                {
                    Console.WriteLine($"Form {i}... Detected type : ");

                    var res = svm.Predict(t.HogDescriptors.GetRow(i));

                    if (res == 0)
                        Console.Write((TileTypes)0 + "\n");
                    else if (res == 1)
                        Console.Write((TileTypes)1 + "\n");
                    else
                        Console.Write("Unknown\n");
                }
            });
        }
    }
}
