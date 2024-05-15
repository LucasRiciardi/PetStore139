

namespace Models;

public class PetModel
{
    public int id;
    public Category category;
    public String name;
    public String[] photUrls;
    public Tag[] tags;
    public String status;
}

public  class Category
{
    public int id;
    public String name;

    // Construtor
    public Category(int id, String name)
    {
    this.id = id;
    this.name = name;
    }
}

public class Tag
{
    public int id;
    public String name;

public Tag(int id, String name)
    {
        this.id = id;
        this.name = name;
    }


}


