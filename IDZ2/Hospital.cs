/// <summary>
/// Больница (справочная таблица, сторона "один")
/// </summary>
public class Hospital
{
    /// <summary>Идентификатор больницы</summary>
    public int Id { get; set; }

    /// <summary>Название больницы</summary>
    public string Name { get; set; }

    /// <summary>Конструктор с параметрами</summary>
    public Hospital(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Hospital() : this(0, "") { }

    public override string ToString() => $"[{Id}] {Name}";
}