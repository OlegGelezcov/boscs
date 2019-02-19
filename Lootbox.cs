using System.Linq;

public class Lootbox
{
    public LootboxItem[] Items { get; set; }

    public LootboxItem this[int i]
    {
        get
        {
            if (i < 0 || i >= Items.Length)
                return null;

            return Items[i];
        }
        set { Items[i] = value; }
    }

    public bool IsValid { get { return Items.All(x => x != null); } }

    public Lootbox(int size)
    {
        Items = new LootboxItem[size];
    }
}
