using System;
using System.IO;
using System.Threading;
using Xunit;
using MODEL_6;
using System.Windows.Threading;

namespace UnitTestProject
{
    public class UnitTest1
    {
        // Проверяет, что метод BuildTransitionMatrix возвращает корректную матрицу переходов.
        [Fact]
        public void TransitionMatrixTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                var mainWindow = new MainWindow();
                var markom = new Markom(mainWindow);
                markom.InitializeGraph();

                var matrix = markom.BuildTransitionMatrix();

                Assert.NotNull(matrix);
                Assert.Equal(5, matrix.GetLength(0));
                Assert.Equal(5, matrix.GetLength(1));
                result = true;

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }

        // Проверяет, что метод InitializeGraph корректно инициализирует граф.
        [Fact]
        public void GraphInitializationTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                var mainWindow = new MainWindow();
                var markom = new Markom(mainWindow);

                markom.InitializeGraph();

                var graph = markom.GetGraph();
                Assert.NotNull(graph);
                Assert.Equal(5, graph.Count);
                result = true;

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }

        // Проверяет, что метод BuildKolmogorovSystem возвращает корректную матрицу системы Колмогорова.
        [Fact]
        public void KolmogorovSystemTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                var mainWindow = new MainWindow();
                var markom = new Markom(mainWindow);
                markom.InitializeGraph();
                var L = markom.BuildTransitionMatrix();

                var A = markom.BuildKolmogorovSystem(L);

                Assert.NotNull(A);
                Assert.Equal(5, A.GetLength(0));
                Assert.Equal(5, A.GetLength(1));
                result = true;

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }

        // Проверяет, что метод EulerMethod возвращает корректные решения.
        [Fact]
        public void EulerMethodTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                using (var writer = new StreamWriter("test_output.txt"))
                {
                    Console.SetOut(writer);

                    var mainWindow = new MainWindow();
                    var markom = new Markom(mainWindow);
                    markom.InitializeGraph();
                    var L = markom.BuildTransitionMatrix();
                    double[] P0 = { 1.0, 0.0, 0.0, 0.0, 0.0 };
                    double dt = 0.1;
                    double t_max = 1.0;

                    Assert.NotNull(L);
                    Assert.Equal(5, L.GetLength(0));
                    Assert.Equal(5, L.GetLength(1));
                    Assert.NotNull(P0);
                    Assert.Equal(5, P0.Length);

                    var solutions = markom.EulerMethod(L, P0, dt, t_max);

                    Console.WriteLine($"Количество шагов: {solutions.Count}");
                    Console.WriteLine("Решения:");
                    foreach (var solution in solutions)
                    {
                        Console.WriteLine(string.Join(", ", solution));
                    }

                    Assert.NotNull(solutions);
                    Assert.NotEmpty(solutions);

                    int expectedSteps = (int)(t_max / dt) + 2;
                    Assert.Equal(expectedSteps, solutions.Count);

                    foreach (var solution in solutions)
                    {
                        Assert.NotNull(solution);
                        Assert.Equal(5, solution.Length);
                    }

                    result = true;

                    Dispatcher.CurrentDispatcher.InvokeShutdown();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }

        // Проверяет, что метод WriteToCSV создает файл с корректными данными.
        [Fact]
        public void WriteToCSVTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                var mainWindow = new MainWindow();
                var markom = new Markom(mainWindow);
                markom.InitializeGraph();
                var L = markom.BuildTransitionMatrix();
                double[] P0 = { 1.0, 0.0, 0.0, 0.0, 0.0 };
                var solutions = markom.EulerMethod(L, P0, 0.1, 1.0);
                string filePath = "test_output.csv";

                markom.WriteToCSV(solutions, 0.1, filePath);

                Assert.True(File.Exists(filePath));
                var lines = File.ReadAllLines(filePath);
                Assert.Equal(13, lines.Length);
                result = true;

                File.Delete(filePath);

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }

        // Проверяет, что метод FindSteadyState возвращает предельные вероятности.
        [Fact]
        public void SteadyStateTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                var mainWindow = new MainWindow();
                var markom = new Markom(mainWindow);
                markom.InitializeGraph();
                var L = markom.BuildTransitionMatrix();
                var A = markom.BuildKolmogorovSystem(L);

                bool success = markom.FindSteadyState(A, out var steadyState);

                Assert.True(success);
                Assert.NotNull(steadyState);
                Assert.Equal(5, steadyState.Length);
                result = true;

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }

        // Проверяет, что метод IsStronglyConnected корректно определяет сильную связность графа.
        [Fact]
        public void StronglyConnectedTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                var mainWindow = new MainWindow();
                var markom = new Markom(mainWindow);
                markom.InitializeGraph();

                bool isStronglyConnected = markom.IsStronglyConnected(markom.GetGraph());

                Assert.True(isStronglyConnected);
                result = true;

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }

        // Проверяет, что метод MatrixToString возвращает корректное строковое представление матрицы.
        [Fact]
        public void MatrixToStringTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                    var mainWindow = new MainWindow();
                    var markom = new Markom(mainWindow);
                    double[,] matrix = { { 1.0, 2.0 }, { 3.0, 4.0 } };

                    var matrixString = markom.MatrixToString(matrix, 2);

                    Assert.Equal("1.0\t\t2.0\n3.0\t\t4.0", matrixString);
                    result = true;
                
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }

        // Проверяет, что метод ModifyGraphForNonErgodicity делает граф неэргодичным.
        [Fact]
        public void NonErgodicGraphTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                var mainWindow = new MainWindow();
                var markom = new Markom(mainWindow);
                markom.InitializeGraph();

                markom.ModifyGraphForNonErgodicity();
                bool isErgodic = markom.IsStronglyConnected(markom.GetGraph());

                Assert.False(isErgodic);
                result = true;

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }

        // Проверяет, что метод Gauss корректно решает систему линейных уравнений.
        [Fact]
        public void GaussSolverTest()
        {
            var result = false;
            var thread = new Thread(() =>
            {
                var mainWindow = new MainWindow();
                var markom = new Markom(mainWindow);
                double[,] a = { { 2, 1, -1, 8 }, { -3, -1, 2, -11 }, { -2, 1, 2, -3 } };
                double[] x = new double[3];

                bool success = markom.Gauss(a, x);

                Assert.True(success);
                Assert.Equal(2, x[0], 5);
                Assert.Equal(3, x[1], 5);
                Assert.Equal(-1, x[2], 5);
                result = true;

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.True(result);
        }
    }
}