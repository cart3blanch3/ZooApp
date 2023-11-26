namespace ZooApp
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.IO;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml.Linq;

    // Перечисление для типа еды
    public enum FoodType
    {
        Meat,   // Мясо
        Grass,  // Трава
        Leaves, // Листва
        Fish,   // Рыба
        Fruits  // Фрукты
    }

    // Класс, представляющий абстрактное животное
    [Serializable]
    [XmlInclude(typeof(Lion))]
    [XmlInclude(typeof(Zebra))]
    [XmlInclude(typeof(Giraffe))]
    [XmlInclude(typeof(Hippopotamus))]
    [XmlInclude(typeof(Penguin))]
    [XmlInclude(typeof(Lemur))]
    [XmlInclude(typeof(Monkey))]
    [XmlInclude(typeof(Otter))]
    [XmlInclude(typeof(Tiger))]
    [XmlInclude(typeof(Jaguar))]
    public abstract class Animal
    {
        // Свойства, представляющие характеристики животного
        public string Name { get; set; }                // Имя
        public int Age { get; set; }                    // Возраст
        public string Species { get; set; }             // Вид
        public FoodType AnimalFoodType { get; set; }    // Тип пищи
        public int CurrentHunger { get; set; }          // Текущий уровень голода
        public int MaxHunger { get; set; }              // Максимальный уровень голода
        public int RequiredFoodAmount { get; set; }     // Необходимое количество пищи
        public Bowl AnimalBowl { get; set; }            // Миска

        // Задачи для асинхронных таймеров
        private Task hungerTask;
        private Task actionTask;

        // Конструктор для создания экземпляра животного
        protected Animal(string name, int age, string species, FoodType foodType, int maxHunger, int requiredFoodAmount)
        {
            // Инициализация характеристик животного
            Name = name;
            Age = age;
            Species = species;
            AnimalFoodType = foodType;
            MaxHunger = maxHunger;
            RequiredFoodAmount = requiredFoodAmount;
            CurrentHunger = MaxHunger;
            AnimalBowl = new Bowl(foodType, 100, name);

            // Запуск таймеров
            StartHungerTimer();
            StartActionTimer();
        }

        // Конструктор без параметров для сериализации
        public Animal()
        {

        }

        // Методы для запуска асинхронных таймеров
        private void StartActionTimer()
        {
            actionTask = Task.Run(async () =>
            {
                while (true)
                {
                    PerformRandomAction();
                    await Task.Delay(5000);
                }
            });
        }

        private void StartHungerTimer()
        {
            hungerTask = Task.Run(async () =>
            {
                while (true)
                {
                    DecreaseHunger();
                    await Task.Delay(1000);
                }
            });
        }

        // Метод для запуска обоих таймеров
        public void StartTimers()
        {
            StartHungerTimer();
            StartActionTimer();
        }

        // Метод для уменьшения уровня голода
        private void DecreaseHunger()
        {
            if (CurrentHunger > 0)
            {
                CurrentHunger--;

                if (CurrentHunger == 0)
                {
                    Logger.Log($"{Name} голоден(а) и начинает есть.");
                    Eat();
                }
            }
        }

        // Метод для выполнения случайного действия
        private void PerformRandomAction()
        {
            Random random = new Random();
            int chance = random.Next(0, 101); // Генерация случайного числа от 0 до 100

            // Шанс срабатывания действия - 10%
            if (chance <= 10)
            {
                int randomNumber = random.Next(0, 2);

                if (randomNumber == 0)
                {
                    MakeSound();
                }
                else
                {
                    Move();
                }
            }
        }

        // Абстрактные методы, которые должны быть реализованы в производных классах
        public abstract void MakeSound();

        public abstract void Move();

        // Метод для питания животного
        public void Eat()
        {
            // Животное ожидает, пока количество еды в миске не станет достаточным
            while (AnimalBowl.CurrentAmount < RequiredFoodAmount)
            {
                Logger.Log($"Животное {Name} ожидает еду в миске {AnimalBowl.OwnerName}.");

                // Можно добавить задержку, чтобы не блокировать поток полностью
                Task.Delay(5000).Wait();
            }

            // Животное успешно поело, уменьшаем количество еды в миске
            // и восстанавливаем уровень сытости до максимального значения
            Logger.Log($"{Name} поел(а) и теперь сыт(а).");
            AnimalBowl.CurrentAmount -= RequiredFoodAmount;
            CurrentHunger = MaxHunger;
        }


        // Переопределенный метод для строкового представления животного
        public override string ToString()
        {
            return $"Имя: {Name}, Возраст: {Age}, Вид: {Species}, Тип пищи: {AnimalFoodType}, Текущий уровень голода: {CurrentHunger}, Максимальный уровень голода: {MaxHunger}, Необходимое количество пищи: {RequiredFoodAmount}, Миска: {AnimalBowl}";
        }
    }

    // Класс, представляющий миску для еды животного
    [Serializable]
    public class Bowl
    {
        // Свойства миски
        public FoodType BowlFood { get; set; }   // Тип пищи в миске
        public int CurrentAmount { get; set; }   // Текущее количество еды в миске
        public int Capacity { get; set; }        // Вместимость миски
        public string OwnerName { get; set; }    // Имя владельца миски

        // Конструктор без параметров для сериализации
        public Bowl()
        {
        }

        // Конструктор для создания экземпляра миски
        public Bowl(FoodType foodType, int capacity, string ownerName)
        {
            BowlFood = foodType;
            CurrentAmount = 0;
            Capacity = capacity;
            OwnerName = ownerName;
        }

        // Метод для добавления еды в миску
        public void AddFood(int amount)
        {
            // Вычисляем оставшееся свободное место в миске
            int spaceLeft = Capacity - CurrentAmount;

            // Проверяем, достаточно ли места для добавления указанного количества еды
            if (amount <= spaceLeft)
            {
                // Добавляем еду в миску и логируем событие
                CurrentAmount += amount;
                Logger.Log($"Добавлено {amount} еды в миску {OwnerName}.");
            }
            else
            {
                // Логируем событие о том, что миска не может вместить столько еды
                Logger.Log($"Миска {OwnerName} не может вместить столько еды. Максимальная вместимость: {Capacity}.");
            }
        }
    }

    // Класс, представляющий льва
    public class Lion : Animal
    {
        // Конструктор без параметров для сериализации
        public Lion()
        {

        }

        // Конструктор для создания экземпляра льва
        public Lion(string name, int age) : base(name, age, "Лев", FoodType.Meat, 50, 10)
        {
        }

        // Реализация абстрактных методов
        public override void MakeSound()
        {
            Logger.Log($"{Name}: Ррр!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} идет по саванне.");
        }
    }

    // Класс представляющий зебру
    public class Zebra : Animal
    {
        public Zebra()
        {

        }

        public Zebra(string name, int age) : base(name, age, "Зебра", FoodType.Grass, 50, 10)
        {
        }

        public override void MakeSound()
        {
            Logger.Log($"{Name}: Иии-го-го!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} гуляет по лугу.");
        }
    }

    // Класс представляющий жираф
    public class Giraffe : Animal
    {
        public Giraffe()
        {

        }
        public Giraffe(string name, int age) : base(name, age, "Жираф", FoodType.Leaves, 45, 9)
        {
        }

        public override void MakeSound()
        {
            Logger.Log($"{Name}: Му-у-у!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} пасется в высоких деревьях.");
        }
    }

    // Класс представляющий бегемот
    public class Hippopotamus : Animal
    {
        public Hippopotamus()
        {

        }
        public Hippopotamus(string name, int age) : base(name, age, "Бегемот", FoodType.Grass, 60, 12)
        {
        }

        public override void MakeSound()
        {
            Logger.Log($"{Name}: Хрю-хрю!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} плавает в водоеме.");
        }
    }

    // Класс представляющий пингвина
    public class Penguin : Animal
    {
        public Penguin()
        {

        }
        public Penguin(string name, int age) : base(name, age, "Пингвин", FoodType.Fish, 30, 6)
        {
        }

        public override void MakeSound()
        {
            Logger.Log($"{Name}: Карр-карр!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} бегает по льду.");
        }
    }

    // Класс представляющий лемура
    public class Lemur : Animal
    {
        public Lemur()
        {

        }
        public Lemur(string name, int age) : base(name, age, "Лемур", FoodType.Fruits, 35, 7)
        {
        }

        public override void MakeSound()
        {
            Logger.Log($"{Name}: Угу-угу!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} лазает по деревьям.");
        }
    }

    // Класс представляющий обезьяну
    public class Monkey : Animal
    {
        public Monkey()
        {

        }
        public Monkey(string name, int age) : base(name, age, "Обезьяна", FoodType.Fruits, 40, 8)
        {
        }

        public override void MakeSound()
        {
            Logger.Log($"{Name}: Га-га-га!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} карабкается по веткам.");
        }
    }

    // Класс представляющий выдру
    public class Otter : Animal
    {
        public Otter()
        {

        }
        public Otter(string name, int age) : base(name, age, "Выдра", FoodType.Fish, 30, 6)
        {
        }

        public override void MakeSound()
        {
            Logger.Log($"{Name}: Чав-чав-чав!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} плавает в воде.");
        }
    }

    // Класс представляющий тигра
    public class Tiger : Animal
    {
        public Tiger()
        {

        }
        public Tiger(string name, int age) : base(name, age, "Тигр", FoodType.Meat, 55, 11)
        {
        }

        public override void MakeSound()
        {
            Logger.Log($"{Name}: Грр-грр-грр!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} бежит по джунглям.");
        }
    }

    // Класс представляющий ягуара
    public class Jaguar : Animal
    {
        public Jaguar()
        {

        }
        public Jaguar(string name, int age) : base(name, age, "Ягуар", FoodType.Meat, 50, 10)
        {
        }

        public override void MakeSound()
        {
            Logger.Log($"{Name}: Рр-рр-рр!");
        }

        public override void Move()
        {
            Logger.Log($"{Name} мчится сквозь джунгли.");
        }
    }

    // Обобщенный интерфейс для вольера
    public interface IEnclosure<out T> : IEnumerable<T> where T : Animal
    {
        string Name { get; }
        int Capacity { get; }
        int GetAvailableSpace();
    }

    [Serializable]
    [DataContract]
    public class Enclosure<T> : IEnclosure<T> where T : Animal
    {
        private List<T> animals;
        private readonly int capacity;

        public string Name { get; set; }

        // Конструктор без параметров для сериализации
        public Enclosure()
        {
            animals = new List<T>();
        }

        // Конструктор для создания вольера с заданным именем и вместимостью
        public Enclosure(string name, int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Вместимость должна быть больше 0", nameof(capacity));
            }

            Name = name;
            this.capacity = capacity;
            animals = new List<T>();
        }

        // Свойство для получения вместимости вольера
        public int Capacity => capacity;

        // Свойство для получения количества животных в вольере
        public int Count => animals.Count;

        // Свойство, указывающее, доступен ли вольер только для чтения
        public bool IsReadOnly => false;

        // Метод для добавления животного в вольер
        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Элемент не может быть пустым (null)");
            }

            // Проверка, есть ли еще место в вольере
            if (Count < capacity)
            {
                animals.Add(item);

                // Запуск таймеров для нового животного
                item.StartTimers();
            }
            else
            {
                throw new ZooException($"Вольер \"{Name}\" заполнен до максимальной вместимости ({capacity}). Невозможно добавить больше животных.");
            }
        }

        // Метод для очистки вольера от всех животных
        public void Clear()
        {
            animals.Clear();
        }

        // Метод для проверки наличия животного в вольере
        public bool Contains(T item)
        {
            return animals.Contains(item);
        }

        // Метод для копирования элементов вольера в массив
        public void CopyTo(T[] array, int arrayIndex)
        {
            animals.CopyTo(array, arrayIndex);
        }

        // Метод для получения перечислителя животных в вольере
        public IEnumerator<T> GetEnumerator()
        {
            return animals.GetEnumerator();
        }

        // Метод для удаления животного из вольера
        public bool Remove(T item)
        {
            return animals.Remove(item);
        }

        // Необобщенный метод для получения перечислителя
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Метод для получения доступного места в вольере
        public int GetAvailableSpace()
        {
            return capacity - Count;
        }

        // Метод для сравнения двух животных с использованием заданной функции сравнения
        public int Compare(Func<T, T, int> comparisonFunc, T animal1, T animal2)
        {
            return comparisonFunc(animal1, animal2);
        }

        // Асинхронный метод для сортировки животных в вольере с использованием заданной функции сортировки
        public async Task SortAnimalsAsync(Action<List<T>> sortAction)
        {
            Logger.Log($"Начинается асинхронная сортировка животных в вольере {Name}.");

            await Task.Run(() =>
            {
                sortAction(animals);
            });

            Logger.Log($"Асинхронная сортировка вольера {Name} завершена.");
        }

        // Переопределенный метод для представления вольера в виде строки
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Вольер {Name}: {Count} из {Capacity}");

            // Добавление информации о каждом животном в вольере
            foreach (var animal in animals)
            {
                sb.AppendLine(animal.ToString());
            }

            return sb.ToString();
        }
    }

    // Класс представляющий смотрителя
    public class Keeper
    {
        // Свойство для получения или установки имени смотрителя
        public string Name { get; set; }

        // Конструктор для создания смотрителя с заданным именем
        public Keeper(string name)
        {
            Name = name;
        }

        // Асинхронный метод для наполнения мисок в вольере
        public async Task FillBowlsAsync(IEnclosure<Animal> enclosure)
        {
            Logger.Log($"{Name} наполняет миски в вольере {enclosure.Name}.");

            // Имитация работы с задержкой
            await Task.Delay(3000);

            // Наполнение мисок каждого животного в вольере
            foreach (var animal in enclosure)
            {
                int foodToAdd = animal.AnimalBowl.Capacity - animal.AnimalBowl.CurrentAmount;
                animal.AnimalBowl.AddFood(foodToAdd);
            }
        }

        // Асинхронный метод для уборки вольера
        public async Task CleanEnclosureAsync(IEnclosure<Animal> enclosure)
        {
            Logger.Log($"{Name} убирает вольер {enclosure.Name}.");

            // Имитация работы с задержкой
            await Task.Delay(5000);
        }
    }

    // Класс представляющий зоопарк
    public class Zoo
    {
        // Список вольеров в зоопарке
        private List<IEnclosure<Animal>> enclosures;

        // Список смотрителей в зоопарке
        private List<Keeper> keepers;

        // Таймер для выполнения ежедневной рутины
        private Timer dailyRoutineTimer;

        // Свойство для получения или установки имени зоопарка
        public string Name { get; set; }

        // Конструктор для создания зоопарка с заданным именем
        public Zoo(string name)
        {
            Name = name;
            enclosures = new List<IEnclosure<Animal>>();
            keepers = new List<Keeper>();
            dailyRoutineTimer = new Timer(PerformDailyRoutineAsync, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        // Метод для получения списка вольеров в зоопарке
        public List<IEnclosure<Animal>> GetEnclosures()
        {
            return enclosures;
        }

        // Метод для добавления вольера в зоопарк
        public void AddEnclosure(IEnclosure<Animal> enclosure)
        {
            enclosures.Add(enclosure);
        }

        // Метод для удаления вольера из зоопарка
        public void RemoveEnclosure(IEnclosure<Animal> enclosure)
        {
            enclosures.Remove(enclosure);
        }

        // Метод для добавления смотрителя в зоопарк
        public void AddKeeper(Keeper keeper)
        {
            keepers.Add(keeper);
        }

        // Метод для удаления смотрителя из зоопарка
        public void RemoveKeeper(Keeper keeper)
        {
            keepers.Remove(keeper);
        }

        // Асинхронный метод для выполнения ежедневной рутины
        private async void PerformDailyRoutineAsync(object state)
        {
            Logger.Log($"Выполняется ежедневная рутина в зоопарке {Name}...");

            // Проверка наличия смотрителей
            if (keepers.Count == 0)
            {
                throw new ZooException("Нет работников в зоопарке.");
            }

            // Создание списка задач для выполнения работы в каждом вольере
            var tasks = new List<Task>();

            for (int i = 0; i < enclosures.Count; i++)
            {
                // Циклическое распределение вольеров между смотрителями
                var keeper = keepers[i % keepers.Count];
                var enclosure = enclosures[i];

                await Task.Delay(2000);

                // Добавление задач для наполнения мисок и уборки вольера
                tasks.Add(keeper.FillBowlsAsync(enclosure));
                tasks.Add(keeper.CleanEnclosureAsync(enclosure));
            }

            // Ожидание завершения всех задач перед завершением рутины
            await Task.WhenAll(tasks);

            Logger.Log("Ежедневная рутина завершена.");
        }
    }

    // Класс для логирования событий в приложении
    public class Logger
    {
        // Единственный экземпляр логгера для реализации синглтона
        private static readonly Logger instance = new Logger();

        // Закрытый конструктор, чтобы предотвратить создание других экземпляров
        private Logger()
        {
        }

        // Статический метод для доступа к единственному экземпляру логгера
        public static Logger Instance => instance;

        // Событие, возникающее при записи лога
        public event Action<string>? LogEvent;

        // Статический метод для записи лога
        public static void Log(string message)
        {
            // Форматирование сообщения с текущей датой и временем
            string formattedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";

            // Вызов события логгера
            instance.LogEvent?.Invoke(formattedMessage);
        }
    }

    // Класс для логирования в консоль и файл
    public class ConsoleFileLogger
    {
        // Объект для блокировки при записи в файл
        private static readonly object fileLock = new object();

        // Метод для логирования в консоль
        public static void ConsoleLogger(string message)
        {
            Console.WriteLine($"{message}");
        }

        // Метод для логирования в файл
        public static void FileLogger(string message)
        {
            try
            {
                // Использование блокировки для предотвращения одновременной записи несколькими потоками
                lock (fileLock)
                {
                    File.AppendAllText("log.txt", $"{message}" + Environment.NewLine);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException("Нет доступа к файлу при записи в лог.");
            }
        }
    }

    // Исключение, представляющее ошибку в зоопарке
    public class ZooException : Exception
    {
        public ZooException(string message) : base(message) { }
    }

    // Интерфейс для сериализации и десериализации
    public interface ISerializer<T>
    {
        void Serialize(T data, string filePath);
        T Deserialize(string filePath);
    }

    // Класс для сериализации и десериализации в формате XML
    public class XmlSerializer<T> : ISerializer<T>
    {
        // Метод для сериализации объекта в XML и сохранения в файл
        public void Serialize(T data, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, data);
            }
        }

        // Метод для десериализации объекта из XML файла
        public T Deserialize(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }

    // Класс для сериализации и десериализации в формате JSON
    public class JsonSerializer<T> : ISerializer<T>
    {
        // Метод для сериализации объекта в JSON и сохранения в файл
        public void Serialize(T data, string filePath)
        {
            // Преобразование объекта в JSON
            string json = JsonConvert.SerializeObject(data);
            // Запись JSON в файл
            File.WriteAllText(filePath, json);
        }

        // Метод для десериализации объекта из JSON файла
        public T Deserialize(string filePath)
        {
            // Чтение JSON из файла
            string json = File.ReadAllText(filePath);
            // Преобразование JSON в объект
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    // Класс для хранения модели данных зоопарка при сериализации
    [Serializable]
    public class ZooSerializationModel
    {
        // Общий вольер для всех животных
        public Enclosure<Animal> AllAnimalsEnclosure { get; set; }

        // Конструктор для инициализации общего вольера
        public ZooSerializationModel()
        {
            AllAnimalsEnclosure = new Enclosure<Animal>("Общий вольер", int.MaxValue);
        }
    }


    class Program
    {
        private static async Task Main()
        {
            try
            {
                // Подписка на событие LogEvent экземпляра Logger для вывода логов в консоль
                Logger.Instance.LogEvent += ConsoleFileLogger.ConsoleLogger;

                // Подписка на событие LogEvent экземпляра Logger для записи логов в файл
                Logger.Instance.LogEvent += ConsoleFileLogger.FileLogger;

                // Создаем вольеры для разных типов животных
                Enclosure<Lion> lionEnclosure = new Enclosure<Lion>("Львиный вольер", 5);
                Enclosure<Zebra> zebraEnclosure = new Enclosure<Zebra>("Вольер зебр", 8);
                Enclosure<Giraffe> giraffeEnclosure = new Enclosure<Giraffe>("Вольер жирафов", 10);
                Enclosure<Hippopotamus> hippoEnclosure = new Enclosure<Hippopotamus>("Вольер бегемотов", 6);
                Enclosure<Penguin> penguinEnclosure = new Enclosure<Penguin>("Вольер пингвинов", 4);
                Enclosure<Lemur> lemurEnclosure = new Enclosure<Lemur>("Вольер лемуров", 7);
                Enclosure<Monkey> monkeyEnclosure = new Enclosure<Monkey>("Вольер обезьян", 5);
                Enclosure<Otter> otterEnclosure = new Enclosure<Otter>("Вольер выдр", 3);
                Enclosure<Tiger> tigerEnclosure = new Enclosure<Tiger>("Вольер тигров", 4);
                Enclosure<Jaguar> jaguarEnclosure = new Enclosure<Jaguar>("Вольер ягуаров", 4);

                // Create an instance of the XML serializer for ZooSerializationModel
                var xmlSerializer = new XmlSerializer<ZooSerializationModel>();

                // Deserialize the zoo data from the XML file
                ZooSerializationModel zooModel = xmlSerializer.Deserialize("zoopark.xml");

                foreach (var animal in zooModel.AllAnimalsEnclosure)
                {
                    if (animal is Lion lion)
                    {
                        lionEnclosure.Add(lion);
                    }
                    else if (animal is Zebra zebra)
                    {
                        zebraEnclosure.Add(zebra);
                    }
                    else if (animal is Giraffe giraffe)
                    {
                        giraffeEnclosure.Add(giraffe);
                    }
                    else if (animal is Hippopotamus hippo)
                    {
                        hippoEnclosure.Add(hippo);
                    }
                    else if (animal is Penguin penguin)
                    {
                        penguinEnclosure.Add(penguin);
                    }
                    else if (animal is Lemur lemur)
                    {
                        lemurEnclosure.Add(lemur);
                    }
                    else if (animal is Monkey monkey)
                    {
                        monkeyEnclosure.Add(monkey);
                    }
                    else if (animal is Otter otter)
                    {
                        otterEnclosure.Add(otter);
                    }
                    else if (animal is Tiger tiger)
                    {
                        tigerEnclosure.Add(tiger);
                    }
                    else if (animal is Jaguar jaguar)
                    {
                        jaguarEnclosure.Add(jaguar);
                    }
                }

                Zoo zoo = new Zoo("Зоопарк");

                zoo.AddEnclosure(lionEnclosure);
                zoo.AddEnclosure(zebraEnclosure);
                zoo.AddEnclosure(giraffeEnclosure);
                zoo.AddEnclosure(hippoEnclosure);
                zoo.AddEnclosure(penguinEnclosure);
                zoo.AddEnclosure(lemurEnclosure);
                zoo.AddEnclosure(monkeyEnclosure);
                zoo.AddEnclosure(otterEnclosure);
                zoo.AddEnclosure(tigerEnclosure);
                zoo.AddEnclosure(jaguarEnclosure);

                zoo.AddKeeper(new Keeper("Джон"));
                zoo.AddKeeper(new Keeper("Алиса"));

                Console.ReadKey();

                // Создаем общий вольер
                Enclosure<Animal> allAnimalsEnclosure = new Enclosure<Animal>("Общий вольер", int.MaxValue);

                // Добавляем всех животных из других вольеров в общий вольер
                foreach (var enclosure in zoo.GetEnclosures())
                {
                    foreach (var animal in enclosure)
                    {
                        allAnimalsEnclosure.Add(animal);
                    }
                }

                // Асинхронно сортируем животных в общем вольере
                await allAnimalsEnclosure.SortAnimalsAsync(animals =>
                {
                    // Ваша логика сортировки, например, сортировка по имени
                    animals.Sort((a1, a2) => a1.Name.CompareTo(a2.Name));
                });

                // Создаем экземпляр класса для сериализации
                ZooSerializationModel zooSerializationModel = new ZooSerializationModel
                {
                    AllAnimalsEnclosure = allAnimalsEnclosure
                };

                // Создаем экземпляр сериализатора XML
                var xmlSerializerAllAnimals = new XmlSerializer<ZooSerializationModel>();

                // Сериализуем в XML
                xmlSerializerAllAnimals.Serialize(zooSerializationModel, "zoo_all_animals.xml");

                // Создаем экземпляр сериализатора JSON
                var jsonSerializer = new JsonSerializer<ZooSerializationModel>();

                // Сериализуем в JSON
                jsonSerializer.Serialize(zooSerializationModel, "zoo_all_animals.json");
            }
            catch (ZooException ex)
            {
                // Обработка и логгирование пользовательских исключений
                Logger.Log($"Произошло пользовательское исключение:{ex.Message}");
            }
            catch (Exception ex)
            {
                // Обработка и логгирование системных исключений
                Logger.Log($"Произошло системное исключение: {ex.Message}");
            }
        }
    }
}