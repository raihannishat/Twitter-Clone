namespace Application.UnitTests.Block;

public static class BlockTestData
{
    public static Blocks BlockEntity = new Blocks()
    {
        BlockedById = "user1",
        BlockedId = "user2"
    };

    public static User UserEntity = new User
    {
        Id = "f1232",
        Name = "Asif",
        Email = "asif@gmai.com"
    };
}
