using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace task3_SerializationAndRefluxing
{
    [Serializable]
    class Person
    {
        public string name;
        public string surname;
        public string gender;
        private DateOnly birthDate;
        public DateOnly BirthDate
        {
            get { return birthDate; }
            set
            {
                DateOnly dateNow = DateOnly.FromDateTime(DateTime.Now);
                if (value > dateNow || dateNow.Year - value.Year > 140)
                {
                    throw new ArgumentOutOfRangeException();
                }
                else
                {
                    birthDate = value;
                }
            }
        }
        public int Age
        {
            get
            {
                int age = (int)(new DateTime(BirthDate, new TimeOnly()) - DateTime.Now).TotalDays / 365;
                return age;
            }
        }

        public Person()
        {
            name = "unknown";
            surname = "unknown";
            gender = "unknown";
            birthDate = new DateOnly(2000, 01, 01);
        }
        public Person(string name, string surname, string gender, string birthDate) :this()
        {
            this.name = name;
            this.surname = surname;
            this.gender = gender;

            DateOnly date;
            if (DateOnly.TryParse(birthDate, out date))
            {
                BirthDate = date;
            }
            
        }

        public override string ToString()
        {
            string info = $"Name: {name}.\nSurname: {surname}.\nGender: {gender}.\nBirthDate {BirthDate}.\n";
            return info;
        }
    }

    [Serializable]
    internal class User: Person
    {
        public string email;
        private string password;
        public bool is_activ = true;
        public bool is_superuser = false;
        public bool is_admin = false;

        public User() : base()
        {
            email = "unknown";
            is_activ = false;
            InstallPassword("0000");
        }
        public User(string name, string surname, string gender, string birthDate, string email, string password,bool is_active= true, bool is_superuser=false,
            bool is_admin=false): base(name, surname, gender, birthDate) 
        {
            this.email = email;
            this.is_activ = is_active;
            this.is_superuser = is_superuser;
            this.is_admin = is_admin;
            InstallPassword(password);
        }

        protected virtual string HashedPassword(string pass)
        {
            return pass.GetHashCode().ToString();
        }
        protected virtual void InstallPassword(string password)
        {
            this.password = HashedPassword(password);
        }
        public virtual bool CheckPassword(string password)
        {
            return this.password == HashedPassword(password);
        }
        public override string ToString()
        {
            string info = base.ToString();
            info += $"Email: {email}.\nIs activ: {is_activ}.\nIs superuser: {is_superuser}.\nIs admin: {is_admin}.\n";
            return info;
        }

        //Методы согласно заданию.
        public static void GetReflexToFile(string fileName, string path) 
        {
            if(!Directory.Exists(path)) { throw new DirectoryNotFoundException(path); }
            Type t = typeof(User);

            string fullPath = Path.Combine(path, fileName);
            using (StreamWriter file = new StreamWriter(fullPath))
            {
                file.WriteLine($"Имя класса: {t.Name}.");
                file.WriteLine($"полное имя класса: {t.FullName}.");
                file.WriteLine($"Пространство имен: {t.Namespace}.");
                file.WriteLine($"Базовый класс: {t.BaseType}.");
                file.WriteLine($"Абстрактный класс: {t.IsAbstract}.");
                file.WriteLine($"Перечислинение: {t.IsEnum}.");
                file.WriteLine($"Интерфейс: {t.IsInterface}.");
                file.WriteLine($"Примитив: {t.IsPrimitive}.");
                file.WriteLine($"Запечатаный класс: {t.IsSealed}.");
                file.WriteLine($"Публичный класс: {t.IsPublic}.");
                file.WriteLine($"Значимый тип: {t.IsValueType}.");

                FieldInfo[] fields = t.GetFields();
                fields = fields.Concat(t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)).ToArray();
                file.WriteLine($"\nПоля класса:");
                foreach (FieldInfo field in fields)
                {
                    file.WriteLine($"{field.Name} - Приватный: {field.IsPrivate}");
                    
                }

                PropertyInfo[] properties = t.GetProperties();
                file.WriteLine($"\nСвойства класса:");
                foreach (PropertyInfo property in properties)
                {
                    file.WriteLine($"{property.Name} - Запись: {property.CanWrite}. Чтение: {property.CanRead}");
                }
            }
        }
        public string Serialize(string fileName, string path)
        {
            if (!Directory.Exists(path)) { throw new DirectoryNotFoundException(path); }
            string fullPatch = Path.Combine(path, fileName);

            using (Stream fs = new FileStream(fullPatch, FileMode.Create))
            {
                //BinaryFormatter bf = new BinaryFormatter(); устарело и запрещено.
                //https://learn.microsoft.com/ru-ru/dotnet/fundamentals/syslib-diagnostics/syslib0011

                using(BinaryWriter bw = new BinaryWriter(fs,Encoding.UTF8,false))
                {
                    bw.Write(this.name);
                    bw.Write(this.surname);
                    bw.Write(this.gender);
                    bw.Write(this.BirthDate.ToString());
                    bw.Write(this.email);
                    bw.Write(this.password.ToString());
                    bw.Write(this.is_activ);
                    bw.Write(this.is_superuser);
                    bw.Write(this.is_admin);
                }
            }
        return fullPatch;
        }
        public User Deserialize(string path)
        {
            return DeserializeObj(path);
        }

        public static User DeserializeObj(string path)
        {
            if (!File.Exists(path)) { throw new DirectoryNotFoundException(path); }

            string Bname, Bsurname, Bgender, bBirthDate, Bemail, Bpassword;
            bool Bis_activ, Bis_superuser, Bis_admin;

            using (Stream fs = new FileStream(path, FileMode.Open))
            {
                using(BinaryReader bw =  new BinaryReader(fs, Encoding.UTF8, false))
                {
                    Bname = bw.ReadString();
                    Bsurname = bw.ReadString();
                    Bgender = bw.ReadString();
                    bBirthDate = bw.ReadString();
                    Bemail = bw.ReadString();
                    Bpassword = bw.ReadString();
                    Bis_activ = bw.ReadBoolean();
                    Bis_superuser = bw.ReadBoolean();
                    Bis_admin = bw.ReadBoolean();
                }
            }
            User obj = new User(Bname, Bsurname, Bgender, bBirthDate, Bemail, Bpassword, Bis_activ, Bis_superuser, Bis_activ);
            obj.password = Bpassword;
            return obj;
        }
        public static string SerializerObj(User obj, string fileName, string path)
        {
            return obj.Serialize(fileName, path);
        }

    }
}
