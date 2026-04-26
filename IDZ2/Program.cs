using System;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        string dbPath = "hospital_data.db";
        string hspCsv = Path.Combine(AppContext.BaseDirectory, "hospitals.csv");
        string docCsv = Path.Combine(AppContext.BaseDirectory, "doctors.csv");

        var db = new DatabaseManager(dbPath);
        db.InitializeDatabase(hspCsv, docCsv);

        Console.WriteLine();

        string choice;
        do
        {
            Console.WriteLine("=== УПРАВЛЕНИЕ ВРАЧАМИ И БОЛЬНИЦАМИ ===");
            Console.WriteLine("1 - Показать все больницы");
            Console.WriteLine("2 - Показать всех врачей");
            Console.WriteLine("3 - Добавить врача");
            Console.WriteLine("4 - Редактировать врача");
            Console.WriteLine("5 - Удалить врача");
            Console.WriteLine("6 - Отчёты");
            Console.WriteLine("0 - Выход");
            Console.Write("Ваш выбор: ");

            choice = Console.ReadLine()?.Trim() ?? "";
            Console.WriteLine();

            switch (choice)
            {
                case "1": ShowHospitals(db); break;
                case "2": ShowDoctors(db); break;
                case "3": AddDoctor(db); break;
                case "4": EditDoctor(db); break;
                case "5": DeleteDoctor(db); break;
                case "6": ReportsMenu(db); break;
                case "0": Console.WriteLine("До свидания!"); break;
                default: Console.WriteLine("Неверный пункт меню."); break;
            }
            Console.WriteLine();
        } while (choice != "0");
    }

    static void ShowHospitals(DatabaseManager db)
    {
        Console.WriteLine("--- Все больницы ---");
        var hospitals = db.GetAllHospitals();
        foreach (var h in hospitals)
            Console.WriteLine(" " + h);
        Console.WriteLine($"Итого: {hospitals.Count}");
    }

    static void ShowDoctors(DatabaseManager db)
    {
        Console.WriteLine("--- Все врачи ---");
        var doctors = db.GetAllDoctors();
        foreach (var d in doctors)
            Console.WriteLine(" " + d);
        Console.WriteLine($"Итого: {doctors.Count}");
    }

    static void AddDoctor(DatabaseManager db)
    {
        Console.WriteLine("--- Добавление врача ---");
        Console.WriteLine("Доступные больницы: ");
        foreach (var h in db.GetAllHospitals())
            Console.WriteLine("  " + h);

        Console.Write("ID больницы: ");
        if (!int.TryParse(Console.ReadLine(), out int hspId))
        {
            Console.WriteLine("Ошибка: введите целое число.");
            return;
        }

        Console.Write("ФИО врача: ");
        string name = Console.ReadLine()?.Trim() ?? "";
        if (name.Length == 0)
        {
            Console.WriteLine("Ошибка: имя не может быть пустым.");
            return;
        }

        Console.Write("Стаж работы (лет): ");
        if (!int.TryParse(Console.ReadLine(), out int exp))
        {
            Console.WriteLine("Ошибка: введите целое число.");
            return;
        }

        try
        {
            var doc = new Doctor(0, hspId, name, exp);
            db.AddDoctor(doc);
            Console.WriteLine("Врач добавлен.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void EditDoctor(DatabaseManager db)
    {
        Console.WriteLine("--- Редактирование врача ---");
        Console.Write("Введите ID врача: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Ошибка: введите целое число.");
            return;
        }

        var doc = db.GetDoctorById(id);
        if (doc == null)
        {
            Console.WriteLine($"Врач с ID={id} не найден.");
            return;
        }

        Console.WriteLine($"Текущие данные: {doc}");
        Console.WriteLine("(нажмите Enter, чтобы оставить значение без изменений)");

        Console.Write($"ФИО [{doc.Name}]: ");
        string input = Console.ReadLine()?.Trim() ?? "";
        if (input.Length > 0) doc.Name = input;

        Console.Write($"ID больницы [{doc.HospitalId}]: ");
        input = Console.ReadLine()?.Trim() ?? "";
        if (input.Length > 0 && int.TryParse(input, out int newHspId))
            doc.HospitalId = newHspId;

        Console.Write($"Стаж [{doc.Experience}]: ");
        input = Console.ReadLine()?.Trim() ?? "";
        if (input.Length > 0 && int.TryParse(input, out int newExp))
        {
            try
            {
                doc.Experience = newExp;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return;
            }
        }

        db.UpdateDoctor(doc);
        Console.WriteLine("Данные обновлены.");
    }

    static void DeleteDoctor(DatabaseManager db)
    {
        Console.WriteLine("--- Удаление врача ---");
        Console.Write("Введите ID врача: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Ошибка: введите целое число.");
            return;
        }

        var doc = db.GetDoctorById(id);
        if (doc == null)
        {
            Console.WriteLine($"Врач с ID={id} не найден.");
            return;
        }

        Console.Write($"Удалить «{doc.Name}»? (да/нет): ");
        string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
        if (confirm == "да")
        {
            db.DeleteDoctor(id);
            Console.WriteLine("Врач удалён.");
        }
        else
        {
            Console.WriteLine("Удаление отменено.");
        }
    }

    static void ReportsMenu(DatabaseManager db)
    {
        string choice;
        do
        {
            Console.WriteLine("--- Отчёты ---");
            Console.WriteLine("1 - Врачи по больницам");
            Console.WriteLine("2 - Количество врачей в больницах");
            Console.WriteLine("3 - Средний стаж врачей по больницам");
            Console.WriteLine("0 - Назад");
            Console.Write("Ваш выбор: ");

            choice = Console.ReadLine()?.Trim() ?? "";

            switch (choice)
            {
                case "1": Report1(db); break;
                case "2": Report2(db); break;
                case "3": Report3(db); break;
                case "0": break;
                default: Console.WriteLine("Неверный пункт."); break;
            }
            Console.WriteLine();
        } while (choice != "0");
    }

    static void Report1(DatabaseManager db)
    {
        new ReportBuilder(db)
            .Query(@"SELECT d.doc_name, h.hsp_name, d.doc_experience 
                     FROM doc d 
                     JOIN hsp h ON d.hsp_id = h.hsp_id 
                     ORDER BY d.doc_name")
            .Title("Список врачей по больницам")
            .Header("ФИО", "Больница", "Стаж (лет)")
            .ColumnWidths(25, 35, 12)
            .Print();
    }

    static void Report2(DatabaseManager db)
    {
        new ReportBuilder(db)
            .Query(@"SELECT h.hsp_name, COUNT(*) AS cnt 
                     FROM doc d 
                     JOIN hsp h ON d.hsp_id = h.hsp_id 
                     GROUP BY h.hsp_name 
                     ORDER BY h.hsp_name")
            .Title("Количество врачей в больницах")
            .Header("Больница", "Количество")
            .ColumnWidths(35, 12)
            .Print();
    }

    static void Report3(DatabaseManager db)
    {
        new ReportBuilder(db)
            .Query(@"SELECT h.hsp_name, ROUND(AVG(d.doc_experience), 1) AS avg_exp 
                     FROM doc d 
                     JOIN hsp h ON d.hsp_id = h.hsp_id 
                     GROUP BY h.hsp_name 
                     ORDER BY avg_exp DESC")
            .Title("Средний стаж врачей по больницам")
            .Header("Больница", "Средний стаж (лет)")
            .ColumnWidths(35, 20)
            .Print();
    }
}