using Xunit;

public class SimpleTest
{
    [Fact]
    public void Test1()
    {
        System.Console.WriteLine("This is a simple test output");
        Assert.True(true);
    }
}
