using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static ConsoleApp5._1.Program;

namespace ConsoleApp5._1
{
        internal class Program
        {
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
            public DateTime CallDate { get; set; }
            public double RatePerMinute { get; set; }
            public double ConversationDuration { get; set; }
            public double PreferentialDiscount { get; set; }
            public SubscriberType TypeOfSubscriber { get; set; }

            public enum SubscriberType
            {
                Regular,
                VIP,
                Corporate
            }

            public double GetTotalCost()
            {
                double cost = RatePerMinute * ConversationDuration;
                return cost - (cost * PreferentialDiscount / 100);
            }

            public override string ToString()
            {
                return $"{FullName};{PhoneNumber};{CallDate:yyyy-MM-dd};{RatePerMinute};{ConversationDuration};{PreferentialDiscount};{TypeOfSubscriber}";
            }

            public static Program FromString(string data)
            {
                var parts = data.Split(';');
                return new Program
                {
                    FullName = parts[0],
                    PhoneNumber = parts[1],
                    CallDate = DateTime.Parse(parts[2]),
                    RatePerMinute = double.Parse(parts[3]),
                    ConversationDuration = double.Parse(parts[4]),
                    PreferentialDiscount = double.Parse(parts[5]),
                    TypeOfSubscriber = (SubscriberType)Enum.Parse(typeof(SubscriberType), parts[6]),
                };
            }

            static void Main(string[] args)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                string filePath = "recording.txt";

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("1. Додати інформацію про платіж");
                    Console.WriteLine("2. Показати всі записи");
                    Console.WriteLine("3. Пошук по прізвищу");
                    Console.WriteLine("4. Пошук по номеру телефону");
                    Console.WriteLine("5. Пошук по даті");
                    Console.WriteLine("0. Вихід");
                    Console.Write("Оберіть опцію: ");
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            AddPayment(filePath);
                            break;
                        case "2":
                            DisplayAllPayments(filePath);
                            break;
                        case "3":
                            SearchByLastName(filePath);
                            break;
                        case "4":
                            SearchByPhoneNumber(filePath);
                            break;
                        case "5":
                            SearchByDate(filePath);
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Невірний вибір, спробуйте ще раз.");
                            break;
                    }

                    Console.WriteLine("\nНатисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();
                }
            }

            static void AddPayment(string filePath)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.WriteLine("Введіть інформацію про платіж:");

                Console.Write("П.І.Б: ");
                string fullName = Console.ReadLine();

                Console.Write("Номер телефону: ");
                string phoneNumber = Console.ReadLine();

                Console.Write("Дата здійснення розмови (yyyy-MM-dd): ");
                DateTime callDate = DateTime.Parse(Console.ReadLine());

                Console.Write("Тариф за хвилину розмови: ");
                double ratePerMinute = double.Parse(Console.ReadLine());

                Console.Write("Тривалість розмови (хв): ");
                double duration = double.Parse(Console.ReadLine());

                Console.Write("Пільгова знижка (%): ");
                double discount = double.Parse(Console.ReadLine());

            SubscriberType subscriberType;
            while (true)
            {
                Console.WriteLine("Тип абонента (Regular, VIP, Corporate): ");
                string typeInput = Console.ReadLine();

                if (Enum.TryParse(typeInput, true, out subscriberType) && Enum.IsDefined(typeof(SubscriberType), subscriberType))
                {
                    break;
                }
                Console.WriteLine("Невірний тип абонента. Будь ласка, введіть один із: Regular, VIP, Corporate.");
            }
           

            var payment = new Program
                {
                    FullName = fullName,
                    PhoneNumber = phoneNumber,
                    CallDate = callDate,
                    RatePerMinute = ratePerMinute,
                    ConversationDuration = duration,
                    PreferentialDiscount = discount,
                    TypeOfSubscriber = subscriberType
                };

                using (FileStream stream = new FileStream(filePath, FileMode.Append))
                {
                    byte[] data = System.Text.Encoding.Default.GetBytes(payment.ToString() + Environment.NewLine);
                    stream.Write(data, 0, data.Length);
                }

                Console.WriteLine("Запис успішно додано.");
            }

            static List<Program> ReadPaymentsFromFile(string filePath)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                var payments = new List<Program>();

                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);

                    string fileContent = System.Text.Encoding.Default.GetString(data);
                    var lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines)
                    {
                        payments.Add(FromString(line));
                    }
                }

                return payments;
            }

            static void DisplayAllPayments(string filePath)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                var payments = ReadPaymentsFromFile(filePath);

                if (payments.Count == 0)
                {
                    Console.WriteLine("Записи відсутні.");
                    return;
                }

                foreach (var payment in payments)
                {
                    Console.WriteLine(payment);
                    Console.WriteLine($"Повна вартість: {payment.GetTotalCost():F2} грн");
                    Console.WriteLine("--------------------------");
                }
            }

            static void SearchByLastName(string filePath)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.Write("Введіть прізвище для пошуку: ");
                string lastName = Console.ReadLine();

                var payments = ReadPaymentsFromFile(filePath)
                    .Where(p => p.FullName.Split(' ')[0].Equals(lastName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                DisplaySearchResults(payments);
            }

            static void SearchByPhoneNumber(string filePath)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.Write("Введіть номер телефону для пошуку: ");
                string phoneNumber = Console.ReadLine();

                var payments = ReadPaymentsFromFile(filePath)
                    .Where(p => p.PhoneNumber.Equals(phoneNumber, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                DisplaySearchResults(payments);
            }

            static void SearchByDate(string filePath)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.Write("Введіть дату для пошуку (yyyy-MM-dd): ");
                DateTime date = DateTime.Parse(Console.ReadLine());

                var payments = ReadPaymentsFromFile(filePath)
                    .Where(p => p.CallDate.Date == date.Date)
                    .ToList();

                DisplaySearchResults(payments);
            }

            static void DisplaySearchResults(List<Program> payments)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                if (payments.Count == 0)
                {
                    Console.WriteLine("Записи не знайдено.");
                    return;
                }

                foreach (var payment in payments)
                {
                    Console.WriteLine(payment);
                    Console.WriteLine($"Повна вартість: {payment.GetTotalCost():F2} грн");
                    Console.WriteLine("--------------------------");
                }
            }
        }
}
