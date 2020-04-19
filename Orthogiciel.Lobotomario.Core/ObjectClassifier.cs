using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using Orthogiciel.Lobotomario.Core.GameObjects;

namespace Orthogiciel.Lobotomario.Core
{
    public class ObjectClassifier
    {
        private readonly GameObjectRepository gameObjectRepository;
        private Image<Bgr, Byte> tileset;
        private Image<Bgr, Byte> playerSet;
        private HOGDescriptor hogDescriptor;
        private SVM svm;

        public ObjectClassifier(GameObjectRepository gameObjectRepository)
        {
            this.gameObjectRepository = gameObjectRepository;
            this.tileset = new Image<Bgr, Byte>(Properties.Resources.Tileset);
            this.playerSet = new Image<Bgr, byte>(Properties.Resources.Player);
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
            var img = new Image<Bgr, Byte>(snapshot);
            var hog = hogDescriptor.Compute(img);

            for (var i = 0; i < hogMatrix.Cols; i++)
            {
                hogMatrix[0,i] = hog[i];
            }

            return svm.Predict(hogMatrix);
        }

        public Mario GetMario(int objectClass)
        {
            switch (objectClass)
            {
                case ObjectClasses.Mario:
                    return gameObjectRepository.Marios.FirstOrDefault(m => m.MarioForm == MarioForms.Mini);
                case ObjectClasses.SuperMario:
                    return gameObjectRepository.Marios.FirstOrDefault(m => m.MarioForm == MarioForms.Super);
                case ObjectClasses.FieryMario:
                    return gameObjectRepository.Marios.FirstOrDefault(m => m.MarioForm == MarioForms.Fiery);
                case ObjectClasses.InvincibleMario:
                    return gameObjectRepository.Marios.FirstOrDefault(m => m.MarioForm == MarioForms.Invincible);
                default:
                    throw new InvalidOperationException("Aucun type de Mario ne correspond à cette classe d'objet !");
            }
        }

        private void ComputeImagesHOGDescriptors()
        {
            // ComputeHogDescriptors for Tiles
            foreach (Tile t in gameObjectRepository.Tiles)
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
            };

            // ComputeHogDescriptors for Marios
            foreach (Mario m in gameObjectRepository.Marios)
            {
                m.HogDescriptors = new Matrix<float>(m.SpritesheetPositions.Count, (int)this.hogDescriptor.DescriptorSize);

                for (var i = 0; i < m.SpritesheetPositions.Count; i++)
                {
                    ComputeHogDescriptorForMarioSpritesheetPosition(m, i, 0);

                    if (m.Bounds.Height == 32)
                    {
                        ComputeHogDescriptorForMarioSpritesheetPosition(m, i, 16);
                    }
                }
            };
        }

        private void ComputeHogDescriptorForMarioSpritesheetPosition(Mario mario, int i, int y_offset)
        {
            Console.WriteLine($"Computing Image HOG Descriptors - Mario {mario.MarioForm} - Position ({mario.SpritesheetPositions[i].X},{mario.SpritesheetPositions[i].Y + y_offset})");

            var img = this.playerSet.GetSubRect(new Rectangle(new Point(mario.SpritesheetPositions[i].X, mario.SpritesheetPositions[i].Y + y_offset), new Size(16, 16)));
            var hog = hogDescriptor.Compute(img);

            for (var j = 0; j < hog.Length; j++)
            {
                mario.HogDescriptors[i, j] = hog[j];
            }
        }

        private void TrainObjectClassifier()
        {
            var trainData = new Matrix<float>(0, 0);
            var trainClasses = new Matrix<int>(0, 0);

            // Add background tiles to training data
            for (var i = 0; i + 16 < this.tileset.Width; i += 16)
            {
                for (var j = 0; j + 16 < this.tileset.Height; j += 16)
                {
                    var position = new Point(i, j);

                    if (!gameObjectRepository.Tiles.Any(t => t.SpritesheetPositions.Any(p => p == position)))
                    {
                        var img = this.tileset.GetSubRect(new Rectangle(position, new Size(16, 16)));
                        var hog = hogDescriptor.Compute(img);

                        if (trainData.Rows == 0)
                        {
                            trainData = new Matrix<float>(1, hog.Length);

                            for (var k = 0; k < hog.Length; k++)
                            {
                                trainData[0, k] = hog[k];
                            }

                            trainClasses = new Matrix<int>(1, 1);
                            trainClasses.SetValue(ObjectClasses.Background);
                        }
                        else
                        {
                            var newTrainClass = new Matrix<int>(1, 1);
                            newTrainClass.SetValue(ObjectClasses.Background);

                            var newHogMatrix = new Matrix<float>(1, hog.Length);
                            for (var k = 0; k < hog.Length; k++)
                            {
                                newHogMatrix[0, k] = hog[k];
                            }

                            trainData = trainData.ConcateVertical(newHogMatrix);
                            trainClasses = trainClasses.ConcateVertical(newTrainClass);
                        }
                    }
                }
            }

            // Add significative tiles to training data
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

            // Add Marios to training data
            foreach (Mario m in gameObjectRepository.Marios)
            {
                if (trainData.Rows == 0)
                {
                    trainData = m.HogDescriptors;
                    trainClasses = new Matrix<int>(m.HogDescriptors.Rows, 1);
                    trainClasses.SetValue(GetMarioClass(m));
                }
                else
                {
                    var newTrainClass = new Matrix<int>(m.HogDescriptors.Rows, 1);
                    newTrainClass.SetValue(GetMarioClass(m));

                    trainData = trainData.ConcateVertical(m.HogDescriptors);
                    trainClasses = trainClasses.ConcateVertical(newTrainClass);
                }
            }           

            svm.Train(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses);
        }

        private int GetMarioClass(Mario mario)
        {
            switch (mario.MarioForm)
            {
                case MarioForms.Mini:
                    return ObjectClasses.Mario;
                case MarioForms.Super:
                    return ObjectClasses.Mario;
                case MarioForms.Fiery:
                    return ObjectClasses.Mario;
                case MarioForms.Invincible:
                    return ObjectClasses.Mario;
                default:
                    throw new InvalidOperationException("Aucune classe d'objet n'est configurée pour ce type de Mario !");
            }
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
