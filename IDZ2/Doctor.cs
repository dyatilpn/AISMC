using System;

/// <summary>
/// Врач (основная таблица, сторона "много")
/// </summary>
public class Doctor
{
    /// <summary>Идентификатор врача</summary>
    public int Id { get; set; }

    /// <summary>Идентификатор больницы (внешний ключ)</summary>
    public int HospitalId { get; set; }

    /// <summary>Имя врача</summary>
    public string Name { get; set; }

    private int _experience;

    /// <summary>
    /// Стаж работы в годах (не может быть отрицательным)
    /// </summary>
    public int Experience
    {
        get => _experience;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException("Стаж работы не может быть отрицательным");
            }
            _experience = value;
        }
    }

    /// <summary>Конструктор с параметрами</summary>
    public Doctor(int id, int hospitalId, string name, int experience)
    {
        Id = id;
        HospitalId = hospitalId;
        Name = name;
        Experience = experience;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Doctor() : this(0, 0, "", 0) { }

    public override string ToString() => $"[{Id}] {Name}, больница #{HospitalId}, стаж: {Experience} лет";
}