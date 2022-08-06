using System;

public class Yields : IEquatable<Yields>
{
    public int Food;
    public int Wealth;
    public int Military;
    public int Science;
    public int Culture;

    public Yields(int food = 0, int wealth = 0, int military = 0, int science = 0, int culture = 0)
    {
        Food = food;
        Wealth = wealth;
        Military = military;
        Science = science;
        Culture = culture;
    }

    public Yields(Yields model)
    {
        Food = model.Food;
        Wealth = model.Wealth;
        Military = model.Military;
        Science = model.Science;
        Culture = model.Culture;
    }

    public int Get(string yield)
    {
        switch (yield)
        {
            case "food":
                return Food;
            case "wealth":
                return Wealth;
            case "military":
                return Military;
            case "science":
                return Science;
            case "culture":
                return Culture;
            default:
                throw new Exception("Invalid Yield Get: " + yield);
        }
    }

    public int Sum()
    {
        return Food + Wealth + Military + Science + Culture;
    }

    public override bool Equals(object obj) => this.Equals(obj as Yields);

    public bool Equals(Yields p)
    {
        if (p is null)
            return false;

        // Optimization for a common success case.
        if (Object.ReferenceEquals(this, p))
            return true;

        // If run-time types are not exactly the same, return false.
        if (this.GetType() != p.GetType())
            return false;
        
        return (Food == p.Food) && (Wealth == p.Wealth) && (Military == p.Military) && (Science == p.Science) && (Culture == p.Culture);
    }
    
    public static Yields operator +(Yields a, Yields b)
        => new Yields(a.Food + b.Food, a.Wealth + b.Wealth, a.Military + b.Military, a.Science + b.Science, a.Culture + b.Culture);

    public static bool operator ==(Yields lhs, Yields rhs)
    {
        if (lhs is null)
        {
            if (rhs is null)
                return true;

            return false;
        }
        return lhs.Equals(rhs);
    }
    public static bool operator !=(Yields lhs, Yields rhs) => !(lhs == rhs);

    public override int GetHashCode()
    {
        return Tuple.Create(Food, Wealth, Military, Science, Culture).GetHashCode();
    }
}