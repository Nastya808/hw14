using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;


namespace hw14
{
    class CreditCard
    {
        public string CardNumber { get; }
        public string CardHolder { get; }
        public DateTime ExpiryDate { get; }
        public string PIN { get; private set; }
        public double CreditLimit { get; }
        public double Balance { get; private set; }

        public event EventHandler<double> AccountReplenished;
        public event EventHandler<double> MoneySpent;
        public event EventHandler CreditUsageStarted;
        public event EventHandler CreditLimitReached;
        public event EventHandler PINChanged;

        public CreditCard(string cardNumber, string cardHolder, DateTime expiryDate, string pin, double creditLimit)
        {
            CardNumber = cardNumber;
            CardHolder = cardHolder;
            ExpiryDate = expiryDate;
            PIN = pin;
            CreditLimit = creditLimit;
            Balance = 0;
        }

        public void Deposit(double amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Сума повинна бути більше нуля.");
                return;
            }

            Balance += amount;
            AccountReplenished?.Invoke(this, amount);
        }

        public void Spend(double amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Сума повинна бути більше нуля.");
                return;
            }

            if (Balance >= amount)
            {
                Balance -= amount;
                MoneySpent?.Invoke(this, amount);
            }
            else if (Balance + (CreditLimit - Balance) >= amount)
            {
                Balance = CreditLimit;
                CreditUsageStarted?.Invoke(this);
                Balance -= amount;
                MoneySpent?.Invoke(this, amount);
            }
            else
            {
                Console.WriteLine("Не вистачає коштів для операції.");
            }

            if (Balance == CreditLimit)
            {
                CreditLimitReached?.Invoke(this);
            }
        }

        public void ChangePIN(string newPIN)
        {
            PIN = newPIN;
            PINChanged?.Invoke(this);
        }

        public void SaveToFile(string filename)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("Номер картки: " + CardNumber);
                writer.WriteLine("ПІБ власника: " + CardHolder);
                writer.WriteLine("Термін дії картки: " + ExpiryDate);
                writer.WriteLine("PIN: " + PIN);
                writer.WriteLine("Кредитний ліміт: " + CreditLimit);
                writer.WriteLine("Сума грошей: " + Balance);
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Введіть номер картки:");
            string cardNumber = Console.ReadLine();

            Console.WriteLine("Введіть ПІБ власника:");
            string cardHolder = Console.ReadLine();

            Console.WriteLine("Введіть термін дії картки (рік-місяць-день):");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime expiryDate) == false)
            {
                Console.WriteLine("Некоректний формат дати.");
                return;
            }

            Console.WriteLine("Введіть PIN:");
            string pin = Console.ReadLine();

            Console.WriteLine("Введіть кредитний ліміт:");
            if (double.TryParse(Console.ReadLine(), out double creditLimit) == false)
            {
                Console.WriteLine("Некоректний формат ліміту.");
                return;
            }

            CreditCard card = new CreditCard(cardNumber, cardHolder, expiryDate, pin, creditLimit);

            card.AccountReplenished += (sender, amount) => Console.WriteLine($"Рахунок поповнено на {amount} грн.");
            card.MoneySpent += (sender, amount) => Console.WriteLine($"Сплачено {amount} грн.");
            card.CreditUsageStarted += (sender, e) => Console.WriteLine("Розпочато використання кредитних коштів.");
            card.CreditLimitReached += (sender, e) => Console.WriteLine("Досягнуто кредитний ліміт.");
            card.PINChanged += (sender, e) => Console.WriteLine("PIN був змінений.");

            Console.WriteLine("Введіть суму для поповнення рахунку:");
            if (double.TryParse(Console.ReadLine(), out double depositAmount))
            {
                card.Deposit(depositAmount);
            }
            else
            {
                Console.WriteLine("Некоректна сума.");
                return;
            }

            Console.WriteLine("Введіть суму для витрати:");
            if (double.TryParse(Console.ReadLine(), out double spendAmount))
            {
                card.Spend(spendAmount);
            }
            else
            {
                Console.WriteLine("Некоректна сума.");
            }

            card.ChangePIN("4321");

            card.SaveToFile("CreditCardInfo.txt");

            Console.ReadLine();
        }
    }
}