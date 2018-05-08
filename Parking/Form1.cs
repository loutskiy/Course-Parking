using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Parking
{
    
    public partial class Form1 : Form
    {
        private CameraModel cameraModel;
        private Parking parking = new Parking();
        public Form1()
        {
            InitializeComponent();
            cameraModel = new CameraModel();
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "MM/dd/yyyy hh:mm:ss";
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
           {
                using (StreamReader reader = new StreamReader(textBox6.Text, Encoding.Default))
                {
                string[] array = reader.ReadLine().Split('|');
                    if (array.Count() == 3)
                    {
                   // Console.WriteLine((reader.ReadLine().Split('|')));
                CameraModel model = new CameraModel(array);
                        parking.addTranscation(model);
                        updateDataGridView1();
                    } else
                    {
                        MessageBox.Show("Неправильная структура файла");
                    }
                }
            } catch
            {
               MessageBox.Show("Файл не найден");
            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (gosNumberField.Text != "")
            {
                string[] args = new string[3];
                args[0] = gosNumberField.Text;
                args[1] = (directionList.SelectedIndex == 0) ? "+" : "-";
                args[2] = dateTimePicker1.Text;
                CameraModel model = new CameraModel(args);
                parking.addTranscation(model);
                updateDataGridView1();
            } else
            {
                MessageBox.Show("Заполните госномер");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "")
            {
                Client client = new Client();
                client.carNumber = textBox1.Text;
                client.clientFullName = textBox2.Text;
                client.clientPhone = textBox3.Text;
                parking.addClient(client);
                this.dataGridView3.Rows.Clear();
                foreach (Client model in parking.getClients())
                {
                    string[] row = new string[] { model.carNumber, model.clientFullName, model.clientPhone };
                    this.dataGridView3.Rows.Add(row);
                }
            } else
            {
                MessageBox.Show("Заполните госномер");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != "" && textBox5.Text != "")
            {
                parking.addDeposit(Convert.ToDouble(textBox5.Text), textBox4.Text);
                MessageBox.Show("Платеж добавлен");
                this.dataGridView2.Rows.Clear();
                foreach (Payment model in parking.getPayments())
                {
                    string[] row = new string[] { model.carNumber, Convert.ToString(model.deposit) };
                    this.dataGridView2.Rows.Add(row);
                }
            } else
            {
                MessageBox.Show("Заполните госномер");
            }
        }

        private void updateDataGridView1 ()
        {
            this.dataGridView1.Rows.Clear();
            foreach(CameraModel model in parking.transactions())
            {
                Client client = parking.searchClientInDB(model.carNumber);
                string[] row = new string[] { model.carNumber, model.direction, model.date, client.clientFullName, client.clientPhone };
                this.dataGridView1.Rows.Add(row);
            }
        }
    }

    struct CameraModel
    {
        public string carNumber;
        public string direction;
        public string date;

        public CameraModel(string[] args)
        {
            carNumber = args[0];
            direction = args[1];
            date = args[2];
        }
    }

    struct Client
    {
        public string carNumber;
        public string clientFullName;
        public string clientPhone;
    }

    struct Payment
    {
        public string carNumber;
        public double deposit;
    }

    class Parking
    {
        private List<CameraModel> data = new List<CameraModel>();
        private int countParkingPlace = 100;
        private double pricePerHour = 100.0;
        private List<Client> clients = new List<Client>();
        private List<Payment> payments = new List<Payment>();

        public void addTranscation (CameraModel transaction)
        {
            if ((data.Count + clients.Count) < countParkingPlace)
            {
                if (isExistClient(transaction.carNumber))
                {
                    showDecisionInfo(searchTransaction(transaction));
                } else
                {
                    switch (transaction.direction)
                    {
                        case "+":
                            showDecisionInfo(searchTransaction(transaction));
                            break;
                        case "-":
                            checkPayment(transaction);
                            break;
                        default:
                            MessageBox.Show("Направление не распознано.");
                            break;
                    }
                }
            } else
            {
                MessageBox.Show("нет свободных парковочных мест");
            }
        }

        private void checkPayment (CameraModel transaction)
        {
            foreach (CameraModel model in data)
            {
                if (model.carNumber == transaction.carNumber)
                {
                    TimeSpan span = Convert.ToDateTime(transaction.date) - Convert.ToDateTime(model.date);
                    double sum = (span.Hours + ((span.Minutes > 0) ? 1 : 0)) * pricePerHour;
                    double fullDeposit = getFullDeposit(transaction.carNumber);
                    if (sum <= fullDeposit)
                    {
                        showDecisionInfo(searchTransaction(transaction));
                    } else
                    {
                        MessageBox.Show("Красный. Неоплачена парковка");
                    }
                    return;
                }
            }
            MessageBox.Show("Не найден въезд этой машины");
        }

        public List<CameraModel> transactions ()
        {
            return data;
        }

        public List<Client> getClients()
        {
            return clients;
        }

        public List<Payment> getPayments()
        {
            return payments;
        }

        public void addClient (Client client)
        {
            if ((data.Count + clients.Count) < countParkingPlace)
            {
                if (!clients.Contains(client))
                {
                    this.clients.Add(client);
                    MessageBox.Show("Клиент успешно добавлен!");
                } else
                {
                    MessageBox.Show("Клиент существует!");
                }
            } else
            {
                MessageBox.Show("Невозможно добавить клиента: паркова заполнена.");
            }
        }

        public Client searchClientInDB (string carNumber)
        {
            foreach (Client client in clients)
            {
                if (client.carNumber == carNumber)
                    return client;
            }
            return new Client();
        }

        private bool isExistClient (string carNumber)
        {
            foreach (Client client in clients)
            {
                if (client.carNumber == carNumber)
                    return true;
            }
            return false;
        }

        public bool searchTransaction(CameraModel transaction)
        {
            foreach (CameraModel objectModel in data) {
                if (objectModel.carNumber == transaction.carNumber && objectModel.direction == transaction.direction)
                {
                    return false;
                }
            }
            this.data.Add(transaction);
            return true;
        }

        private void showDecisionInfo (bool scoring)
        {
            if (scoring)
            {
                MessageBox.Show("Зеленый. Проезд разрешен.");
            } else
            {
                MessageBox.Show("Красный. Проезд запрещен");
            }
        }

        public void addDeposit (double sum, string carNumber)
        {
            Payment payment = new Payment();
            payment.carNumber = carNumber;
            payment.deposit = sum;
            payments.Add(payment);
        }

        private double getFullDeposit (string carNumber)
        {
            double sum = 0.0;
            foreach (Payment payment in payments)
            {
                if (payment.carNumber == carNumber)
                {
                    sum += payment.deposit;
                }
            }
            return sum;
        }

    }
}
