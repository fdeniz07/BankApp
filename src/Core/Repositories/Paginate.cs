namespace Core.Repositories;

public class Paginate<T> : IPaginate<T>
{
    public int From { get; set; }
    public int Index { get; set; }
    public int Size { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
    public IList<T> Items { get; set; } = [];
    public bool HasPrevious => Index + 1 > 1;
    public bool HasNext => Index + 1 < Pages;

    public Paginate()
    {
    }

    public Paginate(IEnumerable<T> source, int index, int size, int from)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(from, index);

        Index = index;
        Size = size;
        From = from;
        Items = source is IQueryable<T> queryable
            ? [.. queryable.Skip((Index - From) * Size).Take(Size)]
            : [.. source.Skip((Index - From) * Size).Take(Size)];
        Count = source.Count();
        Pages = (int)Math.Ceiling(Count / (double)Size);
    }
}
