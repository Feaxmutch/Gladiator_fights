namespace GladiatorFights
{
    internal class Program
    {
        static void Main()
        {
            List<Fighter> fighters = new()
            {
                new Knight(0.5f),
                new Spearman(),
                new Viking(1.1f, 50),
                new Magican(100, 15, 11f),
                new Thief(0.45f),
            };

            Colosseum colosseum = new(fighters);
            colosseum.ShowMenu();
        }
    }

    public static class Utilits
    {
        private static Random s_random = new();

        public static bool GetBoolean(float trueChance)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(trueChance);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(trueChance, 1);
            return s_random.NextDouble() < trueChance;
        }

        public static bool TryRequestNumber(string mesage, out int number)
        {
            Console.WriteLine(mesage);
            string userInput = Console.ReadLine();

            if (userInput == string.Empty)
            {
                Console.WriteLine("Вы ничего не ввели");
            }

            bool isParsed = int.TryParse(userInput, out number);

            if (isParsed == false)
            {
                Console.WriteLine($"Не получилось конвертировать {userInput} в число.");
                Console.WriteLine($"Возможно есть лишние символы");
                Console.ReadKey();
            }

            return isParsed;
        }
    }

    public class Colosseum
    {
        private readonly IReadOnlyList<Fighter> _fighters;

        public Colosseum(List<Fighter> fighters)
        {
            _fighters = fighters;
        }

        public void ShowMenu()
        {
            const string CommandFight = "1";
            const string CommandExit = "2";

            bool isActive = true;

            while (isActive)
            {
                Console.Clear();
                ShowWelcomeMessage();
                Console.WriteLine($"\n{CommandFight}) Посмотреть бой");
                Console.WriteLine($"{CommandExit}) Выйти из колизея");
                string userCommand = Console.ReadLine();

                switch (userCommand)
                {
                    case CommandFight:
                        ExecuteFight(SelectFighter(), SelectFighter());
                        break;

                    case CommandExit:
                        isActive = false;
                        break;
                }
            }
        }

        private void ExecuteFight(Fighter fighter1, Fighter fighter2)
        {
            bool isFighting = true;
            Fighter winer = null;

            Console.Write($"Сейчас {fighter1.Name} и {fighter2.Name} сойдутся в бою");
            Console.ReadKey();

            while (isFighting)
            {
                Console.Clear();

                Console.WriteLine("Статус бойцов:");
                Console.WriteLine($"\n{fighter1.Name}");
                fighter1.ShowStatus();
                Console.WriteLine($"\n{fighter2.Name}");
                fighter2.ShowStatus();

                Console.WriteLine();
                fighter1.Attack(fighter2);

                if (fighter2.IsDead)
                {
                    isFighting = false;
                    winer = fighter1;
                    continue;
                }

                Console.ReadKey();
                fighter2.Attack(fighter1);

                if (fighter1.IsDead)
                {
                    isFighting = false;
                    winer = fighter2;
                    continue;
                }

                Console.ReadKey();
            }

            ShowWinMessage(winer);
            Console.ReadKey();
        }

        private void ShowWelcomeMessage()
        {
            Console.WriteLine("Добро пожаловать в колизей");
        }

        private void ShowWinMessage(Fighter winer)
        {
            Console.WriteLine($"{winer.Name} победил");
        }

        private Fighter SelectFighter()
        {
            bool isSelecting = true;
            int fighterIndex = default;

            while (isSelecting)
            {
                Console.Clear();
                Console.WriteLine($"Выберите бойца:");

                for (int i = 0; i < _fighters.Count; i++)
                {
                    Console.WriteLine($"{i}) {_fighters[i].Name}");
                }

                while (Utilits.TryRequestNumber("Введите номер", out fighterIndex) == false) { }

                if (fighterIndex >= 0 && fighterIndex < _fighters.Count)
                {
                    isSelecting = false;
                    return _fighters[fighterIndex].Clone;
                }
            }

            return null;
        }
    }

    public abstract class Fighter : IDamageble
    {
        private readonly int _maxHealth;
        private int _health;

        protected Fighter(int health, int damage, string name)
        {
            _maxHealth = health;
            _health = _maxHealth;
            Damage = damage;
            Name = name;
        }

        public string Name { get; private set; }

        public bool IsDead => _health <= 0;

        public int Damage { get; }

        public abstract Fighter Clone { get; }

        public virtual void ShowStatus()
        {
            Console.WriteLine($"Здоровье: {_health}");
        }

        public abstract void Attack(IDamageble target);

        public virtual void TakeDamage(int damage)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(damage);
            _health -= damage;
            ShowDamageMassage(damage);
        }

        protected void Heal(int healValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(healValue);
            _health = Math.Min(_health + healValue, _maxHealth);
        }

        private void ShowDamageMassage(int damage)
        {
            Console.WriteLine($"{Name} получил урон в размере {damage}");
        }
    }

    public class Knight : Fighter
    {
        private readonly float _strongAttackMultiplyer = 2f;
        private readonly float _strongAttackChance;

        public override Fighter Clone => new Knight(_strongAttackChance);

        public Knight(float strongAttackChance) : base(1000, 35, "Рыцарь")
        {
            ArgumentOutOfRangeException.ThrowIfNegative(strongAttackChance);
            _strongAttackChance = strongAttackChance;
        }

        public override void Attack(IDamageble target)
        {
            bool isDobleDamage = Utilits.GetBoolean(_strongAttackChance);
            int damage = Damage;
            Console.Write($"{Name} атаковал противника ");

            if (isDobleDamage)
            {
                damage = (int)(damage * _strongAttackMultiplyer);
                Console.Write($"сильной атакой");
            }

            Console.WriteLine();
            target.TakeDamage(damage);
        }
    }

    public class Spearman : Fighter
    {
        private int _attacksCout = default;
        private int _attacksForDobleAttack = 3;

        public Spearman() : base(1000, 45, "Копейщик")
        {

        }

        public override Fighter Clone => new Spearman();

        public override void Attack(IDamageble target)
        {
            _attacksCout++;
            int damage = Damage;
            Console.WriteLine($"{Name} атаковал противника");
            target.TakeDamage(damage);

            if (_attacksCout == _attacksForDobleAttack)
            {
                target.TakeDamage(damage);
                _attacksCout = default;
            }
        }
    }

    public class Viking : Fighter
    {
        private readonly float _maxRage = 100f;
        private readonly float _rageMultiplyer;
        private readonly int _healValue;
        private float _rage;

        public Viking(float rageMultiplyer, int healValue) : base(1000, 30, "Викинг")
        {
            _rageMultiplyer = rageMultiplyer;
            _healValue = healValue;
        }

        public override Fighter Clone => new Viking(_rageMultiplyer, _healValue);

        public override void Attack(IDamageble target)
        {
            int damage = Damage;
            Console.WriteLine($"{Name} атаковал противника ");
            target.TakeDamage(damage);
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            _rage += damage * _rageMultiplyer;

            if (_rage >= _maxRage && IsDead == false)
            {
                Heal(_healValue);
            }
        }
    }

    public class Magican : Fighter
    {
        private readonly int _magicAttackCoast;
        private readonly float _magicAttackMultiplyer;
        private int _mana;

        public Magican(int mana, int magicAttackCoast, float magicAttackMultiplyer) : base(1000, 10, "Маг")
        {
            ArgumentOutOfRangeException.ThrowIfNegative(magicAttackMultiplyer);
            ArgumentOutOfRangeException.ThrowIfNegative(magicAttackCoast);
            ArgumentOutOfRangeException.ThrowIfNegative(mana);

            _mana = mana;
            _magicAttackCoast = magicAttackCoast;
            _magicAttackMultiplyer = magicAttackMultiplyer;
        }

        public override Fighter Clone => new Magican(_mana, _magicAttackCoast, _magicAttackMultiplyer);

        public override void ShowStatus()
        {
            base.ShowStatus();
            Console.WriteLine($"Мана: {_mana}");
        }

        public override void Attack(IDamageble target)
        {
            int damage = Damage;
            Console.Write($"{Name} атаковал противника ");

            if (_mana >= _magicAttackCoast)
            {
                _mana -= _magicAttackCoast;
                damage = (int)(damage * _magicAttackMultiplyer);
                Console.Write($"магией");
            }

            Console.WriteLine();
            target.TakeDamage(damage);
        }
    }

    public class Thief : Fighter
    {
        private readonly float _dogeChance;

        public Thief(float dogeChance) : base(1000, 45, "Вор")
        {
            _dogeChance = dogeChance;
        }

        public override Fighter Clone => new Thief(_dogeChance);

        public override void Attack(IDamageble target)
        {
            int damage = Damage;
            Console.WriteLine($"{Name} атаковал противника");
            target.TakeDamage(damage);
        }

        public override void TakeDamage(int damage)
        {
            bool isDoge = Utilits.GetBoolean(_dogeChance);

            if (isDoge)
            {
                Console.WriteLine($"{Name} уклонился");
            }
            else
            {
                base.TakeDamage(damage);
            }
        }
    }

    public interface IDamageble
    {
        void TakeDamage(int damage);
    }
}
