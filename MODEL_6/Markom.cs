using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MODEL_6
{
    class Transition
    {
        public int To { get; set; }
        public double Rate { get; set; }
    }

    class Markom
    {
        private MainWindow mainWindow;
        private const int N = 5; // Количество состояний
        private List<List<Transition>> graph; // Граф переходов

        public Markom(MainWindow window)
        {
            graph = new List<List<Transition>>(N);
            for (int i = 0; i < N; i++)
            {
                graph.Add(new List<Transition>());
            }
            mainWindow = window;
            InitializeGraph();
        }

        private void InitializeGraph()
        {
            // Очищаем граф перед инициализацией
            graph.Clear();
            for (int i = 0; i < N; i++)
            {
                graph.Add(new List<Transition>());
            }

            // Создаем граф на основе значений из текстовых полей
            double[,] rates = new double[N, N];

            // Считывание значений из текстовых полей
            rates[0, 0] = double.Parse(mainWindow.S00.Text); rates[0, 1] = double.Parse(mainWindow.S01.Text); rates[0, 2] = double.Parse(mainWindow.S02.Text); rates[0, 3] = double.Parse(mainWindow.S03.Text); rates[0, 4] = double.Parse(mainWindow.S04.Text);
            rates[1, 0] = double.Parse(mainWindow.S10.Text); rates[1, 1] = double.Parse(mainWindow.S11.Text); rates[1, 2] = double.Parse(mainWindow.S12.Text); rates[1, 3] = double.Parse(mainWindow.S13.Text); rates[1, 4] = double.Parse(mainWindow.S14.Text);
            rates[2, 0] = double.Parse(mainWindow.S20.Text); rates[2, 1] = double.Parse(mainWindow.S21.Text); rates[2, 2] = double.Parse(mainWindow.S22.Text); rates[2, 3] = double.Parse(mainWindow.S23.Text); rates[2, 4] = double.Parse(mainWindow.S24.Text);
            rates[3, 0] = double.Parse(mainWindow.S30.Text); rates[3, 1] = double.Parse(mainWindow.S31.Text); rates[3, 2] = double.Parse(mainWindow.S32.Text); rates[3, 3] = double.Parse(mainWindow.S33.Text); rates[3, 4] = double.Parse(mainWindow.S34.Text);
            rates[4, 0] = double.Parse(mainWindow.S40.Text); rates[4, 1] = double.Parse(mainWindow.S41.Text); rates[4, 2] = double.Parse(mainWindow.S42.Text); rates[4, 3] = double.Parse(mainWindow.S43.Text); rates[4, 4] = double.Parse(mainWindow.S44.Text);

            // Инициализация переходов на основе считанных значений
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (i != j && rates[i, j] > 0) // Делаем переход только если ставка > 0
                    {
                        graph[i].Add(new Transition { To = j, Rate = rates[i, j] });
                    }
                }
            }
        }

        // Функция для построения матрицы переходных интенсивностей
        private double[,] BuildTransitionMatrix()
        {
            double[,] L = new double[N, N];
            for (int i = 0; i < N; i++)
            {
                foreach (var trans in graph[i])
                {
                    L[i, trans.To] = trans.Rate;
                }
            }
            return L;
        }

        // Функция для построения матрицы коэффициентов системы dP/dt = L P
        private double[,] BuildKolmogorovSystem(double[,] L)
        {
            double[,] A = new double[N, N];
            for (int i = 0; i < N; i++)
            {
                A[i, i] = 0.0;
                for (int j = 0; j < N; j++)
                {
                    if (i != j)
                    {
                        A[i, j] = L[j, i]; // Входящие переходы
                        A[i, i] -= L[i, j]; // Выходящие переходы
                    }
                }
            }
            return A;
        }

        // Метод Эйлера для численного решения системы ОДУ
        public List<double[]> EulerMethod(double[,] L, double[] P0, double dt, double t_max)
        {
            int steps = (int)(t_max / dt);
            var solutions = new List<double[]>();
            double[] P = (double[])P0.Clone();
            solutions.Add((double[])P.Clone());

            for (int step = 0; step <= steps; step++) // Начинаем с 1, так как 0 уже в solutions
            {
                double[] dP = new double[N];

                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (i != j)
                        {
                            dP[i] -= L[i, j] * P[i]; // Исходящие вероятность
                            dP[i] += L[j, i] * P[j];   // Входящие вероятность
                        }
                    }
                }

                for (int i = 0; i < N; i++)
                {
                    P[i] += dP[i] * dt;
                    // Обеспечим, что вероятности не уйдут за границы [0,1]
                    if (P[i] < 0) P[i] = 0;
                    if (P[i] > 1) P[i] = 1;
                }

                solutions.Add((double[])P.Clone()); // Сохраняем состояние после шага
            }

            return solutions;
        }

        // Запись результатов в CSV файл
        public void WriteToCSV(List<double[]> solutions, double dt, string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("Time S1 S2 S3 S4 S5"); // Заголовок
                for (int i = 0; i < solutions.Count; i++)
                {
                    writer.Write(i * dt);
                    foreach (var prob in solutions[i])
                    {
                        writer.Write(" " + Math.Round(prob, 5).ToString().Replace(',', '.'));
                    }
                    writer.WriteLine();
                }
            }
        }

        public void ShowMatricesInDialog()
        {
            var L = BuildTransitionMatrix();
            var A = BuildKolmogorovSystem(L);

            string LMatrixString = "Матрица переходных интенсивностей L:\n" + MatrixToString(L);
            string AMatrixString = "\nСистема Колмогорова (матрица A):\n" + MatrixToString(A);

            MessageBox.Show(LMatrixString + AMatrixString, "Результаты");
        }

        private string MatrixToString(double[,] matrix)
        {
            var rows = new List<string>();
            for (int i = 0; i < N; i++)
            {
                var columns = new List<string>();
                for (int j = 0; j < N; j++)
                {
                    columns.Add(Math.Round(matrix[i, j], 1).ToString("F1").Replace(',', '.')); // Формат с 5 знаками после запятой
                }
                rows.Add(string.Join("\t\t", columns));
            }
            return string.Join("\n", rows);
        }

        // Основная функция для обработки
        public string Run(double dt, double t_max)
        {
            // Построение первоначальных матриц
            double[,] L = BuildTransitionMatrix();
            double[,] A = BuildKolmogorovSystem(L);
            double[] P0 = { 1.0, 0.0, 0.0, 0.0, 0.0 }; // Начальные условия

            // Численное решение
            var solutions = EulerMethod(L, P0, dt, t_max);

            string LMatrixString = "Матрица переходных интенсивностей L:\n" + MatrixToString(L);
            string AMatrixString = "\nСистема Колмогорова (матрица A):\n" + MatrixToString(A);

            // Открываем новое окно с результатами
            string results = LMatrixString + AMatrixString + "\n";

            // Запись результатов в CSV файл
            WriteToCSV(solutions, dt, "output.csv");
            MessageBox.Show("Результаты численного решения записаны в файл output.csv", "Успех");

            // Нахождение предельных вероятностей
            if (FindSteadyState(A, out var steadyState))
            {
                var steadyStateString = "\nПредельные вероятности:\n" +
                                          string.Join("\n", steadyState.Select((p, index) => $"P{index + 1} = {p:F5}"));
                results += steadyStateString + "\n";
            }
            else
            {
                results += "\nОшибка. \nНе удалось найти предельные вероятности (система не имеет единственного решения)\n";
            }

            // Проверка эргодичности
            bool ergodic = IsStronglyConnected(graph);
            results += $"\nГраф {(ergodic ? "эргодичен." : "неэргодичен.")}\n";

            // Изменяем граф для демонстрации неэргодичности
            ModifyGraphForNonErgodicity();

            // Пересчет новых матриц
            double[,] newL = BuildTransitionMatrix();
            double[,] newA = BuildKolmogorovSystem(newL);

            // Формирование нового вывода
            results += "\nИзменяем граф для неэргодичности (удаляем связи из S1).\nНовый граф неэргодичен.\n";
            results += "Новая матрица переходных интенсивностей L:\n" + MatrixToString(newL);
            results += "\nНовая система Колмогорова (матрица A):\n" + MatrixToString(newA);

            // Нахождение новых предельных вероятностей
            if (FindSteadyState(newA, out var newSteadyState))
            {
                var newSteadyStateString = "\nНовые предельные вероятности:\n" +
                                            string.Join("\n", newSteadyState.Select((p, index) => $"P{index + 1} = {p:F5}"));
                results += newSteadyStateString + "\n";
            }
            else
            {
                results += "\nОшибка. \nНе удалось найти новые предельные вероятности (система не имеет единственного решения).\n";
            }

            return results;
        }
        private void ModifyGraphForNonErgodicity()
        {
            // Удаляем все переходы из состояния S1
            graph[0].Clear(); // S1 не имеет исходящих переходов
        }

        // Функция для решения системы линейных уравнений методом Гаусса
        private bool Gauss(double[,] a, double[] x)
        {
            int n = a.GetLength(0);
            int m = a.GetLength(1);
            for (int i = 0; i < n; i++)
            {
                // Поиск максимального элемента в столбце
                double maxEl = Math.Abs(a[i, i]);
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(a[k, i]) > maxEl)
                    {
                        maxEl = Math.Abs(a[k, i]);
                        maxRow = k;
                    }
                }

                // Перестановка строк
                for (int k = i; k < m; k++)
                {
                    double temp = a[maxRow, k];
                    a[maxRow, k] = a[i, k];
                    a[i, k] = temp;
                }

                // Проверка на ноль
                if (Math.Abs(a[i, i]) < 1e-12)
                {
                    return false; // Система не имеет единственного решения
                }

                // Приведение к треугольному виду
                for (int k = i + 1; k < n; k++)
                {
                    double c = -a[k, i] / a[i, i];
                    for (int j = i; j < m; j++)
                    {
                        a[k, j] += c * a[i, j];
                    }
                }
            }

            // Обратная подстановка
            for (int i = n - 1; i >= 0; i--)
            {
                if (Math.Abs(a[i, i]) < 1e-12)
                {
                    return false; // Система не имеет единственного решения
                }
                x[i] = a[i, m - 1] / a[i, i];
                for (int k = i - 1; k >= 0; k--)
                {
                    a[k, m - 1] -= a[k, i] * x[i];
                }
            }
            return true;
        }

        // Функция для нахождения предельных вероятностей
        private bool FindSteadyState(double[,] A, out double[] steadyState)
        {
            steadyState = new double[N];
            double[,] augmented = new double[N, N + 1];

            // Копируем первые (N-1) уравнений Колмогорова
            for (int i = 0; i < N - 1; ++i)
            {
                for (int j = 0; j < N; ++j)
                {
                    augmented[i, j] = A[i, j];
                }
                augmented[i, N] = 0.0; // Правая часть = 0
            }

            // Последнее уравнение — условие нормировки
            for (int j = 0; j < N; j++)
            {
                augmented[N - 1, j] = 1.0;
            }
            augmented[N - 1, N] = 1.0; // Правая часть = 1

            // Решение системы
            return Gauss(augmented, steadyState);
        }

        // Функция для проверки сильной связности графа (эргодичность)
        private bool IsStronglyConnected(List<List<Transition>> graph)
        {
            // Используем алгоритм BFS для каждого узла
            bool bfs(int start, bool[] visited)
            {
                Queue<int> q = new Queue<int>();
                q.Enqueue(start);
                visited[start] = true;
                while (q.Count > 0)
                {
                    int u = q.Dequeue();
                    foreach (var trans in graph[u])
                    {
                        if (!visited[trans.To])
                        {
                            visited[trans.To] = true;
                            q.Enqueue(trans.To);
                        }
                    }
                }

                return !visited.Contains(false); // Проверка на охват всех узлов
            }

            // Проверка для каждого узла
            for (int i = 0; i < N; i++)
            {
                bool[] visited = new bool[N];
                if (!bfs(i, visited)) return false;
            }
            return true;
        }
    }
}
