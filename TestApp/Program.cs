namespace TestApp
{
    public static partial class Program
    {
        public static async ValueTask<string> Execute1()
        {
            await Task.Delay(100);
            return "Test1" + Thread.CurrentThread.Name;
        }

        public static async ValueTask<string> Execute2()
        {
            await Task.Delay(300);
            return "Test2" + Thread.CurrentThread.Name;
        }

        public static async ValueTask<string> Execute3()
        {
            await Task.Delay(200);
            return "Test3" + Thread.CurrentThread.Name;
        }

        public static async Task Main()
        {
            var l1 = Execute1();
            var l2 = Execute2();
            var l3 = Execute3();

            Console.WriteLine("Test4" + Thread.CurrentThread.Name);
            Console.WriteLine("Test5" + Thread.CurrentThread.Name);
            Console.WriteLine("Test6" + Thread.CurrentThread.Name);

            Console.WriteLine(await l1);
            Console.WriteLine(await l2);
            Console.WriteLine(await l3);
        }
    }
}