namespace GladiatorFights
{
    internal class Program
    {
        static void Main()
        {
            Colosseum colosseum = new();
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
    }

    public class Colosseum
    {
        public void ShowMenu()
        {
            const string CommandFight = "1";
            const string CommandExit = "2";

            bool menuIsActive = true;

            while (menuIsActive)
            {
                Console.Clear();
                ShowWelcomeMessage();
                Console.WriteLine($"{CommandFight}) Посмотреть бой");
                Console.WriteLine($"{CommandExit}) Выйти из колизея");
                string userCommand = Console.ReadLine();

                switch (userCommand)
                {
                    case CommandFight:
                        StartFight(SelectFighter(), SelectFighter());
                        break;

                    case CommandExit:
                        menuIsActive = false;
                        break;
                }
            }
        }

        private void StartFight(Fighter fighter1, Fighter fighter2)
        {
            ConsoleColor defaultColor = Console.ForegroundColor;
            bool isFighting = true;
            Fighter winer = null;

            Console.Write($"Сейчас {fighter1.Name} и {fighter2.Name} сойдутся в бою");
            Console.ReadKey();

            while (isFighting)
            {
                Console.Clear();
                Console.WriteLine("Здоровье:");
                Console.WriteLine($"{fighter1.Name} {fighter1.Health}");
                Console.WriteLine($"{fighter2.Name} {fighter2.Health}\n");

                fighter1.Attack(fighter2);

                if (fighter2.IsDead)
                {
                    isFighting = false;
                    winer = fighter1;
                    break;
                }

                Console.ReadKey();
                fighter2.Attack(fighter1);

                if (fighter1.IsDead)
                {
                    isFighting = false;
                    winer = fighter2;
                    break;
                }

                Console.ReadKey();
            }

            Console.WriteLine($"{winer.Name} победил");
            Console.ReadKey();
        }

        private void ShowWelcomeMessage()
        {
            Console.WriteLine("Добро пожаловать в колизей");
        }

        private Fighter SelectFighter()
        {
            const string CommandKnight = "1";
            const string CommandSpearman = "2";
            const string CommandViking = "3";
            const string CommandMagican = "4";
            const string CommandThief = "5";

            bool isSelecting = true;

            while (isSelecting)
            {
                Console.Clear();
                Console.WriteLine($"Выберите бойца:\n" +
                              $"{CommandKnight}) Рыцарь\n" +
                              $"{CommandSpearman}) Копейщик\n" +
                              $"{CommandViking}) Викинг\n" +
                              $"{CommandMagican}) Маг\n" +
                              $"{CommandThief}) Щитовик");

                switch (Console.ReadLine())
                {
                    case CommandKnight:
                        return new Knight(0.5f);

                    case CommandSpearman:
                        return new Spearman();

                    case CommandViking:
                        return new Viking(0.7f, 50);

                    case CommandMagican:
                        return new Magican();

                    case CommandThief:
                        return new Thief(0.45f);
                }
            }

            return null;
        }
    }

    public abstract class Fighter : IDamageble
    {
        private readonly int _maxHealth;

        protected Fighter(int health, int damage, string name)
        {
            _maxHealth = health;
            Health = _maxHealth;
            Damage = damage;
            Name = name;
            TakedDamage += ShowDamageMassage;
        }

        public event Action<int> TakedDamage;
        public event Action Attacked;

        public int Health { get; private set; }

        public string Name { get; private set; }

        public bool IsDead => Health <= 0;

        public int Damage { get; }

        public abstract void Attack(IDamageble target);

        public virtual void TakeDamage(int damage)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(damage);
            Health -= damage;
            TakedDamage?.Invoke(damage);
        }

        protected void Heal(int healValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(healValue);
            Health = Math.Min(Health + healValue, _maxHealth);
        }

        private void ShowDamageMassage(int damage)
        {
            Console.WriteLine($"{Name} получил урон в размере {damage}");
        }
    }

    public class Knight : Fighter
    {
        private readonly float _dobleDamageChance;

        public Knight(float dobleDamageChance) : base(1000, 35, "Рыцарь")
        {
            ArgumentOutOfRangeException.ThrowIfNegative(dobleDamageChance);
            _dobleDamageChance = dobleDamageChance;
        }

        public override void Attack(IDamageble target)
        {
            bool isDobleDamage = Utilits.GetBoolean(_dobleDamageChance);
            int damage = Damage;
            Console.Write($"{Name} атаковал противника ");

            if (isDobleDamage)
            {
                damage *= 2;
                Console.Write($"сильной атакой");
            }

            Console.WriteLine();
            target.TakeDamage(damage);
        }
    }

    public class Spearman : Fighter
    {
        private int _attacksCout = default;

        public Spearman() : base(1000, 45, "Копейщик")
        {

        }

        public override void Attack(IDamageble target)
        {
            _attacksCout++;
            int damage = Damage;
            Console.WriteLine($"{Name} атаковал противника");
            target.TakeDamage(damage);

            if (_attacksCout == 3)
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

        public override void Attack(IDamageble target)
        {
            int damage = Damage;
            Console.Write($"{Name} атаковал противника ");
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

        public Magican() : base(1000, 10, "Маг")
        {
            _mana = 100;
            _magicAttackCoast = 15;
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

        public override void Attack(IDamageble target)
        {
            int damage = Damage;
            Console.Write($"{Name} атаковал противника");
            target.TakeDamage(damage);
        }

        public override void TakeDamage(int damage)
        {
            bool isDoge = Utilits.GetBoolean(_dogeChance);

            if (isDoge)
            {
                Console.Write($"{Name} уклонился");
            }
            else
            {
                base.TakeDamage(damage);
            }
        }
    }

    public interface IDamageble
    {
        event Action<int> TakedDamage;

        int Health { get; }

        void TakeDamage(int damage);
    }
}
