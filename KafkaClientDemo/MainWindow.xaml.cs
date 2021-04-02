using System;
using System.Collections.Generic;
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

/***
 * Includes needed for Kafka Client
 ***/
using System.Threading;
using Confluent.Kafka;

namespace KafkaClientDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            this._cts = new CancellationTokenSource();
            InitializeComponent();
        }

        private async void ConsumeTelemetry(object taskState)
        {
            var token = (CancellationToken) taskState;
            string kafkaBroker = "";

            this.Dispatcher.Invoke(() =>
            {
                kafkaBroker = txtKafkaBrokerIP.Text;
            });

            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaBroker,
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe("mikaylas");

                while(!token.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume();
                    this.Dispatcher.Invoke(() =>
                    {
                        lstTelemetry.Items.Add(consumeResult.Message.Value);
                    });

                    await Task.Delay(1000, token);
                }
            }
        }

        private void BtnStartTelemetry_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(ConsumeTelemetry, this._cts.Token);
        }

        private void BtnCancelTelemetry_Click(object sender, RoutedEventArgs e)
        {
            this._cts.Cancel();
        }
    }
}
