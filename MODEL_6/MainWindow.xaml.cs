using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;

namespace MODEL_6
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            cartesianChart.AxisY[0].MinValue = 0; // Нижняя граница
            cartesianChart.AxisX[0].MinValue = 0; // Нижняя граница

        }
        private void RunMarkomModel()
        {
            Markom model = new Markom(this);
            double dt = double.Parse(StepTB.Text, System.Globalization.CultureInfo.InvariantCulture); // Шаг
            double t_max = double.Parse(TimeTB.Text, System.Globalization.CultureInfo.InvariantCulture); ; // Время моделирования

            ResultsTextBox.Text = model.Run(dt, t_max);
            // Теперь загрузите данные из output.csv и отобразите их на графике

            LoadData();
        }

        private void LoadData()
        {
            // Загрузка данных из файла CSV
            string[] lines = File.ReadAllLines("output.csv");
            var data = new List<DataPoint>();

            // Пропускаем строку заголовка
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(' ');
                if (values.Length >= 6)
                {
                    double time = double.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture); // Время
                    double S1 = double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
                    double S2 = double.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
                    double S3 = double.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture);
                    double S4 = double.Parse(values[4], System.Globalization.CultureInfo.InvariantCulture);
                    double S5 = double.Parse(values[5], System.Globalization.CultureInfo.InvariantCulture);

                    data.Add(new DataPoint { Time = time, S1 = S1, S2 = S2, S3 = S3, S4 = S4, S5 = S5 });
                }
            }

            // Проверка, что данные загружены
            if (data.Any())
            {
                // Настройка графика
                var seriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "S1",
                        Values = new ChartValues<double>(data.Select(dp => dp.S1)),
                        PointGeometry = null
                    },
                    new LineSeries
                    {
                        Title = "S2",
                        Values = new ChartValues<double>(data.Select(dp => dp.S2)),
                        PointGeometry = null
                    },
                    new LineSeries
                    {
                        Title = "S3",
                        Values = new ChartValues<double>(data.Select(dp => dp.S3)),
                        PointGeometry = null
                    },
                    new LineSeries
                    {
                        Title = "S4",
                        Values = new ChartValues<double>(data.Select(dp => dp.S4)),
                        PointGeometry = null
                    },
                    new LineSeries
                    {
                        Title = "S5",
                        Values = new ChartValues<double>(data.Select(dp => dp.S5)),
                        PointGeometry = null
                    }
                };
                
                // Присваиваем коллекцию графику
                cartesianChart.Series = seriesCollection;
                // Установка границ по оси Y
                cartesianChart.AxisY[0].MinValue = 0; // Нижняя граница

                // Настройка оси X
                cartesianChart.AxisX[0].Labels = data.Select(dp => dp.Time.ToString()).ToArray();
                cartesianChart.Background = Brushes.Transparent; // Убираем фон, если он был установлен


            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RunMarkomModel();
        }
    }

    // Класс для хранения данных
    public class DataPoint
    {
        public double Time { get; set; }
        public double S1 { get; set; }
        public double S2 { get; set; }
        public double S3 { get; set; }
        public double S4 { get; set; }
        public double S5 { get; set; }
    }
}

